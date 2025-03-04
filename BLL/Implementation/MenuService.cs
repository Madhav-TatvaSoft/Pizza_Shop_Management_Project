using BLL.Interface;
using DAL.Models;
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

    public async Task<bool> AddCategory(Category category)
    {
        var isCategoryExists = _context.Categories.FirstOrDefault(x => x.CategoryName == category.CategoryName);
        if (category != null || isCategoryExists == null )
        {
            Category cat = new Category();
            cat.CategoryName = category.CategoryName;
            cat.Description = category.Description;
            await _context.Categories.AddAsync(cat);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> EditCategoryById(Category category, long Cat_Id)
    {
        if (category == null || Cat_Id == null)
        {
            return false;
        }
        else
        {
            Category cat = _context.Categories.FirstOrDefault(x => x.CategoryId == Cat_Id);
            cat.CategoryName = category.CategoryName;
            cat.Description = category.Description;
            _context.Categories.Update(cat);
            await _context.SaveChangesAsync();
            return true;
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
