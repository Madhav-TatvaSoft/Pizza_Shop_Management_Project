using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderAppTableService : IOrderAppTableService
{
    private readonly PizzaShopDbContext _context;
    private readonly ICustomerService _customerService;

    #region Constructor
    public OrderAppTableService(PizzaShopDbContext context, ICustomerService customerService)
    {
        _context = context;
        _customerService = customerService;
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
                WaitingCount = sec.Waitinglists.Count(w => w.SectionId == sec.SectionId && !w.Isdelete && !w.Isassign),
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
        List<OrderAppTableVM>? tableListVM = _context.Tables
        .Include(table => table.AssignTables).ThenInclude(At => At.Order)
        .Where(table => table.Section.SectionId == SectionId && !table.Isdelete)
        .Select(table => new OrderAppTableVM
        {
            TableId = table.TableId,
            SectionId = table.SectionId,
            TableName = table.TableName,
            Capacity = table.Capacity,
            Status = table.Status,
            // TableTime = (DateTime)((table.AssignTables.LastOrDefault() != null) ? table.AssignTables.LastOrDefault().CreatedAt : DateTime.Now),
            // OrderAmount = (table.AssignTables.LastOrDefault() != null && table.AssignTables.LastOrDefault().Order != null) ? table.AssignTables.LastOrDefault().Order.TotalAmount : 0
        }).ToList();

        if (tableListVM == null)
        {
            return null;
        }

        return tableListVM;
    }
    #endregion

    #region AddCustomerToWaitingList
    public async Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        try
        {
            long customerId = _customerService.IsCustomerPresent(waitingTokenVM.Email);

            if (waitingTokenVM.WaitingId == 0)
            {
                Waitinglist waitinglist = new();

                Waitinglist? CustomerPresent = _context.Waitinglists.FirstOrDefault(waiting => waiting.CustomerId == customerId);
                if (CustomerPresent != null)
                {
                    return false;
                }

                // if (CustomerPresent != null)
                // {
                //     CustomerPresent.NoOfPerson += waitingTokenVM.NoOfPerson;
                //     _context.Waitinglists.Update(CustomerPresent);
                // }
                // else
                // {
                waitinglist.CustomerId = customerId;
                waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
                waitinglist.SectionId = waitingTokenVM.SectionId;
                waitinglist.CreatedAt = DateTime.Now;
                await _context.Waitinglists.AddAsync(waitinglist);
                // }
            }
            else
            {
                Waitinglist? waitinglist = await _context.Waitinglists.FirstOrDefaultAsync(w => w.WaitingId == waitingTokenVM.WaitingId && !w.Isdelete && !w.Isassign);
                if (waitinglist != null)
                {
                    waitinglist.CustomerId = customerId;
                    waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
                    waitinglist.SectionId = waitingTokenVM.SectionId;
                    waitinglist.ModifiedAt = DateTime.Now;
                    waitinglist.ModifiedBy = userId;
                    _context.Update(waitinglist);
                }
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
        .Where(w => !w.Isdelete && w.Section.SectionId == sectionid && !w.Isassign)
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

    #region Get Waiting Customer List By Id

    public async Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId)
    {
        try
        {
            WaitingTokenDetailViewModel? waitingCustomer = await _context.Waitinglists.Include(w => w.Customer).Include(c => c.Section)
            .Where(w => !w.Isdelete && w.WaitingId == waitingId)
            .Select(w => new WaitingTokenDetailViewModel
            {
                WaitingId = w.WaitingId,
                CustomerId = w.Customer.CustomerId,
                CustomerName = w.Customer.CustomerName,
                PhoneNo = w.Customer.PhoneNo,
                Email = w.Customer.Email,
                NoOfPerson = w.NoOfPerson,
                SectionId = w.Section.SectionId,
                SectionName = w.Section.SectionName
            }).FirstOrDefaultAsync();

            if (waitingCustomer == null) return null;

            return waitingCustomer;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    #endregion

    #region Assign Table

    public async Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId)
    {
        JsonArray? tableIds = JsonSerializer.Deserialize<JsonArray>(TableMainVM.TableIds);
        // var tableIdList = tableIds.Select(id => id.GetValue<int>()).ToList();
        List<int>? tableIdList = tableIds.Select(id => int.Parse(id.GetValue<string>())).ToList();
        Waitinglist? waitinglist = await _context.Waitinglists.Include(x => x.Customer).FirstOrDefaultAsync(x => x.WaitingId == TableMainVM.waitingTokenDetailViewModel.WaitingId && !x.Isdelete && !x.Isassign);

        if (waitinglist != null)
        {
            waitinglist.Isassign = true;
            waitinglist.AssignedAt = DateTime.Now;
            waitinglist.ModifiedAt = DateTime.Now;
            waitinglist.ModifiedBy = userId;
            _context.Waitinglists.Update(waitinglist);
        }

        var tables = _context.Tables.Where(t => tableIdList.Contains((int)t.TableId) && !t.Isdelete && t.Status == "Available").ToList();


        if (tables != null)
        {

            for (int i = 0; i < tableIdList.Count(); i++)
            {
                AssignTable assignTable = new();
                assignTable.CustomerId = _customerService.IsCustomerPresent(TableMainVM.waitingTokenDetailViewModel.Email);
                assignTable.TableId = tableIdList[i];
                assignTable.NoOfPerson = TableMainVM.waitingTokenDetailViewModel.NoOfPerson;
                assignTable.CreatedAt = DateTime.Now;
                assignTable.CreatedBy = userId;
                await _context.AddAsync(assignTable);
                Table? table = await _context.Tables.FirstOrDefaultAsync(x => x.TableId == tableIdList[i] && !x.Isdelete);
                if (table != null)
                {
                    table.Status = "Assigned";
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;
                    _context.Tables.Update(table);
                }
            }
        }

        await _context.SaveChangesAsync();

        return true;

    }

    #endregion

}