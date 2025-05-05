using BLL.Interface;
using DAL.Models;
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
    private readonly IUserService _userService;
    private readonly IUserLoginService _userLoginService;

    public OrderAppMenuController(ICategoryService categoryService, IOrderAppMenuService orderAppMenuService, IUserService userService, IUserLoginService userLoginService)
    {
        _userService = userService;
        _userLoginService = userLoginService;
        _categoryService = categoryService;
        _orderAppMenuService = orderAppMenuService;
    }

    public async Task<IActionResult> OrderAppMenu(long customerId = 0)
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.categoryList = await _categoryService.GetAllCategories();
        ViewData["orderApp-Active"] = "Menu";
        ViewData["Icon"] = "fa-burger";

        OrderAppMenuVM.customerId = customerId;
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

    public IActionResult GetModifiersByItemId(long itemId, long customerId)
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.modifirsByItemList = new List<ItemModifierViewModel>();
        OrderAppMenuVM.customerId = customerId;
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
        OrderDetailViewModel orderDetailVM = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        OrderDetailViewModel orderDetailsVM = await _orderAppMenuService.UpdateOrderDetailPartialView(itemList, orderDetailVM);

        return PartialView("_MenuItemsOrderDetailPartial", orderDetailsVM);
    }

    public async Task<IActionResult> RemoveItemfromOrderDetailPartialView(string ItemList, int count, string orderDetails)
    {
        List<List<int>> itemList = JsonConvert.DeserializeObject<List<List<int>>>(ItemList);
        OrderDetailViewModel orderDetailVM = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        OrderDetailViewModel orderDetailsVM = await _orderAppMenuService.RemoveItemfromOrderDetailPartialView(itemList, count, orderDetailVM);

        return PartialView("_MenuItemsOrderDetailPartial", orderDetailsVM);
    }

    public async Task<IActionResult> UpdateCustomerDetails([FromForm] OrderDetailViewModel orderDetailVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        OrderDetailViewModel? data = await _orderAppMenuService.UpdateCustomerDetails(orderDetailVM, userId);
        if (data != null)
        {
            return Json(new { success = true, text = "Customer Details Updated Successfully", data });
        }
        else
        {
            return Json(new { success = false, text = "Something Went Wrong! Try Again!" });
        }
    }

    public async Task<IActionResult> UpdateOrderComment([FromForm] OrderDetailViewModel orderDetailVM)
    {

        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        OrderDetailViewModel? data = await _orderAppMenuService.UpdateOrderComment(orderDetailVM, userId);

        if (data != null)
        {
            return Json(new { success = true, text = "Order Comment Updated Successfully", data });
        }
        else
        {
            return Json(new { success = false, text = "Something Went Wrong! Try Again!" });
        }
    }

    public async Task<IActionResult> SaveOrder(string orderDetailIds, string orderDetails)
    {
        List<int> orderDetailId = JsonConvert.DeserializeObject<List<int>>(orderDetailIds);
        OrderDetailViewModel orderDetailVM = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        OrderDetailViewModel orderDetailsVM = await _orderAppMenuService.SaveOrder(orderDetailId, orderDetailVM);

        return PartialView("_MenuItemsOrderDetailPartial", orderDetailsVM);
    }

    public async Task<IActionResult> CompleteOrder(string orderDetailIds, string orderDetails)
    {
        List<int>? orderDetailId = JsonConvert.DeserializeObject<List<int>>(orderDetailIds);
        OrderDetailViewModel? orderDetailsVM = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
        bool IsItemsReady = await _orderAppMenuService.IsItemsReady(orderDetailId, orderDetailsVM);
        if (IsItemsReady)
        {
            bool orderDetail = await _orderAppMenuService.CompleteOrder(orderDetailsVM);
            if(orderDetail)
            {
                return Json(new { success = true, text = "Order Completed Successfully" });
            }
            else
            {
                return Json(new { success = false, text = "Something Went Wrong! Try Again!" });
            }
        }
        else
        {
            return Json(new { success = false, text = "Items are not ready yet !"});
        }
    }
















































    // public async Task<IActionResult> SaveRatings(long customerId, int foodreview, int serviceReview, int ambienceReview, string reviewtext)
    // {
    //     long ratingId = await _orderAppMenuService.SaveRatings(customerId, foodreview, serviceReview, ambienceReview, reviewtext);
    //     return Json(ratingId);
    // }
    // public async Task<IActionResult> CompleteOrder(string orderDetails, long paymentmethodId)
    // {
    //     OrderDetailViewModel? orderDetailVM = JsonConvert.DeserializeObject<OrderDetailViewModel>(orderDetails);
    //     OrderDetailViewModel orderDetailsVM = await _orderAppMenuService.CompleteOrder(orderDetailVM, paymentmethodId);
    //     return PartialView("_MenuItemsOrderDetailPartial", orderDetailsVM);
    // }




}