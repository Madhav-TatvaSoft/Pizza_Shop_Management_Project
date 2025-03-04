using DAL.Models;

namespace BLL.Interface;

public interface IMenuService
{

    public  Task<bool> AddCategory(Category category);
    public  Task<bool> EditCategoryById(Category category, long Cat_Id);
    public  Task<bool> DeleteCategory(long Cat_Id);

}
