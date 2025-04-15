using BLL.common;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;


[PermissionAuthorize("AccountManager")]
public class OrderAppTableController : Controller
{
    private readonly IOrderAppTableService _orderAppTableService;
    private readonly IUserService _userService;
    private readonly IUserLoginService _userLoginService;
    public OrderAppTableController(IUserService userService, IUserLoginService userLoginService, IOrderAppTableService orderAppTableService)
    {
        _userService = userService;
        _userLoginService = userLoginService;
        _orderAppTableService = orderAppTableService;
    }

    public async Task<IActionResult> OrderAppTable()
    {
        ViewData["orderApp-Active"] = "Table";
        ViewData["Icon"] = "fa-table";
        return View();
    }

    public async Task<IActionResult> GetAllSectionList()
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.sectionListVM = _orderAppTableService.GetAllSectionList();
        return PartialView("_SectionList", TableMainVM);
    }

    public async Task<IActionResult> GetTablesBySection(long SectionId)
    {
        List<OrderAppTableVM>? tableList = _orderAppTableService.GetTablesBySection(SectionId);
        return PartialView("_TableList", tableList);
    }

    public IActionResult WaitingTokenDetails(long sectionid, string sectionName)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.waitingTokenDetailViewModel = new();
        TableMainVM.waitingTokenDetailViewModel.SectionId = sectionid;
        TableMainVM.waitingTokenDetailViewModel.SectionName = sectionName;
        return PartialView("_WaitingListModal", TableMainVM);
    }

    [HttpPost]
    public async Task<IActionResult> WaitingTokenDetails(OrderAppTableMainViewModel TableMainVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        long customerIdIfPresent = _orderAppTableService.IsCustomerPresent(TableMainVM.waitingTokenDetailViewModel.Email);
        if (customerIdIfPresent == 0)
        {
            bool createCustomer = await _orderAppTableService.AddCustomer(TableMainVM.waitingTokenDetailViewModel, userId);
            if (!createCustomer)
            {
                return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
            }
        }
        bool customerAddToWaitingList = await _orderAppTableService.AddCustomerToWaitingList(TableMainVM.waitingTokenDetailViewModel, userId);
        if (customerAddToWaitingList)
        {
            return Json(new { success = true, text = "Customer Added In Waiting List" });
        }
        return Json(new { success = false, text = "Error While Adding Customer to waiting List. Try Again!" });
    }


}