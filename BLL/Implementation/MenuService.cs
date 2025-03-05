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

    public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        var query = _context.Items
           .Include(x => x.Category).Include(x => x.ItemType)
           .Where(x => x.CategoryId == catid)
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


    public async Task<bool> AddCategory(Category category,long userId)
    {
        var isCategoryExistsAdd = _context.Categories.FirstOrDefault(x => x.CategoryName == category.CategoryName);
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

    public async Task<bool> EditCategoryById(Category category, long Cat_Id,long userId)
    {
        if (category == null || Cat_Id == null)
        {
            return false;
        }
        else
        {
            var isCategoryExistsEdit = _context.Categories.FirstOrDefault(x => x.CategoryName == category.CategoryName);
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
}
