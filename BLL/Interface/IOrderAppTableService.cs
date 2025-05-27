using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppTableService
{
    Task<List<OrderAppSectionVM>> GetAllSectionList();
    List<OrderAppTableVM> GetTablesBySection(long SectionId);
    bool CheckTokenExists(WaitingTokenDetailViewModel waitingTokenVM);
    Task<bool> SaveCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid);
    Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId);
    Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId);

}