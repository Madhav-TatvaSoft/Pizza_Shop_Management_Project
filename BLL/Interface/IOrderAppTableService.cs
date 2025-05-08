using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppTableService
{
    public List<OrderAppSectionVM> GetAllSectionList();
    public List<OrderAppTableVM> GetTablesBySection(long SectionId);
    bool CheckTokenExists(WaitingTokenDetailViewModel waitingTokenVM);
    Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid);
    Task<WaitingTokenDetailViewModel> GetCustomerDetails(long waitingId);
    Task<bool> AssignTable(OrderAppTableMainViewModel TableMainVM, long userId);

}