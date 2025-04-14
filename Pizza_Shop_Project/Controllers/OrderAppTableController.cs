using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;


[PermissionAuthorize("AccountManager")]
public class OrderAppTableController : Controller
{
    private readonly ITableSectionService _sectionService;
    private readonly IOrderAppTableService _orderAppTableService;
    public OrderAppTableController(ITableSectionService sectionService, IOrderAppTableService orderAppTableService)
    {
        _sectionService = sectionService;
        _orderAppTableService = orderAppTableService;
    }

    public async Task<IActionResult> OrderAppTable()
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.sectionListVM = _orderAppTableService.GetAllSectionList();
        ViewData["orderApp-Active"] = "Table";
        ViewData["Icon"] = "fa-table";
        return View(TableMainVM);
    }

    public async Task<IActionResult> GetTablesBySection(long sectionid){
        var tableList = _orderAppTableService.GetTablesBySection(sectionid);
        return PartialView("_TableList",tableList);
    }

}
