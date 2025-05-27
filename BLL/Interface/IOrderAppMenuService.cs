using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppMenuService
{
    List<ItemsViewModel> GetItems(long categoryid, string searchText = "");
    Task<bool> FavouriteItem(long itemId, bool IsFavourite, long userId);
    List<ItemModifierViewModel> GetModifiersByItemId(long itemId);
    OrderDetailViewModel GetOrderDetailsByCustomerId(long customerId);
    Task<OrderDetailViewModel> UpdateOrderDetailPartialView(List<List<int>> itemList, OrderDetailViewModel orderDetailsvm);
    Task<OrderDetailViewModel> RemoveItemfromOrderDetailPartialView(List<List<int>> itemList, int count, OrderDetailViewModel orderDetails);
    // Task<OrderDetailViewModel> UpdateCustomerDetails(OrderDetailViewModel orderDetailVM, long userId);
    // Task<OrderDetailViewModel> UpdateOrderComment(OrderDetailViewModel orderDetailVM, long userId);
    Task<OrderDetailViewModel> SaveOrder(List<int> orderDetailIds, OrderDetailViewModel orderDetailsVM, long userId);
    Task<bool> IsItemsReady(List<int> orderDetailId,OrderDetailViewModel orderDetailsVM);
    Task<bool> CompleteOrder(OrderDetailViewModel orderDetailsVM, long userId);
    Task<bool> SaveRatings(OrderDetailViewModel orderDetailVM, long userId);
    Task<bool> IsAnyItemReady(OrderDetailViewModel orderDetailsVM);
    Task<bool> CancelOrder(OrderDetailViewModel orderDetailsVM, long userId);

}