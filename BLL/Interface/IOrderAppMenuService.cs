using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppMenuService
{
    List<ItemsViewModel> GetItems(long categoryid, string searchText = "");

    Task<bool> FavouriteItem(long itemId, bool IsFavourite);

}
