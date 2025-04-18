using BLL.common;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("AccountManager")]
public class OrderAppWaitingListController : Controller
{
    private readonly IOrderAppTableService _orderAppTableService;
    private readonly IOrderAppWaitingListService _orderAppWaitingListService;
    private readonly IUserService _userService;
    private readonly IUserLoginService _userLoginService;
    private readonly ICustomerService _customerService;

    public OrderAppWaitingListController(IOrderAppTableService orderAppTableService, IOrderAppWaitingListService orderAppWaitingListService, IUserService userService, IUserLoginService userLoginService, ICustomerService customerService)
    {
        _orderAppTableService = orderAppTableService;
        _orderAppWaitingListService = orderAppWaitingListService;
        _userService = userService;
        _userLoginService = userLoginService;
        _customerService = customerService;

    }
    public IActionResult OrderAppWaitingList()
    {

        ViewData["orderApp-Active"] = "WaitingList";
        ViewData["Icon"] = "fa-clock";
        return View();
    }

    public IActionResult GetSectionList()
    {
        OrderAppWaitingViewModel WaitingVM = new();
        WaitingVM.sectionVMList = _orderAppTableService.GetAllSectionList();
        return PartialView("_SectionListInWaiting", WaitingVM);
    }

    public IActionResult GetWaitingList(long sectionid)
    {
        OrderAppWaitingViewModel WaitingVM = new();
        // WaitingVM.WaitingTokenVMList = new List<WaitingTokenDetailViewModel>();
        WaitingVM.WaitingTokenVMList = _orderAppWaitingListService.GetWaitingList(sectionid);
        return PartialView("_WaitingListPartial", WaitingVM);

    }

    public IActionResult GetWaitingToken(long waitingid)
    {
        OrderAppWaitingViewModel WaitingVM = new();
        WaitingVM.sectionVMList = _orderAppTableService.GetAllSectionList();

        if (waitingid == 0)
        {
            WaitingVM.WaitingTokenDetailVM = new();
        }
        else
        {
            WaitingVM.WaitingTokenDetailVM = _orderAppWaitingListService.GetWaitingToken(waitingid);
        }

        return PartialView("_AddEditWaitingTokenPartial", WaitingVM);
    }

    [HttpPost]
    public async Task<IActionResult> SaveWaitingToken([FromForm] OrderAppWaitingViewModel WaitingVM)
    {

        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        // Check Customer Present If not Then Add
        bool createCustomer = await _customerService.AddEditCustomer(WaitingVM.WaitingTokenDetailVM, userId);

        if (!createCustomer)
        {
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
        }

        // Add Customer to Waiting List
        bool customerAddToWaitingList = await _orderAppTableService.AddCustomerToWaitingList(WaitingVM.WaitingTokenDetailVM, userId);

        if (customerAddToWaitingList)
        {
            return Json(new { success = true, text = "Customer Added In Waiting List" });
        }
        else
        {
            return Json(new { success = false, text = "Error While Adding Customer to waiting List. Try Again!" });
        }
    }

}









// function ValidateDates() {
//             var today = new Date().toISOString().split("T")[0];
//             $("#customerFromDate, #customerToDate").attr("max", today);

//             $("#customerFromDate").on("change", function () {
//                 var fromDate = $(this).val();
//                 $("#customerToDate").attr("min", fromDate);
//             });

//             $("#customerToDate").on("change", function () {
//                 var toDate = $(this).val();
//                 $("#customerFromDate").attr("max", toDate);
//             });
//         }