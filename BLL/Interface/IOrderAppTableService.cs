using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppTableService
{
    public List<OrderAppSectionVM> GetAllSectionList();
    public List<OrderAppTableVM> GetTablesBySection(long SectionId);
    Task<bool> AddCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    public long IsCustomerPresent(string Email);
    Task<bool> AddCustomerToWaitingList(WaitingTokenDetailViewModel waitingTokenVM, long userId);

}