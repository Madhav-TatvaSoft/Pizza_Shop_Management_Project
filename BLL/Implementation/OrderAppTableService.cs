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

    public OrderAppTableService(PizzaShopDbContext context, ICustomerService customerService)
    {
        _context = context;
        _customerService = customerService;
    }

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

    public List<OrderAppTableVM> GetTablesBySection(long SectionId)
    {
        List<OrderAppTableVM>? tableListVM = _context.Tables
        .Include(table => table.AssignTables).ThenInclude(At => At.Order)
        .Where(table => table.Section.SectionId == SectionId && !table.Isdelete).OrderBy(table => table.TableId)
        .Select(table => new OrderAppTableVM
        {
            TableId = table.TableId,
            SectionId = table.SectionId,
            TableName = table.TableName,
            Capacity = table.Capacity,
            Status = table.Status,
            TableTime = (DateTime)(table.AssignTables.FirstOrDefault(x => !x.Isdelete) != null ? table.AssignTables.FirstOrDefault(x => !x.Isdelete).CreatedAt : DateTime.Now),
            OrderAmount = table.Status == "Running" ? (table.AssignTables.FirstOrDefault(x => !x.Isdelete) != null ? (table.AssignTables.FirstOrDefault(x => !x.Isdelete).Order != null ? table.AssignTables.FirstOrDefault(x => !x.Isdelete).Order.TotalAmount : 0) : 0) : 0
        }).ToList();

        if (tableListVM == null)
        {
            return null;
        }

        return tableListVM;
    }

    public bool CheckTokenExists(WaitingTokenDetailViewModel waitingTokenVM)
    {
        Waitinglist? waitinglist = _context.Waitinglists.FirstOrDefault(w => w.Customer.Email == waitingTokenVM.Email && w.WaitingId != waitingTokenVM.WaitingId && !w.Isdelete && !w.Isassign);
        if (waitinglist != null)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
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

                    waitinglist.CustomerId = customerId;
                    waitinglist.NoOfPerson = waitingTokenVM.NoOfPerson;
                    waitinglist.SectionId = waitingTokenVM.SectionId;
                    waitinglist.CreatedAt = DateTime.Now;
                    waitinglist.CreatedBy = userId;
                    await _context.Waitinglists.AddAsync(waitinglist);

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

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



    }

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

    #region Get Waiting Customer List By Id
    public async Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId)
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

    #endregion

    #region Assign Table

    public async Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                JsonArray? tableIds = JsonSerializer.Deserialize<JsonArray>(TableMainVM.TableIds);
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

                List<Table>? tables = _context.Tables.Where(t => tableIdList.Contains((int)t.TableId) && !t.Isdelete && t.Status == "Available").ToList();


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

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    #endregion

}