using BLL.Interface;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace Pizza_Shop_Project.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    #region Constructor
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    #endregion

    #region Order Main View
    public IActionResult Order()
    {
        PaginationViewModel<OrdersViewModel>? orderList = _orderService.GetOrderList();
        ViewData["sidebar-active"] = "Order";
        return View(orderList);
    }
    #endregion

    #region Pagination Order
    public IActionResult PaginationForOrder(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string orderStatus = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        PaginationViewModel<OrdersViewModel>? orderList = _orderService.GetOrderList(search, sortColumn, sortDirection, pageNumber, pageSize, orderStatus, fromDate, toDate, selectRange);
        return PartialView("_OrderListDataPartial", orderList);
    }
    #endregion

    #region Export Excel
    public async Task<IActionResult> ExportOrderDataToExcel(string search = "", string orderStatus = "", string selectRange = "")
    {
        byte[]? FileData = await _orderService.ExportData(search, orderStatus, selectRange);
        return File(FileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
    }
    #endregion

    #region Order Details View
    public async Task<IActionResult> OrderDetails(long orderid)
    {
        OrderDetailViewModel? orderDetails = await _orderService.GetOrderDetails(orderid);
        ViewData["sidebar-active"] = "Order";
        return View(orderDetails);
    }
    #endregion

    #region Download Invoice PDF
    public async Task<IActionResult> GenerateInvoicePDF(long orderid)
    {
        OrderDetailViewModel orderDetails = await _orderService.GetOrderDetails(orderid);
        //   return PartialView("Invoice", orderDetails);
        ViewAsPdf PDF = new ViewAsPdf("Invoice", orderDetails)
        {
            FileName = "Invoice.pdf"
        };
        return PDF;
    }
    #endregion

    #region Download OrderDetail PDF
    public async Task<IActionResult> GenerateOrderDetailPDF(long orderid)
    {
        OrderDetailViewModel orderDetails = await _orderService.GetOrderDetails(orderid);
        //   return PartialView("OrderDetailPDF", orderDetails);
        ViewAsPdf PDF = new ViewAsPdf("OrderDetailPDF", orderDetails)
        {
            FileName = "OrderDetail.pdf"
        };
        return PDF;
    }
    #endregion

}