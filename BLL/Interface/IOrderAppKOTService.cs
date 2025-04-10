using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppKOTService
{
    Task<List<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter);
    Task<OrderAppKOTViewModel> GetKOTItemsFromModal(long catid, string filter, long orderid);
    Task<bool> UpdateKOTStatus( string filter, int[] orderDetailId, int[] quantity);

}
