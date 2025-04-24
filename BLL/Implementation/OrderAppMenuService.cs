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
        try
        {
            Customer? orderdetails = _context.Customers
            .Include(x => x.Orders).ThenInclude(o => o.Orderdetails)
            .Include(od =>od.Orders).ThenInclude(x => x.Table).ThenInclude(x => x.Section).ThenInclude(x => x.Waitinglists)
            .FirstOrDefault(x => x.CustomerId == customerId );

            return null;
            
        }

        catch (Exception exception)
        {
            return null;
        }

    }



}
