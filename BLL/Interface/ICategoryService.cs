using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategories();
    Task<bool> AddCategory(Category category, long userId);
    Task<bool> EditCategory(Category category, long Cat_Id, long userId);
    Task<bool> DeleteCategory(long Cat_Id);
    bool IsCategoryExistForAdd(Category category);
    bool IsCategoryExistForEdit(Category category);

}