using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class TaxFeesService : ITaxFeesService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public TaxFeesService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Get Tax List
    public PaginationViewModel<TaxViewModel> GetTaxList(int pageNumber = 1, string search = "", int pageSize = 3)
    {
        var query = _context.Taxes
          .Where(x => x.Isdelete == false).OrderBy(x => x.TaxId)
          .Select(x => new TaxViewModel
          {
              TaxId = x.TaxId,
              TaxName = x.TaxName,
              TaxType = x.TaxType,
              TaxValue = x.TaxValue,
              Isenable = (bool)x.Isenable,
              Isdefault = (bool)x.Isdefault,
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
    #endregion

    #region Get Tax By Id
    public TaxViewModel GetTaxById(long taxid)
    {
        var tax = _context.Taxes.FirstOrDefault(x => x.TaxId == taxid && x.Isdelete == false);
        if (tax != null)
        {
            TaxViewModel taxVM = new TaxViewModel
            {
                TaxId = tax.TaxId,
                TaxName = tax.TaxName,
                TaxType = tax.TaxType,
                TaxValue = tax.TaxValue,
                Isenable = (bool)tax.Isenable,
                Isdefault = (bool)tax.Isdefault,
                Isdelete = tax.Isdelete
            };
            return taxVM;
        }
        return null;
    }
    #endregion

    #region Add Tax
    public async Task<bool> AddTax(TaxViewModel taxVM, long userId)
    {
        var isTaxExist = await _context.Taxes.FirstOrDefaultAsync(x => x.TaxName.ToLower().Trim() == taxVM.TaxName.ToLower().Trim() && x.Isdelete == false);
        if (isTaxExist != null)
        {
            return false;
        }

        Tax tax = new Tax
        {
            TaxName = taxVM.TaxName,
            TaxType = taxVM.TaxType,
            TaxValue = taxVM.TaxValue,
            Isenable = taxVM.Isenable,
            Isdefault = taxVM.Isdefault,
            Isdelete = false,
            CreatedAt = DateTime.Now,
            CreatedBy = userId,
        };
        await _context.Taxes.AddAsync(tax);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Edit Tax
    public async Task<bool> EditTax(TaxViewModel taxVM, long userId)
    {
        var isTaxExist = await _context.Taxes.FirstOrDefaultAsync(x => x.TaxId != taxVM.TaxId && x.TaxName.ToLower().Trim() == taxVM.TaxName.ToLower().Trim() && x.Isdelete == false);

        if (isTaxExist != null)
        {
            return false;
        }

        var tax = _context.Taxes.FirstOrDefault(x => x.TaxId == taxVM.TaxId && x.Isdelete == false);
        if (tax != null)
        {
            tax.TaxName = taxVM.TaxName;
            tax.TaxType = taxVM.TaxType;
            tax.TaxValue = taxVM.TaxValue;
            tax.Isenable = taxVM.Isenable;
            tax.Isdefault = taxVM.Isdefault;
            tax.ModifiedAt = DateTime.Now;
            tax.ModifiedBy = userId;
            _context.Taxes.Update(tax);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    #endregion

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