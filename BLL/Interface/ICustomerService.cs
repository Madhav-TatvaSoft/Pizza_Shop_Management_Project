using DAL.ViewModels;

namespace BLL.Interface;

public interface ICustomerService
{
    PaginationViewModel<CustomerViewModel> GetCustomerList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "");
    Task<byte[]> ExportData(string search = "", string fromDate = "", string toDate = "", string selectRange = "");
    CustomerHistoryViewModel GetCustomerHistory(long customerid);
    long IsCustomerPresent(string Email);
    List<CustomerViewModel> GetCustomerEmail(string searchTerm);
    Task<bool> AddEditCustomer(WaitingTokenDetailViewModel waitingTokenVM, long userId);
    IQueryable<CustomerViewModel> GetAllCustomers();
}