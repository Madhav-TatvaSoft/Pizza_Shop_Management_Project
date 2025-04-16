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

    #region Get Section List
    public List<OrderAppSectionVM> GetAllSectionList()
    {
        List<OrderAppSectionVM> sectionList = _context.Sections
            .Where(sec => !sec.Isdelete).OrderBy(sec => sec.SectionId)
            .Select(sec => new OrderAppSectionVM
            {
                SectionId = sec.SectionId,
                SectionName = sec.SectionName,
                AvailableCount = sec.Tables.Count(table => table.Status == "Available" && !table.Isdelete),
                AssignedCount = sec.Tables.Count(table => table.Status == "Assigned" || table.Status == "Occupied" && !table.Isdelete),
                RunningCount = sec.Tables.Count(table => table.Status == "Running" && !table.Isdelete),
            }).ToList();

        if (sectionList == null)
        {
            return null;
        }

        return sectionList;
    }
    #endregion

    #region Get Table List
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
    #endregion

    #region IsCustomerPresent
    public long IsCustomerPresent(string Email)
    {
        Customer customer = _context.Customers.FirstOrDefault(x => x.Email == Email && !x.Isdelete);
        if (customer != null) return customer.CustomerId;
        else return 0;
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

    #region Get Customer Email
    public List<CustomerViewModel> GetCustomerEmail(string searchTerm)
    {
        List<CustomerViewModel>? Emails = _context.Customers
        .Where(c => c.Email.Contains(searchTerm))
        .Select(c => new CustomerViewModel
        {
            Email = c.Email,
            CustomerName = c.CustomerName ?? "",
            PhoneNo = c.PhoneNo
        })
        .Take(10)
        .ToList();

        return Emails;
    }
    #endregion

    #region AddCustomerToWaitingList
    public async Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        try
        {
            long customerId = IsCustomerPresent(waitingTokenVM.Email);
            Waitinglist waitinglist = new();

            Waitinglist? CustomerPresent = _context.Waitinglists.FirstOrDefault(waiting => waiting.CustomerId == customerId);

            if (CustomerPresent != null)
            {
                CustomerPresent.NoOfPerson += waitingTokenVM.NoOfPerson;
                _context.Waitinglists.Update(CustomerPresent);
            }
            else
            {
                waitinglist.CustomerId = customerId;
                waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
                waitinglist.SectionId = waitingTokenVM.SectionId;
                await _context.Waitinglists.AddAsync(waitinglist);
            }

            await _context.SaveChangesAsync();
            return true;

        }
        catch (Exception exception)
        {
            return false;
        }
    }

    #endregion

    #region Get Waiting Customer List
    public async Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid)
    {

        List<WaitingTokenDetailViewModel>? WaitingCustomer = await _context.Waitinglists.Include(w => w.Customer).Include(c => c.Section)
        .Where(w => !w.Isdelete && w.Section.SectionId == sectionid && w.Isassign == false)
        .Select(w => new WaitingTokenDetailViewModel
        {
            WaitingId = w.WaitingId,
            CustomerName = w.Customer.CustomerName,
            PhoneNo = w.Customer.PhoneNo,
            Email = w.Customer.Email,
            NoOfPerson = w.NoOfPerson,
            SectionId = w.Section.SectionId,
            SectionName = w.Section.SectionName
        }).ToListAsync();

        if (WaitingCustomer != null)
        {
            return WaitingCustomer;
        }

        return null;
    }

    #endregion

}













































































// #region GetListOfCustomerWaiting
// public List<OrderAppTableWaitingDetails> GetListOfCustomerWaiting(long sectionId)
// {
//     List<OrderAppTableWaitingDetails> customerWaiting = _context.Waitinglists.Include(x => x.Customer).Include(x => x.Section)
//                 .Where(x => x.Isdelete == false && x.Isassign == false && x.SectionId == sectionId)
//                 .Select(w => new OrderAppTableWaitingDetails
//                 {
//                     ID = w.WaitingId,
//                     Name = w.Customer.CustomerName,
//                     NoOfPerson = w.NoOfPerson,
//                     customerDetails = new WaitingTokenDetailsViewModel
//                     {
//                         Email = w.Customer.Email,
//                         Name = w.Customer.CustomerName,
//                         Mobileno = (int)w.Customer.Phoneno,
//                         NoOfPerson = w.NoOfPerson,
//                         SectionID = w.SectionId,
//                         SectionName = w.Section.SectionName
//                     }
//                 }).ToList();
//     if (customerWaiting != null) return customerWaiting;
//     else return null;
// }
// #endregion

// #region Assigntable
// public async Task<bool> Assigntable(string Email, int[] TableIds, long userId)
// {
//     try{
//         Waitinglist waitinglist = await _context.Waitinglists.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Customer.Email == Email && x.Isdelete == false && x.Isassign == false);
//     if (waitinglist == null) { return false; }
//     waitinglist.Isassign = true;
//     waitinglist.AssignedAt = DateTime.Now;
//     waitinglist.ModifiedAt= DateTime.Now;
//     waitinglist.ModifiedBy = userId;

//     for (int i = 0; i < TableIds.Length; i++)
//     {
//         Assigntable assigntable = new();
//         assigntable.CustomerId = waitinglist.CustomerId;
//         assigntable.TableId = TableIds[i];
//         assigntable.NoOfPerson = waitinglist.NoOfPerson;
//         await _context.AddAsync(assigntable);

//         Table table =await _context.Tables.FirstOrDefaultAsync(x => x.TableId == TableIds[i] && x.Isdelete==false);
//         table.Status = "Assigned";
//         table.ModifiedAt = DateTime.Now;
//         table.ModifiedBy = userId;
//         _context.Update(table);
//         await _context.SaveChangesAsync();
//     }

//     _context.Update(waitinglist);
//     await _context.SaveChangesAsync();

//     return true;
//     }catch(Exception e){
//         return false;
//     }
// }
