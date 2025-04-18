using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderAppWaitingListService : IOrderAppWaitingListService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public OrderAppWaitingListService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    public List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid)
    {
        if (sectionid == 0)
        {
            try
            {
                var waiting = _context.Waitinglists.Include(x => x.Customer).Where(waiting => !waiting.Isdelete && !waiting.Isassign)
                        .Select(waiting => new WaitingTokenDetailViewModel
                        {
                            WaitingId = waiting.WaitingId,
                            CustomerId = waiting.CustomerId,
                            CustomerName = waiting.Customer.CustomerName,
                            PhoneNo = waiting.Customer.PhoneNo,
                            Email = waiting.Customer.Email,
                            NoOfPerson = waiting.NoOfPerson,
                            CreatedAt = waiting.CreatedAt,
                            SectionId = waiting.SectionId,
                        }).ToList();
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
                    CreatedAt = waiting.CreatedAt,
                    SectionId = waiting.SectionId,
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
                    CreatedAt = waitingList.CreatedAt
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

    // public async Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId)
    // {
    //     try
    //     {
    //         if (waitingTokenVM.WaitingTokenDetailVM.WaitingId == 0)
    //         {
    //             Waitinglist waiting = new Waitinglist
    //             {
    //                 CustomerId = waitingTokenVM.WaitingTokenDetailVM.CustomerId,
    //                 NoOfPerson = WaitingVM.WaitingTokenDetailVM.NoOfPerson,
    //                 SectionId = waitingTokenVM.WaitingTokenDetailVM.SectionId,
    //                 Isdelete = false,
    //                 Isassign = false,
    //                 CreatedAt = DateTime.Now,
    //             };
    //             await _context.Waitinglists.AddAsync(waiting);
    //         }
    //         else
    //         {
    //             Waitinglist waiting = await _context.Waitinglists.FirstOrDefaultAsync(x => x.WaitingId == WaitingVM.WaitingTokenDetailVM.WaitingId && !x.Isdelete);
    //             if (waiting != null)
    //             {
    //                 waiting.CustomerId = WaitingVM.WaitingTokenDetailVM.CustomerId;
    //                 waiting.NoOfPerson = WaitingVM.WaitingTokenDetailVM.NoOfPerson;
    //                 waiting.SectionId = WaitingVM.WaitingTokenDetailVM.SectionId;
    //                 waiting.ModifiedAt = DateTime.Now;
    //                 waiting.ModifiedBy = Use
    //             }
    //         }
    //         await _context.SaveChangesAsync();
    //         return true;
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e.Message);
    //         return false;
    //     }
    // }

}