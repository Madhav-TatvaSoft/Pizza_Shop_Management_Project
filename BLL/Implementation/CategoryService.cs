using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace BLL.Implementation;
public class CategoryService : ICategoryService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public CategoryService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Get List

    public async Task<List<Category>> GetAllCategories()
    {
        return await _context.Categories.Where(x => !x.Isdelete).OrderBy(x => x.CategoryId).ToListAsync();
    }
    #endregion

    #region Add 
    public async Task<bool> AddCategory(Category category, long userId)
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
    #endregion

    #region Edit 
    public async Task<bool> EditCategory(Category category, long Cat_Id, long userId)
    {
        if (category == null || Cat_Id == null)
        {
            return false;
        }

        Category cat = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryId == Cat_Id && !x.Isdelete);
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
    #endregion

    #region Delete 
    public async Task<bool> DeleteCategory(long Cat_Id)
    {
        Category category = await _context.Categories.FirstOrDefaultAsync(x => x.CategoryId == Cat_Id && !x.Isdelete);

        List<Item> ItemsInCategory = await _context.Items.Where(x => x.CategoryId == Cat_Id && !x.Isdelete).ToListAsync();

        // if (ItemsInCategory.Count > 0)
        // {
        //     foreach (var item in ItemsInCategory)
        //     {
        //         item.Isdelete = true;
        //         item.ModifiedAt = DateTime.Now;
        //         _context.Items.Update(item);
        //         await _context.SaveChangesAsync();
        //     }
        // }

        for (int i = 0; i < ItemsInCategory.Count; i++)
        {
            // ItemsInCategory[i].ItemName = ItemsInCategory[i].ItemName + DateTime.Now;
            ItemsInCategory[i].Isdelete = true;
            ItemsInCategory[i].ModifiedAt = DateTime.Now;
            _context.Items.Update(ItemsInCategory[i]);
            await _context.SaveChangesAsync();
        }

        // category.CategoryName = category.CategoryName + DateTime.Now;
        if (category == null)
        {
            return false;
        }

        category.Isdelete = true;
        category.ModifiedAt = DateTime.Now;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return true;
    }
    #endregion

    #region Check Category Exist
    public bool IsCategoryExistForAdd(Category category)
    {
        return _context.Categories.Any(x => !x.Isdelete && x.CategoryName.ToLower().Trim() == category.CategoryName.ToLower().Trim());
    }

    public bool IsCategoryExistForEdit(Category category)
    {
        return _context.Categories.Any(x => x.CategoryId != category.CategoryId && x.CategoryName.ToLower().Trim() == category.CategoryName.ToLower().Trim() && !x.Isdelete);

    }
    #endregion

}