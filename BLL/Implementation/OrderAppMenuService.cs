using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Implementation;

public class OrderAppMenuService : IOrderAppMenuService
{
    private readonly PizzaShopDbContext _context;

    public OrderAppMenuService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public List<ItemsViewModel> GetItems(long categoryid, string searchText = "")
    {
        var AllItems = _context.Items.Where(x => x.Isavailable == true && !x.Isdelete).ToList();

        if (categoryid == -1)
        {
            AllItems = AllItems.Where(x => x.IsFavourite == true).ToList();
        }
        else if (categoryid == 0)
        {
            AllItems = AllItems;
        }
        else
        {
            AllItems = AllItems.Where(x => x.CategoryId == categoryid).ToList();
        }

        if (!searchText.IsNullOrEmpty())
        {
            AllItems = AllItems.Where(x => x.ItemName.ToLower().Trim().Contains(searchText.ToLower().Trim())).ToList();
        }

        List<ItemsViewModel> itemsList = AllItems.Select(i => new ItemsViewModel
        {
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            CategoryId = i.CategoryId,
            ItemTypeId = i.ItemTypeId,
            Rate = Math.Ceiling(i.Rate),
            ItemImage = i.ItemImage,
            IsFavourite = i.IsFavourite,
            Isdelete = i.Isdelete
        }).ToList();

        return itemsList;

    }

    public async Task<bool> FavouriteItem(long itemId, bool IsFavourite)
    {
        Item? item = await _context.Items.FirstOrDefaultAsync(x => x.ItemId == itemId && !x.Isdelete);
        if (item != null)
        {
            item.IsFavourite = IsFavourite;
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<ItemModifierViewModel> GetModifiersByItemId(long itemId)
    {
        Item? SelectedItem = _context.Items
        .Include(item => item.ItemModifierGroupMappings).ThenInclude(itemmodgrp => itemmodgrp.ModifierGrp).ThenInclude(modgrp => modgrp.Modifiers).FirstOrDefault(i => i.ItemId == itemId && !i.Isdelete);

        if (SelectedItem == null)
        {
            return new List<ItemModifierViewModel>();
        }
        else
        {
            List<ItemModifierViewModel>? itemModifierGroupMappings = SelectedItem.ItemModifierGroupMappings
                .Where(x => !x.Isdelete)
                .Select(x => new ItemModifierViewModel
                {
                    ModifierGrpId = x.ModifierGrpId,
                    ModifierGrpName = x.ModifierGrp.ModifierGrpName,
                    Minmodifier = x.Minmodifier,
                    Maxmodifier = x.Maxmodifier,
                    modifiersList = x.ModifierGrp.Modifiers
                        .Where(e => !e.Isdelete)
                        .Select(x => new Modifier
                        {
                            ModifierId = x.ModifierId,
                            ModifierName = x.ModifierName,
                            Rate = x.Rate
                        }).ToList()
                }).ToList();
            return itemModifierGroupMappings;
        }
    }

    public OrderDetailViewModel GetOrderDetailsByCustomerId(long customerId)
    {
        List<Customer>? customerList = _context.Customers.Include(cus => cus.AssignTables).ThenInclude(at => at.Table).ThenInclude(t => t.Section)
                    .Include(sec => sec.AssignTables).ThenInclude(at => at.Order).ThenInclude(o => o.Orderdetails)
                    .Where(od => od.CustomerId == customerId && !od.Isdelete).ToList();

        long orderId = _context.AssignTables.FirstOrDefault(at => at.CustomerId == customerId && !at.Isdelete)?.OrderId ?? 0;
        List<AssignTable>? AssignTableList = customerList[0].AssignTables.Where(at => !at.Isdelete).ToList();

        OrderDetailViewModel orderDetailsvm = customerList
          .Select(od => new OrderDetailViewModel
          {
              OrderId = orderId,

              // Table Details
              SectionId = AssignTableList[0].Table.SectionId,
              SectionName = AssignTableList[0].Table.Section.SectionName,
              tableList = AssignTableList.Select(t => new Table
              {
                  TableId = t.TableId,
                  TableName = t.Table.TableName,
                  SectionId = t.Table.SectionId
              }).ToList(),

              //Customer Details
              CustomerId = od.CustomerId,
              CustomerName = od.CustomerName,
              PhoneNo = od.PhoneNo,
              Email = od.Email
          }).ToList()[0];
        // //orderDetails
        if (orderId != 0)
        {
            var orderDetails = _context.Orderdetails.Include(od => od.Item)
                            .Include(x => x.Modifierorders).ThenInclude(modo => modo.Modifier)
                            .Where(m => m.OrderId == orderId && !m.Isdelete).ToList();

            orderDetailsvm.itemOrderVM = orderDetails
                        .Select(i => new ItemOrderViewModel
                        {
                            ItemId = i.ItemId,
                            ItemName = i.Item.ItemName,
                            Rate = i.Item.Rate,
                            status = "In Progress",
                            Quantity = i.Quantity,
                            TotalItemAmount = Math.Round(i.Quantity * i.Item.Rate, 2),
                            modifierOrderVM = _context.Modifierorders.Include(m => m.Modifier).Include(m => m.Orderdetail).ThenInclude(m => m.Item)
                                .Where(m => m.Orderdetail.ItemId == i.ItemId)
                                .Select(m => new ModifierorderViewModel
                                {
                                    ModifierId = m.ModifierId,
                                    ModifierName = m.Modifier.ModifierName,
                                    Rate = m.Modifier.Rate,
                                    Quantity = m.ModifierQuantity,
                                    TotalModifierAmount = Math.Round(m.ModifierQuantity * (decimal)m.Modifier.Rate, 2),
                                }).ToList()

                        }).ToList();

            orderDetailsvm.SubTotalAmountOrder = Math.Round((decimal)orderDetailsvm.itemOrderVM
                                                    .Sum(x => x.TotalItemAmount + x.modifierOrderVM.Sum(x => x.TotalModifierAmount)), 2);

            var taxedetails = _context.TaxInvoiceMappings.Include(x => x.Invoice).Include(x => x.Tax)
            .Where(x => x.Invoice.OrderId == orderId).ToList();

            orderDetailsvm.taxInvoiceVM = new List<TaxInvoiceViewModel>();

            foreach (var tax in taxedetails)
            {

                if (tax.Tax.TaxType == "Fix Amount")
                {
                    orderDetailsvm.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.Tax.TaxId,
                            TaxName = tax.Tax.TaxName,
                            TaxType = tax.Tax.TaxType,
                            TaxValue = tax.Tax.TaxValue
                        }
                    );
                }
                else
                {
                    orderDetailsvm.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.Tax.TaxId,
                            TaxName = tax.Tax.TaxName,
                            TaxType = tax.Tax.TaxType,
                            TaxValue = Math.Round(tax.Tax.TaxValue / 100 * orderDetailsvm.SubTotalAmountOrder, 2)
                        }
                    );
                }
            }
            orderDetailsvm.TotalAmountOrder = orderDetailsvm.SubTotalAmountOrder + orderDetailsvm.taxInvoiceVM.Sum(x => x.TaxValue);

            return orderDetailsvm;
        }
        return orderDetailsvm;
    }
}
