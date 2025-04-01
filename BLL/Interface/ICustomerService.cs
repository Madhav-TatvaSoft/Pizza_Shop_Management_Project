using DAL.ViewModels;

namespace BLL.Interface;

public interface ICustomerService
{
    public PaginationViewModel<CustomerViewModel> GetCustomerList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5, string fromDate = "", string toDate = "", string selectRange = "");
    public Task<byte[]> ExportData(string search = "", string fromDate = "", string toDate = "", string selectRange = "");

}
