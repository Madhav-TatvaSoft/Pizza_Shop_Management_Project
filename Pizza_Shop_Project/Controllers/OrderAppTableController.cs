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
    private readonly ICustomerService _customerService;

    public OrderAppTableController(IUserService userService, IUserLoginService userLoginService, IOrderAppTableService orderAppTableService, ICustomerService customerService)
    {
        _userService = userService;
        _userLoginService = userLoginService;
        _orderAppTableService = orderAppTableService;
        _customerService = customerService;
    }

    public async Task<IActionResult> OrderAppTable()
    {
        ViewData["orderApp-Active"] = "Table";
        ViewData["Icon"] = "fa-table";
        return View();
    }

    #region List
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
    #endregion

    #region GET
    public IActionResult WaitingTokenDetails(long sectionid, string sectionName)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.waitingTokenDetailViewModel = new();
        TableMainVM.waitingTokenDetailViewModel.SectionId = sectionid;
        TableMainVM.waitingTokenDetailViewModel.SectionName = sectionName;
        return PartialView("_WaitingListModal", TableMainVM);
    }

    public IActionResult GetCustomerEmail(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return Json(new List<object>());
        }

        var Emails = _customerService.GetCustomerEmail(searchTerm);

        return Json(Emails);
    }

    public async Task<IActionResult> GetWaitingListAndCustomerDetails(long sectionid, string sectionName)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.WaitingTokenVMList = await _orderAppTableService.GetWaitingCustomerList(sectionid);
        TableMainVM.SectionId = sectionid;
        TableMainVM.SectionName = sectionName;
        return PartialView("_AssignTableCanvas", TableMainVM);
    }

    public async Task<IActionResult> GetCustomerDetails(long waitingId, long sectionId, string sectionName)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        if (waitingId == 0)
        {
            TableMainVM.waitingTokenDetailViewModel = new();
            TableMainVM.waitingTokenDetailViewModel.SectionId = sectionId;
            TableMainVM.waitingTokenDetailViewModel.SectionName = sectionName;
            return PartialView("_CustomerDetails", TableMainVM);
        }
        TableMainVM.waitingTokenDetailViewModel = await _orderAppTableService.GetCustomerDetails(waitingId);
        return PartialView("_CustomerDetails", TableMainVM);
    }

    public async Task<IActionResult> GetCustomerIdByTableId(long tableId)
    {
        long CustomerId = await _customerService.GetCustomerIdByTableId(tableId);
        return Json(new { customerId = CustomerId });
    }

    #endregion

    #region POST

    [HttpPost]
    public async Task<IActionResult> WaitingTokenDetails([FromForm] OrderAppTableMainViewModel TableMainVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);
        bool tokenExists = _orderAppTableService.CheckTokenExists(TableMainVM.waitingTokenDetailViewModel);
        if (tokenExists)
        {
            return Json(new {success = false, text = "Token Already Exists !"});
        }
        else
        {
            // long customerIdIfPresent = _customerService.IsCustomerPresent(TableMainVM.waitingTokenDetailViewModel.Email);

            // if (customerIdIfPresent == 0)
            // {
            bool createCustomer = await _customerService.SaveCustomer(TableMainVM.waitingTokenDetailViewModel, userId);

            if (!createCustomer)
            {
                return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
            }
            // }

            bool IsCustomerAddedToWaiting = await _orderAppTableService.AddCustomerToWaitingList(TableMainVM.waitingTokenDetailViewModel, userId);

            if (IsCustomerAddedToWaiting)
            {
                return Json(new { success = true, text = "Customer Added In Waiting List" });
            }
            return Json(new { success = false, text = "Error While Adding Customer to waiting List. Try Again!" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AssignTable([FromForm] OrderAppTableMainViewModel TableMainVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        bool createCustomer = await _customerService.SaveCustomer(TableMainVM.waitingTokenDetailViewModel, userId);

        if (!createCustomer)
        {
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
        }

        long customerId = await _customerService.GetCustomerIdByTableId(TableMainVM.TableIds[0]);

        bool TableAssignStatus = await _orderAppTableService.AssignTable(TableMainVM, userId);
        if (TableAssignStatus)
        {
            return Json(new { success = true, text = "Table Assigned", customerid = customerId });
        }
        return Json(new { success = false, text = "Something Went wrong, Try Again!" });
    }

    #endregion

}