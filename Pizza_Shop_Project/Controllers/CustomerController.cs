using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    public IActionResult Customer()
    {
        PaginationViewModel<CustomerViewModel>? orderList = _customerService.GetCustomerList();
        ViewData["sidebar-active"] = "Order";
        return View(orderList);
    }
    public IActionResult PaginationForCustomer(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "")
    {
        PaginationViewModel<CustomerViewModel>? customerList = _customerService.GetCustomerList(search, sortColumn, sortDirection, pageNumber, pageSize, fromDate, toDate, selectRange);
        return PartialView("_CustomerListPartial", customerList);
    }

    public async Task<IActionResult> ExportCustomerDataToExcel(string search = "",string selectRange = "")
    {
        byte[]? FileData = await _customerService.ExportData(search, selectRange);
        return File(FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Customers.xlsx");
    }

}
