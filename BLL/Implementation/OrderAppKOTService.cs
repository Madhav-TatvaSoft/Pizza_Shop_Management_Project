using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class OrderAppKOTService : IOrderAppKOTService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public OrderAppKOTService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Get KOT Items
    public async Task<List<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter)
    {
        IQueryable<Order> query = _context.Orders
            .Include(x => x.Orderdetails)
                .ThenInclude(x => x.Item)
            .Include(x => x.Orderdetails)
                .ThenInclude(x => x.Modifierorders)
            .Include(x => x.AssignTables)
                .ThenInclude(x => x.Table)
            .Include(x => x.Section)
            .Where(x => !x.Isdelete);

        // && (x.OrderDate.Date > DateTime.Now.Date.AddDays(-6)) && (x.OrderDate.Date < DateTime.Now.Date)

        // IQueryable<Order> query2 = from o in _context.Orders.Where(x => !x.Isdelete)
        //                             join od in _context.Orderdetails.Where(x => !x.Isdelete) on o.OrderId equals od.OrderId
        //                             join i in _context.Items.Where(x => !x.Isdelete) on od.ItemId equals i.ItemId
        //                             join mo in _context.Modifierorders.Where(x => !x.Isdelete) on od.OrderdetailId equals mo.OrderdetailId
        //                             select new Order
        //                             {
        //                                 OrderId = o.OrderId,
        //                                 OrderDate = o.OrderDate,
        //                                 SectionId = o.SectionId,
        //                                 Section = o.Section,
        //                             };

        if (catid == 0)
        {
            query = query.Where(x => x.Orderdetails.Any(od => !od.Item.Isdelete && ((filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))));

            var data = query
            .Select(x => new OrderAppKOTViewModel
            {
                OrderId = x.OrderId,
                OrderDate = x.OrderDate,
                ExtraInstruction = x.ExtraInstruction,
                SectionId = x.SectionId,
                SectionName = x.Section.SectionName,
                tableList = x.AssignTables.Select(y => new Table
                {
                    TableId = y.TableId,
                    TableName = y.Table.TableName
                }).ToList(),
                itemOrderVM = x.Orderdetails.Where(od => !od.Item.Isdelete && (filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))
                .Select(y => new ItemOrderViewModel
                {
                    ItemId = y.ItemId,
                    OrderdetailId = y.OrderdetailId,
                    ItemName = y.Item.ItemName,
                    ExtraInstruction = y.ExtraInstruction,
                    Quantity = (filter == "Ready") ? y.ReadyQuantity : (y.Quantity - y.ReadyQuantity),
                    modifierOrderVM = y.Modifierorders.Select(z => new ModifierorderViewModel
                    {
                        ModifierId = z.ModifierId,
                        ModifierName = z.Modifier.ModifierName,
                        Quantity = (int)z.ModifierQuantity
                    }).ToList()
                }).ToList()
            }).ToList();

            return data;
        }
        else
        {
            query = query.Where(x => x.Orderdetails.Any(od => (od.Item.CategoryId == catid) && !od.Item.Isdelete && ((filter == "Ready") ? (od.ReadyQuantity > 0) : (od.Quantity - od.ReadyQuantity > 0))));

            var data = query
            .Select(x => new OrderAppKOTViewModel
            {
                OrderId = x.OrderId,
                OrderDate = x.OrderDate,
                ExtraInstruction = x.ExtraInstruction,
                SectionId = x.SectionId,
                SectionName = x.Section.SectionName,
                tableList = x.AssignTables.Select(y => new Table
                {
                    TableId = y.TableId,
                    TableName = y.Table.TableName
                }).ToList(),
                itemOrderVM = x.Orderdetails.Where(x => (x.Item.CategoryId == catid) && !x.Item.Isdelete && ((filter == "Ready") ? (x.ReadyQuantity > 0) : (x.Quantity - x.ReadyQuantity > 0)))
                .Select(y => new ItemOrderViewModel
                {
                    ItemId = y.ItemId,
                    OrderdetailId = y.OrderdetailId,
                    ItemName = y.Item.ItemName,
                    ExtraInstruction = y.ExtraInstruction,
                    Quantity = (filter == "Ready") ? y.ReadyQuantity : (y.Quantity - y.ReadyQuantity),
                    modifierOrderVM = y.Modifierorders.Select(z => new ModifierorderViewModel
                    {
                        ModifierId = z.ModifierId,
                        ModifierName = z.Modifier.ModifierName,
                        Quantity = (int)z.ModifierQuantity
                    }).ToList()
                }).ToList()
            }).ToList();
            return data;
        }
    }

    #endregion

    #region Get KOT Items From Modal
    public async Task<OrderAppKOTViewModel> GetKOTItemsFromModal(long catid, string filter, long orderid)
    {
        List<OrderAppKOTViewModel> KotModalData = await GetKOTItems(catid, filter);
        var KotData = KotModalData.Where(x => x.OrderId == orderid).SingleOrDefault();
        if (KotData == null)
        {
            KotData = new OrderAppKOTViewModel();
        }
        return KotData;
    }

    #endregion

    #region Update KOT Status
    public async Task<bool> UpdateKOTStatus(string filter, int[] orderDetailId, int[] quantity)
    {
        if (orderDetailId.Length == 0 || orderDetailId == null || quantity == null || quantity.Length == 0 || orderDetailId.Length != quantity.Length)
        {
            return false;
        }

        for (int i = 0; i < orderDetailId.Length; i++)
        {
            Orderdetail? orderDetail = await _context.Orderdetails.FirstOrDefaultAsync(x => x.OrderdetailId == orderDetailId[i]);

            if (orderDetail == null)
            {
                return false;
            }
            if (filter == "In_Progress")
            {
                orderDetail.ReadyQuantity += quantity[i];
            }
            else
            {
                orderDetail.ReadyQuantity -= quantity[i];
            }
            _context.Orderdetails.Update(orderDetail);
        }
        await _context.SaveChangesAsync();
        return true;
    }
    #endregion

}
