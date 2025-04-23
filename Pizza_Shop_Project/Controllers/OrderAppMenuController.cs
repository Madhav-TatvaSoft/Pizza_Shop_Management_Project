using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> OrderAppMenu()
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.categoryList = await _categoryService.GetAllCategories();
        ViewData["orderApp-Active"] = "Menu";
        ViewData["Icon"] = "fa-burger";
        return View(OrderAppMenuVM);
    }

    public IActionResult GetItems(long categoryid, string searchText = "")
    {
        OrderAppMenuViewModel OrderAppMenuVM = new();
        OrderAppMenuVM.itemList = _orderAppMenuService.GetItems(categoryid, searchText);
        return PartialView("_ItemCardsPartial", OrderAppMenuVM.itemList);
    }

    #region FavouriteItemManage
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
    #endregion
}
