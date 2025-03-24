
using DAL.ViewModels;

namespace BLL.Interface;

public interface ITaxFeesService
{
    public PaginationViewModel<TaxViewModel> GetTaxList(int pageNumber = 1,string search = "",  int pageSize = 3);  

    public TaxViewModel GetTaxById(long taxid);
    public Task<bool> AddTax(TaxViewModel taxVM , long userId);
    public Task<bool> EditTax(TaxViewModel taxVM , long userId);
    public Task<bool> DeleteTax(long taxid);

}
