using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppKOTService
{
    Task<PaginationViewModel<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter,int page = 1,int pageSize = 5);
    Task<OrderAppKOTViewModel> GetKOTItemsFromModal(long catid, string filter, long orderid);
    Task<bool> UpdateKOTStatus( string filter, int[] orderDetailId, int[] quantity);

}
