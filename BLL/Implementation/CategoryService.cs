using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace BLL.Implementation;
public class CategoryService : ICategoryService
{
    private readonly PizzaShopDbContext _context;
    public CategoryService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAll()
    {
        return await _context.Categories.Where(x => !x.Isdelete).OrderBy(x => x.CategoryId).ToListAsync();
    }

    public async Task<bool> Add(Category category, long userId)
    {

        if (category == null) return false;

        Category cat = new Category();
        cat.CategoryName = category.CategoryName;
        cat.Description = category.Description;
        cat.CreatedBy = userId;
        cat.CreatedAt = DateTime.Now;
        cat.Isdelete = false;
        await _context.Categories.AddAsync(cat);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> Update(Category category, long Cat_Id, long userId)
    {

        if (category == null || Cat_Id == null)
        {
            return false;
        }

        Category? cat = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryId == Cat_Id && !x.Isdelete);
        if (cat == null)
        {
            return false;
        }
        cat.CategoryName = category.CategoryName;
        cat.Description = category.Description;
        cat.ModifiedBy = userId;
        cat.ModifiedAt = DateTime.Now;
        _context.Categories.Update(cat);
        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> Delete(long Cat_Id, long userId)
    {

        if (Cat_Id == null)
        {
            return false;
        }

        Category? category = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryId == Cat_Id && !x.Isdelete);
        if (category == null)
        {
            return false;
        }

        var items = await _context.Items.Where(x => x.CategoryId == Cat_Id && !x.Isdelete).ToListAsync();
        if (items != null)
        {
            foreach (var item in items)
            {
                item.Isdelete = true;
                item.ModifiedAt = DateTime.Now;
                item.ModifiedBy = userId;
                _context.Items.Update(item);
            }
        }

        category.Isdelete = true;
        category.ModifiedAt = DateTime.Now;
        category.ModifiedBy = userId;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return true;

    }

    public bool IsExist(Category category)
    {
        if (category.CategoryId == 0)
        {
            return _context.Categories.Any(x => !x.Isdelete && x.CategoryName.ToLower().Trim() == category.CategoryName.ToLower().Trim());
        }
        else
        {
            return _context.Categories.Any(x => x.CategoryId != category.CategoryId && x.CategoryName.ToLower().Trim() == category.CategoryName.ToLower().Trim() && !x.Isdelete);
        }
    }

}