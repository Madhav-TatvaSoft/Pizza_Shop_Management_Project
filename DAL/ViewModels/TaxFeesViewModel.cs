using DAL.Models;

namespace DAL.ViewModels;

public class TaxFeesViewModel
{
    public Tax taxes { get; set; }
    public TaxViewModel taxVM { get; set; }
    public PaginationViewModel<TaxFeesViewModel> GetTaxList{get; set;}

}
