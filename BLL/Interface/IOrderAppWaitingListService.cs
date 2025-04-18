using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppWaitingListService
{
    List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid);
    WaitingTokenDetailViewModel GetWaitingToken(long waitingid);
    // Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);

}
