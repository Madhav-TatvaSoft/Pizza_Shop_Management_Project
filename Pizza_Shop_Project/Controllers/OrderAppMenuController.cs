using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("AccountManager")]
public class OrderAppMenuController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IOrderAppMenuService _orderAppMenuService;

    public OrderAppMenuController(ICategoryService categoryService, IOrderAppMenuService orderAppMenuService)
    {
        _categoryService = categoryService;
        _orderAppMenuService = orderAppMenuService;
    }
    public async Task<IActionResult> OrderAppMenu(long customerId = 0)
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.categoryList = await _categoryService.GetAllCategories();
        ViewData["orderApp-Active"] = "Menu";
        ViewData["Icon"] = "fa-burger";

        ViewData["customerId"] = customerId;
        if (customerId != 0)
        {
            // OrderAppMenuVM.orderdetails= GetOrderDetailsBycustId(customerId);
        }

        return View(OrderAppMenuVM);
    }

    public IActionResult GetItems(long categoryid, string searchText = "")
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.itemList = _orderAppMenuService.GetItems(categoryid, searchText);
        return PartialView("_ItemCardsPartial", OrderAppMenuVM.itemList);
    }

    public async Task<IActionResult> FavouriteItem(long itemId, bool IsFavourite)
    {
        bool status = await _orderAppMenuService.FavouriteItem(itemId, IsFavourite);
        if (status)
        {
            if (IsFavourite)
            {
                return Json(new { success = true, text = "Marked as Favourites" });
            }
            else
            {
                return Json(new { success = true, text = "Removed from Favourites" });
            }
        }
        else
        {
            return Json(new { success = false, text = "Something Went Wrong! Try Again!" });
        }
    }

    public IActionResult GetModifiersByItemId(long itemId)
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.modifirsByItemList = new List<ItemModifierViewModel>();
        OrderAppMenuVM.modifirsByItemList = _orderAppMenuService.GetModifiersByItemId(itemId);
        return PartialView("_ModifiersByItemModalPartial", OrderAppMenuVM);
    }

    public IActionResult GetOrderDetailsByCustomerId(long customerId)
    {
        OrderDetailViewModel orderDetailVM = new();
        orderDetailVM = _orderAppMenuService.GetOrderDetailsByCustomerId(customerId);
        return PartialView("_MenuItemsOrderDetailPartial", orderDetailVM);
    }

    public async Task<IActionResult> UpdateOrderDetailPartialView(string ItemList, string orderDetails)
    {
        List<List<int>> itemList = JsonConvert.DeserializeObject<List<List<int>>>(ItemList);
        OrderDetailViewModel orderDetailvm = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        OrderDetailViewModel orderDetailsvm = await _orderAppMenuService.UpdateOrderDetailPartialView(itemList, orderDetailvm);

        return PartialView("_MenuItemsOrderDetailPartial", orderDetailsvm);
    }

    public async Task<IActionResult> RemoveItemfromOrderDetailPartialView(string ItemList, int count, string orderDetails)
    {
        List<List<int>> itemList = JsonConvert.DeserializeObject<List<List<int>>>(ItemList);
        OrderDetailViewModel orderDetailvm = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        OrderDetailViewModel orderDetailsvm = await _orderAppMenuService.RemoveItemfromOrderDetailPartialView(itemList, count, orderDetailvm);

        return PartialView("_MenuItemsOrderDetailPartial", orderDetailsvm);
    }

}
























































































// #region UpdateOrderDetailPartialView
// public async Task<IActionResult> UpdateOrderDetailPartialView(string ItemList, string orderDetails){
//     List<List<int>> itemList = JsonConvert.DeserializeObject<List<List<int>>>(ItemList);
//     OrderDetaIlsInvoiceViewModel orderDetailvm = JsonConvert.DeserializeObject<OrderDetaIlsInvoiceViewModel>(orderDetails);
//     OrderDetaIlsInvoiceViewModel orderDetailsvm =await _orderAppMenuService.UpdateOrderDetailPartialView(itemList,orderDetailvm );

//     return PartialView("_MenuItemsWithOrderDetails",orderDetailsvm);
// }
// #endregion

// #region RemoveItemfromOrderDetailPartialView
// public async Task<IActionResult> RemoveItemfromOrderDetailPartialView(string ItemList, int count, string orderDetails){
//     List<List<int>> itemList = JsonConvert.DeserializeObject<List<List<int>>>(ItemList);
//     OrderDetaIlsInvoiceViewModel orderDetailvm = JsonConvert.DeserializeObject<OrderDetaIlsInvoiceViewModel>(orderDetails);
//     OrderDetaIlsInvoiceViewModel orderDetailsvm =await _orderAppMenuService.RemoveItemfromOrderDetailPartialView(itemList, count ,orderDetailvm);

//     return PartialView("_MenuItemsWithOrderDetails",orderDetailsvm);
// }
// #endregion