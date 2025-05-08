using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderAppWaitingListService : IOrderAppWaitingListService
{
    private readonly PizzaShopDbContext _context;

    public OrderAppWaitingListService(PizzaShopDbContext context)
    {
        _context = context;
    }
    
    public List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid)
    {
        if (sectionid == 0)
        {
            try
            {
                List<WaitingTokenDetailViewModel>? waiting = _context.Waitinglists.Include(x => x.Customer).Where(waiting => !waiting.Isdelete && !waiting.Isassign)
                        .Select(waiting => new WaitingTokenDetailViewModel
                        {
                            WaitingId = waiting.WaitingId,
                            CustomerId = waiting.CustomerId,
                            CustomerName = waiting.Customer.CustomerName,
                            PhoneNo = waiting.Customer.PhoneNo,
                            Email = waiting.Customer.Email,
                            NoOfPerson = waiting.NoOfPerson,
                            CreatedAt = (DateTime)waiting.CreatedAt,
                            WaitingTime = TimeOnly.FromTimeSpan((TimeSpan)(DateTime.Now - waiting.CreatedAt)),
                            SectionId = waiting.SectionId,
                            SectionName = waiting.Section.SectionName
                        }).OrderBy(w => w.WaitingId).ToList();
                if (waiting == null)
                {
                    return null;
                }
                return waiting;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        else
        {
            var waitingList = _context.Waitinglists.Where(waiting => !waiting.Isdelete && waiting.SectionId == sectionid && !waiting.Isassign)
                .Select(waiting => new WaitingTokenDetailViewModel
                {
                    WaitingId = waiting.WaitingId,
                    CustomerId = waiting.CustomerId,
                    CustomerName = waiting.Customer.CustomerName,
                    PhoneNo = waiting.Customer.PhoneNo,
                    Email = waiting.Customer.Email,
                    NoOfPerson = waiting.NoOfPerson,
                    CreatedAt = (DateTime)waiting.CreatedAt,
                    SectionId = waiting.SectionId,
                    SectionName = waiting.Section.SectionName
                }).ToList();

            if (waitingList == null)
            {
                return null;
            }
            return waitingList;
        }
    }

    public WaitingTokenDetailViewModel GetWaitingToken(long waitingid)
    {
        try
        {
            Waitinglist? waitingList = _context.Waitinglists.Include(w => w.Customer).Include(wc => wc.Section).FirstOrDefault(wcs => wcs.WaitingId == waitingid && !wcs.Isdelete);
            if (waitingList != null)
            {
                WaitingTokenDetailViewModel WaitingListVM = new WaitingTokenDetailViewModel
                {
                    WaitingId = waitingList.WaitingId,
                    CustomerId = waitingList.CustomerId,
                    CustomerName = waitingList.Customer.CustomerName,
                    PhoneNo = waitingList.Customer.PhoneNo,
                    Email = waitingList.Customer.Email,
                    NoOfPerson = waitingList.NoOfPerson,
                    SectionName = waitingList.Section.SectionName,
                    SectionId = waitingList.Section.SectionId,
                    CreatedAt = (DateTime)waitingList.CreatedAt
                };
                return WaitingListVM;
            }
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task<bool> DeleteWaitingToken(long waitingid)
    {
        try
        {
            var waiting = await _context.Waitinglists.FirstOrDefaultAsync(w => w.WaitingId == waitingid && !w.Isdelete && !w.Isassign);
            if (waiting != null)
            {
                waiting.Isdelete = true;
                // waiting.ModifiedAt = DateTime.Now;
                _context.Waitinglists.Update(waiting);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public List<OrderAppTableVM> GetAvailableTables(long sectionid)
    {
        List<OrderAppTableVM>? tables = _context.Tables
           .Where(t => t.SectionId == sectionid && !t.Isdelete && t.Status == "Available")
           .Select(t => new OrderAppTableVM
           {
               TableId = t.TableId,
               TableName = t.TableName,
               SectionId = t.SectionId,
               Capacity = t.Capacity,
           }).ToList();
        return tables;
    }

    public async Task<bool> AssignTableInWaiting(long waitingId, long sectionId, long customerid, int persons, int[] tableIds, long userId)
    {
        List<int>? tableIdsList = tableIds.ToList();
        Waitinglist? waitinglist = await _context.Waitinglists.Include(x => x.Customer).FirstOrDefaultAsync(x => x.WaitingId == waitingId && !x.Isdelete && !x.Isassign);

        if (waitinglist == null)
        {
            return false;
        }

        waitinglist.Isassign = true;
        waitinglist.AssignedAt = DateTime.Now;
        waitinglist.ModifiedAt = DateTime.Now;
        waitinglist.ModifiedBy = userId;
        _context.Waitinglists.Update(waitinglist);

        List<Table>? tables = _context.Tables.Where(t => tableIdsList.Contains((int)t.TableId) && !t.Isdelete && t.Status == "Available").ToList();

        if (tables != null)
        {

            for (int i = 0; i < tables.Count(); i++)
            {
                AssignTable assignTable = new();
                assignTable.CustomerId = customerid;
                assignTable.TableId = tableIdsList[i];
                assignTable.NoOfPerson = persons;
                assignTable.CreatedAt = DateTime.Now;
                assignTable.CreatedBy = userId;
                await _context.AddAsync(assignTable);

                Table? table = await _context.Tables.FirstOrDefaultAsync(x => x.TableId == tableIdsList[i] && !x.Isdelete);
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

}