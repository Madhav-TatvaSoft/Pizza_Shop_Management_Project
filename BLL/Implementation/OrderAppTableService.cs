using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderAppTableService : IOrderAppTableService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public OrderAppTableService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    public List<OrderAppSectionVM> GetAllSectionList()
    {
        List<OrderAppSectionVM> sectionList = _context.Sections
            .Where(x => !x.Isdelete)
            .Select(x => new OrderAppSectionVM
            {
                SectionId = x.SectionId,
                SectionName = x.SectionName,
                AvailableCount = x.Tables.Count(y => y.Status == "Available"),
                AssignedCount = x.Tables.Count(y => y.Status == "Assigned"),
                RunningCount = x.Tables.Count(y => y.Status == "Running"),
            }).ToList();

        return sectionList;
    }

    public List<OrderAppTableVM> GetTablesBySection(long sectionid)
    {
        List<OrderAppTableVM>? tableListVM = _context.Tables.Where(x => x.Section.SectionId == sectionid && !x.Isdelete)
        .Select(y => new OrderAppTableVM
        {
            TableId = y.TableId,
            SectionId = y.SectionId,
            TableName = y.TableName,
            Capacity = y.Capacity,
            Status = y.Status,
            TableTime = (DateTime)y.CreatedAt,
            OrderAmount = (decimal)205.00
            //  _context.Orders
            //     .Where(o => o.SectionId == x.SectionId && o.TableId == y.TableId && !o.Isdelete)
            //     .Sum(o => o.Orderdetails.Sum(od => od.Quantity * od.Item.Rate))
        }).ToList();

        return tableListVM;
    }
}
