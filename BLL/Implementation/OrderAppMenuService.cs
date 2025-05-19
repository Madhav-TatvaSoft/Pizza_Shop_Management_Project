
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

    public async Task<bool> FavouriteItem(long itemId, bool IsFavourite, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Item? item = await _context.Items.FirstOrDefaultAsync(x => x.ItemId == itemId && !x.Isdelete);
                if (item != null)
                {
                    item.IsFavourite = IsFavourite;
                    item.ModifiedAt = DateTime.Now;
                    item.ModifiedBy = userId;
                    _context.Items.Update(item);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
        List<Customer> customerList = _context.Customers
                                        .Include(cus => cus.AssignTables).ThenInclude(at => at.Table).ThenInclude(t => t.Section)
                                        .Include(sec => sec.AssignTables).ThenInclude(at => at.Order).ThenInclude(o => o.Orderdetails)
                                        .Where(od => od.CustomerId == customerId && !od.Isdelete)
                                        .ToList();

        long orderId = _context.AssignTables.FirstOrDefault(at => at.CustomerId == customerId && !at.Isdelete)?.OrderId ?? 0;

        List<AssignTable> AssignTableList = customerList[0].AssignTables.Where(at => !at.Isdelete).ToList();

        OrderDetailViewModel orderDetailsvm = customerList
          .Select(od => new OrderDetailViewModel
          {
              OrderId = orderId,
              PaymentmethodId = 4,

              // Table Details

              SectionId = AssignTableList[0].Table.SectionId,
              SectionName = AssignTableList[0].Table.Section.SectionName,
              tableList = AssignTableList.Select(t => new Table
              {
                  TableId = t.TableId,
                  TableName = t.Table.TableName,
                  Capacity = t.Table.Capacity,
                  SectionId = t.Table.SectionId
              }).ToList(),

              //Customer Details
              CustomerId = od.CustomerId,
              CustomerName = od.CustomerName,
              PhoneNo = od.PhoneNo,
              Email = od.Email,
              NoOfPerson = od.AssignTables.FirstOrDefault().NoOfPerson

          }).ToList()[0];

        if (orderId != 0)
        {
            var orderDetails = _context.Orderdetails.Include(od => od.Item)
                            .Include(x => x.Modifierorders).ThenInclude(modo => modo.Modifier)
                            .Where(m => m.OrderId == orderId && !m.Isdelete).ToList();

            orderDetailsvm.Status = _context.Orders.FirstOrDefault(x => x.OrderId == orderId && !x.Isdelete)!.Status;

            orderDetailsvm.OrderInstruction = _context.Orders.FirstOrDefault(x => x.OrderId == orderId && !x.Isdelete)?.ExtraInstruction;

            orderDetailsvm.InvoiceId = _context.Invoices.FirstOrDefault(i => i.OrderId == orderId && i.CustomerId == customerId && !i.Isdelete) == null ? 0 : _context.Invoices.FirstOrDefault(i => i.OrderId == orderId && i.CustomerId == customerId && !i.Isdelete)!.InvoiceId;

            orderDetailsvm.OrderDate = _context.AssignTables.FirstOrDefault(x => x.CustomerId == customerId && !x.Isdelete)!.Order!.OrderDate;

            orderDetailsvm.ModifiedOn = _context.AssignTables.FirstOrDefault(x => x.CustomerId == customerId && !x.Isdelete)!.Order!.ModifiedAt == null ?
                                            _context.AssignTables.FirstOrDefault(x => x.CustomerId == customerId && !x.Isdelete)!.Order!.OrderDate : (DateTime)_context.AssignTables.FirstOrDefault(x => x.CustomerId == customerId && !x.Isdelete)!.Order!.ModifiedAt!;

            orderDetailsvm.ratingVM = new RatingsViewModel();

            orderDetailsvm.itemOrderVM = orderDetails
                        .Select(i => new ItemOrderViewModel
                        {
                            ItemId = i.ItemId,
                            ItemName = i.Item.ItemName,
                            Rate = i.Item.Rate,
                            status = "In Progress",
                            Quantity = i.Quantity,
                            ExtraInstruction = i.ExtraInstruction == null ? "" : i.ExtraInstruction,
                            OrderdetailId = i.OrderdetailId,
                            TotalItemAmount = Math.Round(i.Quantity * i.Item.Rate, 2),
                            modifierOrderVM = _context.Modifierorders.Include(m => m.Modifier).Include(m => m.Orderdetail).ThenInclude(m => m.Item)
                                .Where(m => m.Orderdetail.OrderdetailId == i.OrderdetailId)
                                .Select(m => new ModifierorderViewModel
                                {
                                    ModifierId = m.ModifierId,
                                    ModifierName = m.Modifier.ModifierName,
                                    Rate = m.Modifier.Rate,
                                    Quantity = i.Quantity,
                                    TotalModifierAmount = Math.Round(i.Quantity * (decimal)m.Modifier.Rate, 2),
                                }).OrderBy(x => x.ModifierId).ToList()

                        }).ToList();

            orderDetailsvm.SubTotalAmountOrder = Math.Round(orderDetailsvm.itemOrderVM
                                                    .Sum(x => x.TotalItemAmount + x.modifierOrderVM.Sum(x => x.TotalModifierAmount)), 2);

            var taxedetails = _context.Taxes.Where(tax => !tax.Isdelete && (bool)tax.Isenable);

            orderDetailsvm.taxInvoiceVM = new List<TaxInvoiceViewModel>();

            foreach (var tax in taxedetails)
            {

                if (tax.TaxType == "Flat Amount")
                {
                    orderDetailsvm.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.TaxId,
                            TaxName = tax.TaxName,
                            TaxType = tax.TaxType,
                            TaxValue = tax.TaxValue
                        }
                    );
                }
                else
                {
                    orderDetailsvm.taxInvoiceVM.Add(
                        new TaxInvoiceViewModel
                        {
                            TaxId = tax.TaxId,
                            TaxName = tax.TaxName,
                            TaxType = tax.TaxType,
                            TaxValue = Math.Round(tax.TaxValue / 100 * orderDetailsvm.SubTotalAmountOrder, 2)
                        }
                    );
                }
            }
            orderDetailsvm.TotalAmountOrder = orderDetailsvm.SubTotalAmountOrder + orderDetailsvm.taxInvoiceVM.Sum(x => x.TaxValue);

            return orderDetailsvm;
        }
        return orderDetailsvm;
    }

    public async Task<OrderDetailViewModel> UpdateOrderDetailPartialView(List<List<int>> itemList, OrderDetailViewModel orderDetailsvm)
    {
        OrderDetailViewModel orderdetails = orderDetailsvm;
        List<ItemOrderViewModel> itemOrderVM = new();
        itemOrderVM = orderDetailsvm.itemOrderVM;

        orderdetails.itemOrderVM = new();
        for (int k = 0; k < itemList.Count; k++)
        {
            long itemId = itemList[k][0];

            ItemOrderViewModel? itemdata = await _context.Items.Where(x => x.ItemId == itemId && !x.Isdelete)
                                                    .Select(i => new ItemOrderViewModel
                                                    {
                                                        ItemId = i.ItemId,
                                                        ItemName = i.ItemName,
                                                        Rate = i.Rate,
                                                        status = k >= _context.Orderdetails.Where(x => x.OrderId == orderdetails.OrderId && !x.Isdelete).Count() ? "Pending" : "In Progress",
                                                        Quantity = itemList[k][1] >= 1 ? itemList[k][1] : 1,
                                                        ExtraInstruction = itemOrderVM != null ? (k >= itemOrderVM.Count() ? null : itemOrderVM[k].ExtraInstruction) : null,
                                                        OrderdetailId = itemOrderVM != null ? (k >= itemOrderVM.Count() ? 0 : itemOrderVM[k].OrderdetailId) : 0,
                                                        TotalItemAmount = Math.Round(i.Rate * (itemList[k][1] >= 1 ? itemList[k][1] : 1), 2)
                                                    }).FirstAsync();
            itemdata.modifierOrderVM = new();
            for (int j = 2; j < itemList[k].Count; j++)
            {
                Modifier modifier = await _context.Modifiers.FirstOrDefaultAsync(x => x.ModifierId == itemList[k][j] && !x.Isdelete);
                ModifierorderViewModel mod = new();
                mod.ModifierId = modifier.ModifierId;
                mod.ModifierName = modifier.ModifierName;
                mod.Rate = modifier.Rate;
                mod.TotalModifierAmount = Math.Round((decimal)(modifier.Rate * itemdata.Quantity), 2);
                itemdata.modifierOrderVM.Add(mod);
            }
            orderdetails.itemOrderVM.Add(itemdata);

        }
        orderdetails.SubTotalAmountOrder = Math.Round((decimal)orderdetails.itemOrderVM
                                                   .Sum(x => x.TotalItemAmount + x.modifierOrderVM.Sum(x => x.TotalModifierAmount)), 2);
        List<Tax>? taxedetails = await _context.Taxes
        .Where(x => !x.Isdelete && (bool)x.Isenable).ToListAsync();

        orderdetails.taxInvoiceVM = new List<TaxInvoiceViewModel>();
        foreach (var tax in taxedetails)
        {
            if (tax.TaxType == "Flat Amount")
            {
                orderdetails.taxInvoiceVM.Add(
                    new TaxInvoiceViewModel
                    {
                        TaxId = tax.TaxId,
                        TaxName = tax.TaxName,
                        TaxType = tax.TaxType,
                        TaxValue = tax.TaxValue
                    }
                );
            }
            else
            {
                orderdetails.taxInvoiceVM.Add(
                    new TaxInvoiceViewModel
                    {
                        TaxId = tax.TaxId,
                        TaxName = tax.TaxName,
                        TaxType = tax.TaxType,
                        TaxValue = Math.Round(tax.TaxValue / 100 * orderdetails.SubTotalAmountOrder, 2)
                    }
                );
            }
        }
        orderdetails.TotalAmountOrder = orderdetails.SubTotalAmountOrder + orderdetails.taxInvoiceVM.Sum(x => x.TaxValue);
        return orderdetails;
    }

    public async Task<OrderDetailViewModel> RemoveItemfromOrderDetailPartialView(List<List<int>> itemList, int count, OrderDetailViewModel orderDetails)
    {
        OrderDetailViewModel orderdetails = orderDetails;
        ItemOrderViewModel item = orderDetails.itemOrderVM[count];
        orderDetails.itemOrderVM.Remove(item);
        orderdetails.SubTotalAmountOrder = Math.Round((decimal)orderdetails.itemOrderVM
                                                   .Sum(x => x.TotalItemAmount + x.modifierOrderVM.Sum(x => x.TotalModifierAmount)), 2);
        if (orderDetails.SubTotalAmountOrder == 0)
        {
            orderDetails.itemOrderVM = null;
            orderDetails.taxInvoiceVM = null;
            orderDetails.TotalAmountOrder = 0;
            return orderDetails;
        }
        var taxedetails = _context.Taxes
        .Where(x => !x.Isdelete).ToList();

        orderdetails.taxInvoiceVM = new List<TaxInvoiceViewModel>();
        foreach (var tax in taxedetails)
        {
            if (tax.TaxType == "Flat Amount")
            {
                orderdetails.taxInvoiceVM.Add(
                    new TaxInvoiceViewModel
                    {
                        TaxId = tax.TaxId,
                        TaxName = tax.TaxName,
                        TaxType = tax.TaxType,
                        TaxValue = tax.TaxValue
                    }
                );
            }
            else
            {
                orderdetails.taxInvoiceVM.Add(
                    new TaxInvoiceViewModel
                    {
                        TaxId = tax.TaxId,
                        TaxName = tax.TaxName,
                        TaxType = tax.TaxType,
                        TaxValue = Math.Round(tax.TaxValue / 100 * orderdetails.SubTotalAmountOrder, 2)
                    }
                );
            }
        }
        orderdetails.TotalAmountOrder = orderdetails.SubTotalAmountOrder + orderdetails.taxInvoiceVM.Sum(x => x.TaxValue);
        return orderdetails;
    }

    public async Task<OrderDetailViewModel> UpdateCustomerDetails(OrderDetailViewModel orderDetailVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Customer? customer = await _context.Customers.SingleOrDefaultAsync(x => x.CustomerId == orderDetailVM.CustomerId && !x.Isdelete);

                if (customer == null)
                {
                    return null;
                }
                customer.CustomerName = orderDetailVM.CustomerName;
                customer.PhoneNo = orderDetailVM.PhoneNo;
                customer.Email = orderDetailVM.Email;
                customer.ModifiedAt = DateTime.Now;
                customer.ModifiedBy = userId;
                _context.Customers.Update(customer);

                var AssignTable = _context.AssignTables.Where(x => x.CustomerId == orderDetailVM.CustomerId && !x.Isdelete).ToList();

                foreach (var table in AssignTable)
                {
                    table.NoOfPerson = orderDetailVM.NoOfPerson;
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;
                    _context.AssignTables.Update(table);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return orderDetailVM;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<OrderDetailViewModel> SaveOrder(List<int> orderDetailIds, OrderDetailViewModel orderDetailsVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                long orderId;
                if (orderDetailsVM.OrderId == 0)
                {
                    Order newOrder = new Order
                    {
                        CustomerId = orderDetailsVM.CustomerId,
                        OrderDate = DateTime.Now,
                        Status = "Pending",
                        TotalAmount = orderDetailsVM.TotalAmountOrder,
                        PaymentmethodId = 4,
                        PaymentStatusId = 1,
                        SectionId = orderDetailsVM.SectionId,
                        TableId = orderDetailsVM.tableList[0].TableId,
                        ExtraInstruction = orderDetailsVM.OrderInstruction,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now,
                        OrderType = "DineIn",
                    };
                    await _context.Orders.AddAsync(newOrder);
                    await _context.SaveChangesAsync();
                    orderId = newOrder.OrderId;
                }
                else
                {
                    Order? ExistOrder = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderDetailsVM.OrderId && !x.Isdelete);
                    ExistOrder.TotalAmount = orderDetailsVM.TotalAmountOrder;
                    ExistOrder.PaymentmethodId = orderDetailsVM.PaymentmethodId;
                    ExistOrder.PaymentStatusId = 1;
                    ExistOrder.ExtraInstruction = orderDetailsVM.OrderInstruction;
                    ExistOrder.ModifiedBy = userId;
                    ExistOrder.ModifiedAt = DateTime.Now;
                    _context.Orders.Update(ExistOrder);
                    await _context.SaveChangesAsync();
                    orderId = ExistOrder.OrderId;
                }

                // for update
                orderDetailsVM.OrderId = orderId;

                // Add Orderdetails to the database
                for (int i = orderDetailIds.Count; i < orderDetailsVM.itemOrderVM.Count; i++)
                {
                    Orderdetail orderdetail = new Orderdetail
                    {
                        OrderId = orderId,
                        ItemId = orderDetailsVM.itemOrderVM[i].ItemId,
                        Quantity = (int)orderDetailsVM.itemOrderVM[i].Quantity,
                        ExtraInstruction = orderDetailsVM.itemOrderVM[i].ExtraInstruction,
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                    };
                    await _context.Orderdetails.AddAsync(orderdetail);
                    await _context.SaveChangesAsync();

                    orderDetailsVM.itemOrderVM[i].OrderdetailId = orderdetail.OrderdetailId;

                    for (int j = 0; j < orderDetailsVM.itemOrderVM[i].modifierOrderVM.Count; j++)
                    {
                        Modifierorder modifierOrder = new Modifierorder
                        {
                            OrderdetailId = orderdetail.OrderdetailId,
                            ModifierId = orderDetailsVM.itemOrderVM[i].modifierOrderVM[j].ModifierId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            ModifierQuantity = (int)orderDetailsVM.itemOrderVM[i].Quantity,

                        };
                        await _context.Modifierorders.AddAsync(modifierOrder);
                    }
                }


                // Update the Existing Orderdetails
                for (int i = 0; i < orderDetailIds.Count; i++)
                {
                    Orderdetail? ExistingOrderdetail = await _context.Orderdetails.FirstOrDefaultAsync(od => od.OrderdetailId == orderDetailIds[i] && !od.Isdelete);
                    if (ExistingOrderdetail != null)
                    {
                        ExistingOrderdetail.Quantity = (int)orderDetailsVM.itemOrderVM[i].Quantity;
                        ExistingOrderdetail.ExtraInstruction = orderDetailsVM.itemOrderVM[i].ExtraInstruction;
                        ExistingOrderdetail.ModifiedAt = DateTime.Now;
                        ExistingOrderdetail.ModifiedBy = userId;
                        _context.Orderdetails.Update(ExistingOrderdetail);
                        // await _context.SaveChangesAsync();

                        List<Modifierorder>? ExistingModifierOrders = await _context.Modifierorders.Where(x => x.OrderdetailId == ExistingOrderdetail.OrderdetailId).ToListAsync();
                        foreach (var modifierOrder in ExistingModifierOrders)
                        {
                            modifierOrder.ModifierQuantity = (int)orderDetailsVM.itemOrderVM[i].Quantity;
                            modifierOrder.ModifiedAt = DateTime.Now;
                            modifierOrder.ModifiedBy = userId;
                            _context.Modifierorders.Update(modifierOrder);
                        }
                        // await _context.SaveChangesAsync();
                    }
                }

                // Update the AssignTable Orderid and status -> Running
                for (int i = 0; i < orderDetailsVM.tableList.Count; i++)
                {
                    AssignTable? assignTable = await _context.AssignTables.FirstOrDefaultAsync(x => x.TableId == orderDetailsVM.tableList[i].TableId && !x.Isdelete);

                    if (assignTable != null)
                    {
                        assignTable.OrderId = orderId;
                        assignTable.ModifiedAt = DateTime.Now;
                        assignTable.ModifiedBy = userId;
                        _context.AssignTables.Update(assignTable);
                    }

                    Table? tables = await _context.Tables.FirstOrDefaultAsync(x => x.TableId == orderDetailsVM.tableList[i].TableId && !x.Isdelete);
                    if (tables != null)
                    {
                        tables.Status = "Running";
                        tables.ModifiedAt = DateTime.Now;
                        tables.ModifiedBy = userId;
                        _context.Tables.Update(tables);
                    }

                    // await _context.AssignTables.AddAsync(assignTable);
                }

                // Update the Order status to "In Progress"
                Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && !x.Isdelete);
                if (order != null)
                {
                    order.Status = "In Progress";
                    _context.Orders.Update(order);
                }

                for (int i = 0; i < orderDetailsVM.itemOrderVM.Count; i++)
                {
                    orderDetailsVM.itemOrderVM[i].status = "In Progress";
                }



                await _context.SaveChangesAsync();
                var taxedetails = _context.Taxes.Where(x => !x.Isdelete && (bool)x.Isenable).ToList();

                orderDetailsVM.taxInvoiceVM = new List<TaxInvoiceViewModel>();
                foreach (var tax in taxedetails)
                {

                    if (tax.TaxType == "Flat Amount")
                    {
                        orderDetailsVM.taxInvoiceVM.Add(
                            new TaxInvoiceViewModel
                            {
                                TaxId = tax.TaxId,
                                TaxName = tax.TaxName,
                                TaxType = tax.TaxType,
                                TaxValue = tax.TaxValue
                            }
                        );
                    }
                    else
                    {
                        orderDetailsVM.taxInvoiceVM.Add(
                            new TaxInvoiceViewModel
                            {
                                TaxId = tax.TaxId,
                                TaxName = tax.TaxName,
                                TaxType = tax.TaxType,
                                TaxValue = Math.Round(tax.TaxValue / 100 * orderDetailsVM.SubTotalAmountOrder, 2)
                            }
                        );
                    }
                }

                //Add invoice
                if (orderDetailsVM.InvoiceId == 0 || orderDetailsVM.InvoiceId == null)
                {
                    Invoice invoiceObj = new();
                    invoiceObj.InvoiceNo = "#DOM" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    invoiceObj.OrderId = orderDetailsVM.OrderId;
                    invoiceObj.CustomerId = orderDetailsVM.CustomerId;
                    invoiceObj.CreatedAt = DateTime.Now;
                    invoiceObj.CreatedBy = userId;
                    await _context.AddAsync(invoiceObj);
                    _context.SaveChanges();

                    orderDetailsVM.InvoiceId = invoiceObj.InvoiceId;
                }

                await transaction.CommitAsync();

                return orderDetailsVM;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<bool> IsItemsReady(List<int> orderDetailId, OrderDetailViewModel orderDetailsVM)
    {
        foreach (int od in orderDetailId)
        {
            Orderdetail? orderdetail = await _context.Orderdetails.FirstOrDefaultAsync(x => x.OrderdetailId == od && !x.Isdelete);

            if (orderdetail.Quantity != orderdetail.ReadyQuantity)
            {
                return false;
            }
        }
        return true;
    }

    public async Task<bool> CompleteOrder(OrderDetailViewModel orderDetailsVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Update Order
                Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderDetailsVM.OrderId && !x.Isdelete);

                if (order == null)
                {
                    return false;
                }

                order.TotalAmount = orderDetailsVM.TotalAmountOrder;
                order.ExtraInstruction = orderDetailsVM.OrderInstruction;
                order.PaymentmethodId = orderDetailsVM.PaymentmethodId;
                order.Status = "Completed";
                order.PaymentStatusId = 2;
                order.ModifiedAt = DateTime.Now;
                order.ModifiedBy = userId;

                _context.Update(order);
                await _context.SaveChangesAsync();

                // Update OrderDetail
                for (int i = 0; i < orderDetailsVM.itemOrderVM.Count; i++)
                {
                    Orderdetail? orderdetail = await _context.Orderdetails.FirstOrDefaultAsync(x => x.OrderdetailId == orderDetailsVM.itemOrderVM[i].OrderdetailId && !x.Isdelete);
                    orderdetail.Status = "Served";
                    orderdetail.ModifiedAt = DateTime.Now;
                    orderdetail.ModifiedBy = userId;
                    _context.Update(orderdetail);

                }

                // Update AssignTable
                List<AssignTable> assigntable = _context.AssignTables.Where(x => x.OrderId == orderDetailsVM.OrderId && x.CustomerId == orderDetailsVM.CustomerId && !x.Isdelete).ToList();
                for (int i = 0; i < assigntable.Count; i++)
                {
                    assigntable[i].Isdelete = true;
                    assigntable[i].ModifiedAt = DateTime.Now;
                    assigntable[i].ModifiedBy = userId;
                    _context.Update(assigntable[i]);
                }

                // Update Table Status
                for (int i = 0; i < orderDetailsVM.tableList.Count; i++)
                {
                    Table? table = await _context.Tables.FirstOrDefaultAsync(t => t.TableId == orderDetailsVM.tableList[i].TableId && !t.Isdelete);
                    table.Status = "Available";
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;
                    _context.Update(table);
                }
                await _context.SaveChangesAsync();

                // Add TaxInvoice Mapping

                for (int i = 0; i < orderDetailsVM.taxInvoiceVM.Count; i++)
                {
                    TaxInvoiceMapping taxinvoicemapping = new();
                    taxinvoicemapping.TaxId = orderDetailsVM.taxInvoiceVM[i].TaxId;
                    taxinvoicemapping.InvoiceId = orderDetailsVM.InvoiceId;
                    taxinvoicemapping.TaxName = orderDetailsVM.taxInvoiceVM[i].TaxName;
                    taxinvoicemapping.TaxAmount = orderDetailsVM.taxInvoiceVM[i].TaxValue;
                    await _context.AddAsync(taxinvoicemapping);
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

    public async Task<bool> SaveRatings(OrderDetailViewModel orderDetailVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Rating? ratingData = await _context.Ratings.FirstOrDefaultAsync(r => r.CustomerId == orderDetailVM.CustomerId && !r.Isdelete);

                if (ratingData == null)
                {
                    Rating rating = new();
                    rating.Food = orderDetailVM.ratingVM.Food;
                    rating.CustomerId = orderDetailVM.CustomerId;
                    rating.Ambience = orderDetailVM.ratingVM.Ambience;
                    rating.Service = orderDetailVM.ratingVM.Service;
                    rating.Review = orderDetailVM.ratingVM.Review;
                    rating.CreatedAt = DateTime.Now;
                    rating.CreatedBy = userId;
                    await _context.Ratings.AddAsync(rating);
                    await _context.SaveChangesAsync();

                    Order? order = _context.Orders.FirstOrDefault(x => x.CustomerId == orderDetailVM.CustomerId && !x.Isdelete && x.Status == "Completed");

                    if (order != null)
                    {
                        order.RatingId = rating.RatingId;
                        order.ModifiedAt = DateTime.Now;
                        order.ModifiedBy = userId;
                        _context.Orders.Update(order);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return true;
                }
                await transaction.RollbackAsync();
                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<bool> IsAnyItemReady(OrderDetailViewModel orderDetailsVM)
    {
        for (int i = 0; i < orderDetailsVM.itemOrderVM.Count; i++)
        {
            if (orderDetailsVM.itemOrderVM[i].OrderdetailId != 0)
            {
                Orderdetail? orderdetail = await _context.Orderdetails.FirstOrDefaultAsync(x => x.OrderdetailId == orderDetailsVM.itemOrderVM[i].OrderdetailId);
                if (orderdetail.ReadyQuantity > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public async Task<bool> CancelOrder(OrderDetailViewModel orderDetailsVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (orderDetailsVM.OrderId != 0)
                {
                    Order? order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderDetailsVM.OrderId && !o.Isdelete);
                    order!.Status = "Cancelled";
                    order.ModifiedAt = DateTime.Now;
                    order.ModifiedBy = userId;
                    _context.Update(order);
                }

                for (int i = 0; i < orderDetailsVM.itemOrderVM.Count; i++)
                {
                    if (orderDetailsVM.itemOrderVM[i].OrderdetailId != 0)
                    {
                        Orderdetail? orderdetail = await _context.Orderdetails.FirstOrDefaultAsync(od => od.OrderdetailId == orderDetailsVM.itemOrderVM[i].OrderdetailId && !od.Isdelete);
                        orderdetail!.Status = "Cancelled";
                        orderdetail.ModifiedAt = DateTime.Now;
                        orderdetail.ModifiedBy = userId;
                        _context.Update(orderdetail);
                    }
                }

                List<AssignTable> assigntableList = _context.AssignTables.Where(at => at.CustomerId == orderDetailsVM.CustomerId && !at.Isdelete).ToList();

                foreach (var assigntable in assigntableList)
                {
                    assigntable.Isdelete = true;
                    assigntable.ModifiedAt = DateTime.Now;
                    assigntable.ModifiedBy = userId;
                    _context.Update(assigntable);

                    Table? table = await _context.Tables.FirstOrDefaultAsync(t => t.TableId == assigntable.TableId && !t.Isdelete);
                    table!.Status = "Available";
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;
                    _context.Update(table);
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

}