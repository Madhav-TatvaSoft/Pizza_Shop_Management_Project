using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;

namespace BLL.Implementation;

public class TaxFeesService : ITaxFeesService
{
    private readonly PizzaShopDbContext _context;

    public TaxFeesService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public PaginationViewModel<TaxViewModel> GetTaxList(string search = "", int pageNumber = 1, int pageSize = 3)
    {
        var query = _context.Taxes
          .Where(x => x.Isdelete == false).OrderBy(x => x.TaxId)
          .Select(x => new TaxViewModel
          {
              TaxId = x.TaxId,
              TaxName = x.TaxName,
              TaxType = x.TaxType,
              TaxValue = x.TaxValue,
              Isenable = x.Isenable,
              Isdefault = x.Isdefault,
              Isdelete = x.Isdelete
          })
          .AsQueryable();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.TaxName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<TaxViewModel>(items, totalCount, pageNumber, pageSize);
    }

    #region Delete Tax
    public async Task<bool> DeleteTax(long taxid)
    {
        var tax = _context.Taxes.FirstOrDefault(x => x.TaxId == taxid && x.Isdelete == false);
        if (tax != null)
        {
            tax.TaxName = tax.TaxName + DateTime.Now;
            tax.Isdelete = true;
            _context.Taxes.Update(tax);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    #endregion

}
