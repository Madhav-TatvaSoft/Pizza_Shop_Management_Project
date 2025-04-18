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

    #region Constructor
    public OrderAppTableController(IUserService userService, IUserLoginService userLoginService, IOrderAppTableService orderAppTableService, ICustomerService customerService)
    {
        _userService = userService;
        _userLoginService = userLoginService;
        _orderAppTableService = orderAppTableService;
        _customerService = customerService;
    }
    #endregion

    #region Main View
    public async Task<IActionResult> OrderAppTable()
    {
        ViewData["orderApp-Active"] = "Table";
        ViewData["Icon"] = "fa-table";
        return View();
    }
    #endregion

    #region Section List
    public async Task<IActionResult> GetAllSectionList()
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.sectionListVM = _orderAppTableService.GetAllSectionList();
        return PartialView("_SectionList", TableMainVM);
    }
    #endregion

    #region Table List
    public async Task<IActionResult> GetTablesBySection(long SectionId)
    {
        List<OrderAppTableVM>? tableList = _orderAppTableService.GetTablesBySection(SectionId);
        return PartialView("_TableList", tableList);
    }
    #endregion

    #region WaitingTokenDetail GET
    public IActionResult WaitingTokenDetails(long sectionid, string sectionName)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.waitingTokenDetailViewModel = new();
        TableMainVM.waitingTokenDetailViewModel.SectionId = sectionid;
        TableMainVM.waitingTokenDetailViewModel.SectionName = sectionName;
        return PartialView("_WaitingListModal", TableMainVM);
    }
    #endregion

    #region Get Customer Email
    public IActionResult GetCustomerEmail(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return Json(new List<object>());
        }

        var Emails = _customerService.GetCustomerEmail(searchTerm);

        return Json(Emails);
    }
    #endregion

    #region WaitingTokenDetail POST
    [HttpPost]
    public async Task<IActionResult> WaitingTokenDetails([FromForm] OrderAppTableMainViewModel TableMainVM)
    {
        string token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        // long customerIdIfPresent = _customerService.IsCustomerPresent(TableMainVM.waitingTokenDetailViewModel.Email);

        // if (customerIdIfPresent == 0)
        // {
        bool createCustomer = await _customerService.AddEditCustomer(TableMainVM.waitingTokenDetailViewModel, userId);

        if (!createCustomer)
        {
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Customer") });
        }
        // }

        bool customerAddToWaitingList = await _orderAppTableService.AddCustomerToWaitingList(TableMainVM.waitingTokenDetailViewModel, userId);

        if (customerAddToWaitingList)
        {
            return Json(new { success = true, text = "Customer Added In Waiting List" });
        }
        return Json(new { success = false, text = "Error While Adding Customer to waiting List. Try Again!" });
    }

    #endregion

    #region AssignTable GET
    public async Task<IActionResult> GetWaitingListAndCustomerDetails(long sectionid)
    {
        OrderAppTableMainViewModel TableMainVM = new();
        TableMainVM.WaitingTokenVMList = await _orderAppTableService.GetWaitingCustomerList(sectionid);
        return PartialView("_AssignTableCanvas", TableMainVM);
    }
    #endregion

    // #region AssignTable POST
    // [HttpPost]
    // public async Task<IActionResult> AssignTable(string Email, int [] TableIds){
    //    string token = Request.Cookies["AuthToken"];
    //    List<User>? userData = _userService.getUserFromEmail(token);
    //    long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);
    //    bool tableAssignStatus =await _orderAppTableService.Assigntable(Email, TableIds, userId);
    //    if(tableAssignStatus){
    //        return Json(new{ success = true, text = "Table Assigned "});
    //       }
    //    return Json(new { success = false, text = "Something Went wrong, Try Again!" });
    //}
    // #endregion


}

























