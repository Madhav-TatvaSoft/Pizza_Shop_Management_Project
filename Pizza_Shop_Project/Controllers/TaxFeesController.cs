using BLL.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class TaxFeesController : Controller
{
    private readonly ITaxFeesService _taxFeesService;

    public TaxFeesController(ITaxFeesService taxFeesService)
    {
        _taxFeesService = taxFeesService;
    }

    public IActionResult TaxFees(){
        ViewData["sidebar-active"] = "TaxFees";
        return View();
    }

    public IActionResult PaginationForTax(int page = 1,string search = "",int pageSize = 3) {

        var taxList = _taxFeesService.GetTaxList(search, page, pageSize);

        return PartialView("_TaxListDataPartial", taxList);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTaxAsync(long taxid) {

       var deleteTaxStatus = await _taxFeesService.DeleteTax(taxid);
        if (deleteTaxStatus)
        {
            return Json(new { success = true, text = "Tax Deleted successfully" });
        }
        return Json(new { success = false, text = "Failed to Delete Tax" });
    }
}
