using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("AccountManager")]
public class OrderAppWaitingListController : Controller
{
    private readonly IOrderAppTableService _orderAppTableService;

    public OrderAppWaitingListController(IOrderAppTableService orderAppTableService){
        _orderAppTableService = orderAppTableService;
    }
    public IActionResult OrderAppWaitingList()
    {
        OrderAppWaitingViewModel WaitingVM = new();
        WaitingVM.sectionVMList = _orderAppTableService.GetAllSectionList();
        ViewData["orderApp-Active"] = "WaitingList";
        ViewData["Icon"] = "fa-clock";
        return View(WaitingVM);
    }
}