using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    #region Constructor
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    #endregion

    #region Customer Main View
    [PermissionAuthorize("Customers.View")]
    public IActionResult Customer()
    {
        PaginationViewModel<CustomerViewModel>? customerList = _customerService.GetCustomerList();
        ViewData["sidebar-active"] = "Customer";
        return View(customerList);
    }
    #endregion

    #region Pagination
    [PermissionAuthorize("Customers.View")]
    public IActionResult PaginationForCustomer(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "")
    {
        PaginationViewModel<CustomerViewModel>? customerList = _customerService.GetCustomerList(search, sortColumn, sortDirection, pageNumber, pageSize, fromDate, toDate, selectRange);
        return PartialView("_CustomerListPartial", customerList);
    }
    #endregion

    #region Export Excel
    public async Task<IActionResult> ExportCustomerDataToExcel(string search = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        byte[]? FileData = await _customerService.ExportData(search, fromDate, toDate, selectRange);
        return File(FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customers.xlsx");
    }
    #endregion

    #region Get Customer History
    public IActionResult GetCustomerHistory(long customerid){
        var customerhistory = _customerService.GetCustomerHistory(customerid);
        return PartialView("_CustomerHistoryPartial",customerhistory);
    }
    #endregion

}