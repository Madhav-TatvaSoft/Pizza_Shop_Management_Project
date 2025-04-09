using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class OrderAppKOTController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IOrderAppKOTService _orderAppKOTService;
    public OrderAppKOTController(ICategoryService categoryService, IOrderAppKOTService orderAppKOTService)
    {
        _categoryService = categoryService;
        _orderAppKOTService = orderAppKOTService;
    }

    public async Task<IActionResult> OrderAppKOT()
    {
        OrderAppKOTViewModel KOTVM = new();
        KOTVM.categoryList = await _categoryService.GetAllCategories();
        ViewData["orderApp-Active"] = "KOT";
        ViewData["Icon"] = "fa-clipboard";
        return View(KOTVM);
    }

    public async Task<IActionResult> GetKOTItems(long catid, string filter)
    {
        List<OrderAppKOTViewModel>? KOTVM = await _orderAppKOTService.GetKOTItems(catid, filter);
        return PartialView("_KOTItemsPartial", KOTVM);
    }

    public async Task<IActionResult> GetKOTItemsFromModal(long catid, string filter,long orderid)
    {
        OrderAppKOTViewModel KOTVM = await _orderAppKOTService.GetKOTItemsFromModal(catid, filter,orderid);
        return PartialView("_KOTModalDataPartial", KOTVM);
    }
}