using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppMenuService
{
    List<ItemsViewModel> GetItems(long categoryid, string searchText = "");

    Task<bool> FavouriteItem(long itemId, bool IsFavourite);

    List<ItemModifierViewModel> GetModifiersByItemId(long itemId);

    OrderDetailViewModel GetOrderDetailsByCustomerId(long customerId);

    Task<OrderDetailViewModel> UpdateOrderDetailPartialView(List<List<int>> itemList, OrderDetailViewModel orderDetailsvm);

    Task<OrderDetailViewModel> RemoveItemfromOrderDetailPartialView(List<List<int>> itemList, int count, OrderDetailViewModel orderDetails);

    Task<OrderDetailViewModel> UpdateCustomerDetails(OrderDetailViewModel orderDetailVM, long userId);
    Task<OrderDetailViewModel> UpdateOrderComment(OrderDetailViewModel orderDetailVM, long userId);

}
