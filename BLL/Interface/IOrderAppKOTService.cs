using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppKOTService
{
    Task<List<OrderAppKOTViewModel>> GetKOTItems(long catid, string filter);

}
