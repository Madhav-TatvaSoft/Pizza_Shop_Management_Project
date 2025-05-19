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

    #region GET
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
    #endregion

    #region Waiting Token CRUD

    [HttpPost]
    public async Task<IActionResult> SaveWaitingToken([FromForm] OrderAppWaitingViewModel WaitingVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);


        bool tokenExists = _orderAppTableService.CheckTokenExists(WaitingVM.WaitingTokenDetailVM);
        bool AlreadyAssigned = await _orderAppWaitingListService.AlreadyAssigned(WaitingVM.WaitingTokenDetailVM.CustomerId);

        if (tokenExists)
        {
            return Json(new { success = false, text = "Token Already Exists!" });
        }
        else if (AlreadyAssigned)
        {
            return Json(new { success = false, text = "Customer Already Assigned!" });
        }
        else
        {
            // Check Customer Present If not Then Add
            bool createCustomer = await _customerService.SaveCustomer(WaitingVM.WaitingTokenDetailVM, userId);

            if (!createCustomer)
            {
                return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
            }

            // Add Customer to Waiting List
            bool IsCustomerAddedToWaiting = await _orderAppTableService.AddCustomerToWaitingList(WaitingVM.WaitingTokenDetailVM, userId);

            if (IsCustomerAddedToWaiting)
            {
                return Json(new { success = true, text = "Customer Added In Waiting List" });
            }
            else
            {
                return Json(new { success = false, text = "Something went wrong, Please try again!" });
            }
        }


    }

    [HttpPost]
    public async Task<IActionResult> DeleteWaitingToken(long waitingid)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        bool deleteTokenStatus = await _orderAppWaitingListService.DeleteWaitingToken(waitingid, userId);
        if (deleteTokenStatus)
        {
            return Json(new { success = true, text = NotificationMessage.EntityDeleted.Replace("{0}", "Waiting Token") });
        }
        return Json(new { success = false, text = NotificationMessage.EntityDeletedFailed.Replace("{0}", "Waiting Token") });
    }

    #endregion

    #region Assign Table
    public IActionResult GetAssignTableModal(long sectionid)
    {
        OrderAppWaitingViewModel WaitingVM = new();
        WaitingVM.tableVMList = _orderAppWaitingListService.GetAvailableTables(sectionid);
        return PartialView("_AssignTableModal", WaitingVM);
    }

    [HttpPost]
    public async Task<IActionResult> AssignTableInWaiting(long waitingId, long sectionId, long customerid, int persons, int[] tableIds)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        bool TableAssignStatus = await _orderAppWaitingListService.AssignTableInWaiting(waitingId, sectionId, customerid, persons, tableIds, userId);
        if (TableAssignStatus)
        {
            return Json(new { success = true, text = "Table Assigned Successfully" });
        }
        return Json(new { success = false, text = "Something Went wrong, Try Again!" });
    }

    #endregion

}