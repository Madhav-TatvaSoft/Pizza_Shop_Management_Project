using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderService
{
    public PaginationViewModel<OrdersViewModel> GetOrderList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string orderStatus = "", string fromDate = "", string toDate = "", string selectRange = "");
    public Task<byte[]> ExportData(string search = "", string orderStatus = "", string selectRange = "");
    public OrderDetailViewModel GetOrderDetails(long orderid);

}
