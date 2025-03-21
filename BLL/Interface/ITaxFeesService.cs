using DAL.ViewModels;

namespace BLL.Interface;

public interface ITaxFeesService
{
    public PaginationViewModel<TaxViewModel> GetTaxList(string search = "", int pageNumber = 1, int pageSize = 3);
    
    public Task<bool> DeleteTax(long taxid);

}
