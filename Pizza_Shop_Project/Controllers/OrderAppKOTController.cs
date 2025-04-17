using BLL.common;
using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("KOT")]

public class OrderAppKOTController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IOrderAppKOTService _orderAppKOTService;

    #region Constructor
    public OrderAppKOTController(ICategoryService categoryService, IOrderAppKOTService orderAppKOTService)
    {
        _categoryService = categoryService;
        _orderAppKOTService = orderAppKOTService;
    }
    #endregion

    #region Main KOT View
    public async Task<IActionResult> OrderAppKOT()
    {
        OrderAppKOTViewModel KOTVM = new();
        KOTVM.categoryList = await _categoryService.GetAllCategories();
        ViewData["orderApp-Active"] = "KOT";
        ViewData["Icon"] = "fa-clipboard";
        return View(KOTVM);
    }
    #endregion

    #region Get KOT Items
    public async Task<IActionResult> GetKOTItems(long catid, string filter, int page = 1, int pageSize = 5)
    {
        PaginationViewModel<OrderAppKOTViewModel>? KOTVM = await _orderAppKOTService.GetKOTItems(catid, filter, page, pageSize);
        return PartialView("_KOTItemsPartial", KOTVM);
    }
    #endregion

    #region KOT Items Modal
    public async Task<IActionResult> GetKOTItemsFromModal(long catid, string filter, long orderid)
    {
        var KOTVM = await _orderAppKOTService.GetKOTItemsFromModal(catid, filter, orderid);
        return PartialView("_KOTModalDataPartial", KOTVM);
    }
    #endregion

    #region Update KOT Status
    public async Task<IActionResult> UpdateKOTStatus(string filter, int[] orderDetailId, int[] quantity)
    {
        bool UpdateStatus = await _orderAppKOTService.UpdateKOTStatus(filter, orderDetailId, quantity);
        if (UpdateStatus == true)
        {
            return Json(new { success = true, text = NotificationMessage.EntityUpdated.Replace("{0}", "Item Status") });
        }
        return Json(new { success = false, text = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "Item Status") });
    }
    #endregion

}