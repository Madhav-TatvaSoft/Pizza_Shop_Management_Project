using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderService : IOrderService
{
    private readonly PizzaShopDbContext _context;

    public OrderService(PizzaShopDbContext context)
    {
        _context = context;
    }
    public PaginationViewModel<OrdersViewModel> GetOrderList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string orderStatus = "", string fromDate = "", string toDate = "", string selectRange = "")
    {
        var query = _context.Orders
            .Include(u => u.Customer)
            .Include(u => u.Rating)
            .Include(u => u.Paymentmethod)
            .Where(u => u.Isdelete == false).OrderBy(u => u.OrderId)
            .Select(u => new OrdersViewModel
            {
                OrderId = u.OrderId,
                OrderDate = System.DateOnly.FromDateTime(u.OrderDate.Date),
                CustomerId = u.Customer.CustomerId,
                CustomerName = u.Customer.CustomerName,
                Status = u.Status,
                TotalAmount = u.TotalAmount,
                PaymentmethodId = u.Paymentmethod.PaymentMethodId,
                PaymentMethodName = u.Paymentmethod.PaymentType,
                RatingId = u.Rating.RatingId,
                rating = (int)Math.Ceiling(((double)u.Rating.Ambience + (double)u.Rating.Food + (double)u.Rating.Service) / 3),
            })
            .AsQueryable();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.CustomerName.ToLower().Contains(lowerSearchTerm) ||
                u.OrderId.ToString().Contains(lowerSearchTerm)
            );
        }

        // Apply filter
        switch (orderStatus)
        {
            case "Pending":
                query = query.Where(u => u.Status == "Pending");
                break;
            case "In Progress":
                query = query.Where(u => u.Status == "In Progress");
                break;
            case "Served":
                query = query.Where(u => u.Status == "Served");
                break;
            case "Completed":
                query = query.Where(u => u.Status == "Completed");
                break;
            case "Cancelled":
                query = query.Where(u => u.Status == "Cancelled");
                break;
            case "On Hold":
                query = query.Where(u => u.Status == "On Hold");
                break;
            case "Failed":
                query = query.Where(u => u.Status == "Failed");
                break;
        }

        // Apply date range filter
        if (!string.IsNullOrEmpty(selectRange))
        {
            switch (selectRange)
            {
                case "Last 7 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)));
                    break;
                case "Last 30 days":
                    query = query.Where(x => x.OrderDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));
                    break;
                case "Current Month":
                    query = query.Where(x => x.OrderDate.Month == DateTime.Now.Month);
                    break;

            }
        }

        // Apply date filter
        if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
        {
            DateTime fromTemp = DateTime.Parse(fromDate);
            DateTime toTemp = DateTime.Parse(toDate);
            DateOnly from = DateOnly.FromDateTime(fromTemp);
            DateOnly to = DateOnly.FromDateTime(toTemp);
            query = query.Where(u => u.OrderDate >= from && u.OrderDate <= to);
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        //sorting
        switch (sortColumn)
        {
            case "Order":
                query = sortDirection == "asc" ? query.OrderBy(u => u.OrderId) : query.OrderByDescending(u => u.OrderId);
                break;

            case "Date":
                query = sortDirection == "asc" ? query.OrderBy(u => u.OrderDate) : query.OrderByDescending(u => u.OrderDate);
                break;
            case "Customer":
                query = sortDirection == "asc" ? query.OrderBy(u => u.CustomerName) : query.OrderByDescending(u => u.CustomerName);
                break;
            case "Amount":
                query = sortDirection == "asc" ? query.OrderBy(u => u.TotalAmount) : query.OrderByDescending(u => u.TotalAmount);
                break;
        }

        // Apply pagination
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<OrdersViewModel>(items, totalCount, pageNumber, pageSize);
    }
}
