using BLL.Interface;
using DAL.Models;

namespace BLL.Implementation;

public class MenuService : IMenuService
{
    private readonly PizzaShopDbContext _context;

    public MenuService(PizzaShopDbContext context){
        _context = context;
    }
    public bool AddCategory(Category category)
    {
        if (category == null) 
        { 
            return false; 
        }else{
            Category cat = new Category();
            cat.CategoryName = category.CategoryName;
            cat.Description = category.Description;


            return true;
        }
    }

}
