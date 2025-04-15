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
            .Where(sec => !sec.Isdelete).OrderBy(sec => sec.SectionId)
            .Select(sec => new OrderAppSectionVM
            {
                SectionId = sec.SectionId,
                SectionName = sec.SectionName,
                AvailableCount = sec.Tables.Count(table => table.Status == "Available"),
                AssignedCount = sec.Tables.Count(table => table.Status == "Assigned" || table.Status == "Occupied"),
                RunningCount = sec.Tables.Count(table => table.Status == "Running"),
            }).ToList();

        if (sectionList == null)
        {
            return null;
        }

        return sectionList;
    }

    public List<OrderAppTableVM> GetTablesBySection(long SectionId)
    {
        List<OrderAppTableVM>? tableListVM = _context.Tables.Include(table => table.AssignTables).ThenInclude(At => At.Order).Where(table => table.Section.SectionId == SectionId && !table.Isdelete)
        .Select(table => new OrderAppTableVM
        {
            TableId = table.TableId,
            SectionId = table.SectionId,
            TableName = table.TableName,
            Capacity = table.Capacity,
            Status = table.Status,
            TableTime = (DateTime)((table.AssignTables.FirstOrDefault() != null) ? table.AssignTables.FirstOrDefault().CreatedAt : DateTime.Now),
            OrderAmount = (table.AssignTables.FirstOrDefault() != null) ? table.AssignTables.FirstOrDefault().Order.TotalAmount : 0
        }).ToList();

        if (tableListVM == null)
        {
            return null;
        }

        return tableListVM;
    }

    #region IsCustomerPresent
    public long IsCustomerPresent(string Email)
    {
        return _context.Customers.FirstOrDefault(x => x.Email == Email && !x.Isdelete).CustomerId;
    }
    #endregion

    #region AddCustomer
    public async Task<bool> AddCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        Customer customer = new();
        customer.CustomerName = waitingTokenVM.CustomerName;
        customer.Email = waitingTokenVM.Email;
        customer.PhoneNo = waitingTokenVM.PhoneNo;
        customer.CreatedBy = userId;
        await _context.AddAsync(customer);
        await _context.SaveChangesAsync();
        return true;
    }
    #endregion

    #region AddCustomerToWaitingList
    public async Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        try
        {
            long customerId = IsCustomerPresent(waitingTokenVM.Email);

            Waitinglist waitinglist = new();
            waitinglist.CustomerId = customerId;
            waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
            waitinglist.SectionId = waitingTokenVM.SectionId;
            await _context.AddAsync(waitinglist);
            await _context.SaveChangesAsync();
            return true;

        }
        catch (Exception e)
        {
            return false;
        }
    }

    #endregion
}