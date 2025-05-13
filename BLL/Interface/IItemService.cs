using DAL.ViewModels;

namespace BLL.Interface;

public interface IItemService
{
    PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3);
    Task<bool> AddItem(AddItemViewModel addItemVM, long userId);
    AddItemViewModel GetItemsByItemId(long itemid);
    Task<bool> EditItem(AddItemViewModel editItemVM, long userId);
    Task<bool> DeleteItem(long itemid ,long userId);
    bool IsItemExist(AddItemViewModel ItemVM);

}