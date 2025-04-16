using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppTableService
{
    public List<OrderAppSectionVM> GetAllSectionList();
    public List<OrderAppTableVM> GetTablesBySection(long SectionId);
    Task<bool> AddCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    public long IsCustomerPresent(string Email);
    public List<CustomerViewModel> GetCustomerEmail(string searchTerm);
    Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    Task<List<WaitingTokenDetailViewModel>> GetWaitingCustomerList(long sectionid);

}