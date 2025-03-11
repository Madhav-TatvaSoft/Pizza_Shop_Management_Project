using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IMenuService
{

    public List<Category> GetAllCategories();
    public List<Modifiergroup> GetAllModifierGroupList();
    public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3);
    public Task<bool> AddCategory(Category category, long userId);
    public Task<bool> EditCategoryById(Category category, long Cat_Id, long userId);
    public Task<bool> DeleteCategory(long Cat_Id);
    public Task<bool> AddItem(AddItemViewModel addItemVM, long userId);
    public AddItemViewModel GetItemsByItemId(long itemid);
    public Task<bool> EditItem(AddItemViewModel editItemVM, long userId);
    public Task<bool> DeleteItem(long itemid);
}