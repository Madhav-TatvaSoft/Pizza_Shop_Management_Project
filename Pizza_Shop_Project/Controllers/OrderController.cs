using BLL.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    public IActionResult Order()
    {
        ViewData["sidebar-active"] = "Order";
        return View();
    }
    public IActionResult PaginationForOrder(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string orderStatus = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        var orderList = _orderService.GetOrderList(search, sortColumn, sortDirection, pageNumber, pageSize, orderStatus, fromDate, toDate, selectRange);
        return PartialView("_OrderListDataPartial", orderList);
    }

    public async Task<IActionResult> ExportOrderDataToExcel(string search = "", string orderStatus = "", string selectRange = ""){
        var FileData = await _orderService.ExportData(search, orderStatus, selectRange);
        return File(FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
    }
}