using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IMenuService
{

    public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3);

    public Task<bool> AddCategory(Category category,long userId);
    public Task<bool> EditCategoryById(Category category, long Cat_Id,long userId);
    public Task<bool> DeleteCategory(long Cat_Id);

}
