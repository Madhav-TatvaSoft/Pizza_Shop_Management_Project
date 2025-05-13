using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class ItemService : IItemService
{
    private readonly PizzaShopDbContext _context;
    public ItemService(PizzaShopDbContext context)
    {
        _context = context;
    }
    public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        IQueryable<ItemsViewModel>? query = _context.Items
           .Include(x => x.Category).Include(x => x.ItemType)
           .Where(x => x.CategoryId == catid
            && !x.Isdelete).OrderBy(x => x.ItemId)
           .Select(x => new ItemsViewModel
           {
               ItemId = x.ItemId,
               ItemName = x.ItemName,
               CategoryId = x.CategoryId,
               ItemTypeId = x.ItemTypeId,
               TypeImage = x.ItemType.TypeImage,
               Rate = x.Rate,
               Quantity = x.Quantity,
               ItemImage = x.ItemImage,
               Isavailable = x.Isavailable,
               Isdelete = x.Isdelete
           })
           .AsQueryable();

        //search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(x =>
                x.ItemName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        List<ItemsViewModel>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<ItemsViewModel>(items, totalCount, pageNumber, pageSize);
    }

    #region Item CRUD
    public async Task<bool> AddItem(AddItemViewModel addItemVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (addItemVM == null)
                {
                    return false;
                }

                Item item = new Item();
                item.CategoryId = addItemVM.CategoryId;
                item.ItemName = addItemVM.ItemName;
                item.ItemTypeId = addItemVM.ItemTypeId;
                item.Rate = addItemVM.Rate;
                item.Quantity = addItemVM.Quantity;
                item.Unit = addItemVM.Unit;
                item.Isavailable = addItemVM.Isavailable;
                item.Isdefaulttax = addItemVM.Isdefaulttax;
                item.TaxValue = addItemVM.TaxValue;
                item.Description = addItemVM.Description;
                item.ItemImage = addItemVM.ItemImage;
                item.ShortCode = addItemVM.ShortCode;
                item.CreatedBy = userId;

                await _context.Items.AddAsync(item);
                await _context.SaveChangesAsync();

                foreach (var modifier in addItemVM.itemModifiersVM)
                {
                    ItemModifierGroupMapping itemModifierMapping = new ItemModifierGroupMapping
                    {
                        ItemId = item.ItemId,
                        ModifierGrpId = modifier.ModifierGrpId,
                        Minmodifier = modifier.Minmodifier,
                        Maxmodifier = modifier.Maxmodifier
                    };
                    _context.ItemModifierGroupMappings.Add(itemModifierMapping);
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

    public AddItemViewModel GetItemsByItemId(long itemid)
    {
        Item? item = _context.Items.FirstOrDefault(x => x.ItemId == itemid && !x.Isdelete);

        if (item == null)
        {
            return null;
        }

        AddItemViewModel edititemVM = new();
        {
            edititemVM.CategoryId = item.CategoryId;
            edititemVM.ItemId = item.ItemId;
            edititemVM.ItemName = item.ItemName;
            edititemVM.Description = item.Description;
            edititemVM.Isavailable = (bool)item.Isavailable;
            edititemVM.Isdefaulttax = (bool)item.Isdefaulttax;
            edititemVM.ItemImage = item.ItemImage;
            edititemVM.ItemTypeId = item.ItemTypeId;
            edititemVM.Quantity = (int)item.Quantity;
            edititemVM.Rate = item.Rate;
            edititemVM.ShortCode = item.ShortCode;
            edititemVM.TaxValue = (decimal)item.TaxValue;
            edititemVM.Unit = item.Unit;
        }

        List<ItemModifierViewModel>? data = _context.ItemModifierGroupMappings.Where(e => e.ItemId == itemid && !e.Isdelete)
       .Select(x => new ItemModifierViewModel
       {
           ModifierGrpId = x.ModifierGrpId,
           Minmodifier = x.Minmodifier,
           Maxmodifier = x.Maxmodifier,
           modifiersList = _context.Modifiers.Where(e => e.ModifierGrpId == x.ModifierGrpId && !x.Isdelete).ToList(),
           ModifierGrpName = _context.Modifiergroups.FirstOrDefault(e => e.ModifierGrpId == x.ModifierGrpId && !x.Isdelete).ModifierGrpName
       }).ToList();

        edititemVM.itemModifiersVM = data;

        return edititemVM;
    }

    public async Task<bool> EditItem(AddItemViewModel editItemVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (editItemVM == null)
                {
                    return false;
                }

                Item? item = await _context.Items.FirstOrDefaultAsync(x => x.ItemId == editItemVM.ItemId && !x.Isdelete);

                if (item == null)
                {
                    return false;
                }

                item.CategoryId = editItemVM.CategoryId;
                item.ItemName = editItemVM.ItemName;
                item.ItemTypeId = editItemVM.ItemTypeId;
                item.Rate = editItemVM.Rate;
                item.Quantity = editItemVM.Quantity;
                item.Unit = editItemVM.Unit;
                item.Isavailable = editItemVM.Isavailable;
                item.Isdefaulttax = editItemVM.Isdefaulttax;
                item.TaxValue = editItemVM.TaxValue;
                item.Description = editItemVM.Description;
                if (item.ItemImage == null)
                {
                    item.ItemImage = editItemVM.ItemImage;
                }
                item.ShortCode = editItemVM.ShortCode;
                item.ModifiedAt = DateTime.Now;
                item.ModifiedBy = userId;

                _context.Items.Update(item);
                await _context.SaveChangesAsync();

                List<ItemModifierGroupMapping>? itemModifier = _context.ItemModifierGroupMappings.Where(x => x.ItemId == item.ItemId && !x.Isdelete).ToList();

                foreach (var itemMod in itemModifier)
                {
                    _context.ItemModifierGroupMappings.Remove(itemMod);
                }

                foreach (var modifier in editItemVM.itemModifiersVM)
                {
                    ItemModifierGroupMapping itemModifierMapping = new ItemModifierGroupMapping
                    {
                        ItemId = item.ItemId,
                        ModifierGrpId = modifier.ModifierGrpId,
                        Minmodifier = modifier.Minmodifier,
                        Maxmodifier = modifier.Maxmodifier
                    };
                    _context.ItemModifierGroupMappings.Add(itemModifierMapping);
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

    public async Task<bool> DeleteItem(long itemid, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                List<ItemModifierGroupMapping> itemModifierGroupMappings = await _context.ItemModifierGroupMappings.Where(x => x.ItemId == itemid && !x.Isdelete).ToListAsync();

                for (int i = 0; i < itemModifierGroupMappings.Count; i++)
                {
                    itemModifierGroupMappings[i].Isdelete = true;
                    _context.ItemModifierGroupMappings.Update(itemModifierGroupMappings[i]);
                    await _context.SaveChangesAsync();
                }

                Item? itemToDelete = await _context.Items.SingleOrDefaultAsync(x => x.ItemId == itemid && !x.Isdelete);

                if (itemToDelete == null)
                {
                    return false;
                }

                itemToDelete.Isdelete = true;
                itemToDelete.ModifiedAt = DateTime.Now;
                itemToDelete.ModifiedBy = userId;
                _context.Update(itemToDelete);
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

    #region Check Item Exist
    public bool IsItemExist(AddItemViewModel ItemVM)
    {
        if (ItemVM.ItemId == 0)
        {
            return _context.Items.Any(x => x.ItemName.ToLower().Trim() == ItemVM.ItemName.ToLower().Trim() && !x.Isdelete);
        }
        else
        {
            return _context.Items.Any(x => x.ItemId != ItemVM.ItemId && x.ItemName.ToLower().Trim() == ItemVM.ItemName.ToLower().Trim() && !x.Isdelete);
        }
    }
    #endregion

}