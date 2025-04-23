using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppWaitingListService
{
    List<WaitingTokenDetailViewModel> GetWaitingList(long sectionid);
    WaitingTokenDetailViewModel GetWaitingToken(long waitingid);
    // Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);

    Task<bool> DeleteWaitingToken(long waitingid);
    List<OrderAppTableVM> GetAvailableTables(long sectionid);

    Task<bool> AssignTableInWaiting(long waitingId, long sectionId,long customerid, int persons, int[] tableIds, long userId);

}
