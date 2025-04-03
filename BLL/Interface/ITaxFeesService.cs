
using DAL.ViewModels;

namespace BLL.Interface;

public interface ITaxFeesService
{
    PaginationViewModel<TaxViewModel> GetTaxList(int pageNumber = 1, string search = "", int pageSize = 3);

    TaxViewModel GetTaxById(long taxid);
    // Task<bool> AddTax(TaxViewModel taxVM , long userId);
    // Task<bool> EditTax(TaxViewModel taxVM , long userId);
    Task<bool> AddEditTax(TaxViewModel taxVM, long userId);
    Task<bool> DeleteTax(long taxid);

}
