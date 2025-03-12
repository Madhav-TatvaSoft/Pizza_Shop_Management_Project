using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class MenuService : IMenuService
{
    private readonly PizzaShopDbContext _context;

    public MenuService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public List<Category> GetAllCategories()
    {
        return _context.Categories.Where(x => x.Isdelete == false).ToList();
    }

    public List<Modifiergroup> GetAllModifierGroupList()
    {
        return _context.Modifiergroups.Where(x => x.Isdelete == false).ToList();
    }

    public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        var query = _context.Items
           .Include(x => x.Category).Include(x => x.ItemType)
           .Where(x => x.CategoryId == catid).Where(x => x.Isdelete == false)
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
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<ItemsViewModel>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> AddCategory(Category category, long userId)
    {
        var isCategoryExistsAdd = _context.Categories.FirstOrDefault(x => x.CategoryName == category.CategoryName);
        // var isCategoryExistsAdd = _context.Categories.FirstOrDefault(x => x.Isdelete == false && x.CategoryName == category.CategoryName);

        if (category != null && isCategoryExistsAdd == null)
        {
            Category cat = new Category();
            cat.CategoryName = category.CategoryName;
            cat.Description = category.Description;
            cat.CreatedBy = userId;
            await _context.Categories.AddAsync(cat);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> EditCategoryById(Category category, long Cat_Id, long userId)
    {
        if (category == null || Cat_Id == null)
        {
            return false;
        }
        else
        {
            var isCategoryExistsEdit = _context.Categories.FirstOrDefault(x => x.CategoryName.ToLower() == category.CategoryName.ToLower());
            if (isCategoryExistsEdit == null)
            {
                Category cat = _context.Categories.FirstOrDefault(x => x.CategoryId == Cat_Id);
                cat.CategoryName = category.CategoryName;
                cat.Description = category.Description;
                cat.ModifiedBy = userId;
                cat.ModifiedAt = DateTime.Now;
                _context.Categories.Update(cat);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public async Task<bool> DeleteCategory(long Cat_Id)
    {
        if (Cat_Id == null)
        {
            return false;
        }
        Category category = _context.Categories.FirstOrDefault(x => x.CategoryId == Cat_Id);

        category.CategoryName = category.CategoryName + DateTime.Now;
        category.Isdelete = true;
        _context.Update(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddItem(AddItemViewModel addItemVM, long userId)
    {
        if (addItemVM.CategoryId == null)
        {
            return false;
        }
        else
        {
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
            return true;

        }
    }

    public AddItemViewModel GetItemsByItemId(long itemid)
    {
        var item = _context.Items.FirstOrDefault(x => x.ItemId == itemid && x.Isdelete == false);
        AddItemViewModel additemVM = new();
        {
            additemVM.CategoryId = item.CategoryId;
            additemVM.ItemId = item.ItemId;
            additemVM.ItemName = item.ItemName;
            additemVM.Description = item.Description;
            additemVM.Isavailable = (bool)item.Isavailable;
            additemVM.Isdefaulttax = (bool)item.Isdefaulttax;
            additemVM.ItemImage = item.ItemImage;
            additemVM.ItemTypeId = item.ItemTypeId;
            additemVM.Quantity = (int)item.Quantity;
            additemVM.Rate = item.Rate;
            additemVM.ShortCode = item.ShortCode;
            additemVM.TaxValue = (decimal)item.TaxValue;
            additemVM.Unit = item.Unit;
        }
        return additemVM;
    }

    public async Task<bool> EditItem(AddItemViewModel editItemVM, long userId)
    {
        if (editItemVM.CategoryId == null)
        {
            return false;
        }
        else
        {
            var item = _context.Items.FirstOrDefault(x => x.ItemId == editItemVM.ItemId && x.Isdelete == false);
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
            item.ItemImage = editItemVM.ItemImage;
            item.ShortCode = editItemVM.ShortCode;
            item.ModifiedAt = DateTime.Now;
            item.ModifiedBy = userId;

            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public async Task<bool> DeleteItem(long itemid)
    {
        var itemToDelete = _context.Items.FirstOrDefault(x => x.ItemId == itemid);

        itemToDelete.ItemName = itemToDelete.ItemName + DateTime.Now;
        itemToDelete.Isdelete = true;
        _context.Update(itemToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

    public PaginationViewModel<ModifiersViewModel> GetMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        var query = _context.Modifiers.Include(x => x.ModifierGrp).Where(x => x.ModifierGrpId == modgrpid).Where(x => x.Isdelete == false)
           .Select(x => new ModifiersViewModel
           {
               ModifierId = x.ModifierId,
               ModifierName = x.ModifierName,
               ModifierGrpId = x.ModifierGrpId,
               Unit = x.Unit,
               Rate = x.Rate,
               Quantity = x.Quantity,
               Isdelete = x.Isdelete
           })
           .AsQueryable();

        //search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(x => x.ModifierName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<ModifiersViewModel>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> DeleteModifier(long modid)
    {
        var modofierToDelete = _context.Modifiers.FirstOrDefault(x => x.ModifierId == modid);

        modofierToDelete.ModifierName = modofierToDelete.ModifierName + DateTime.Now;
        modofierToDelete.Isdelete = true;
        _context.Update(modofierToDelete);
        await _context.SaveChangesAsync();
        return true;
    }


}