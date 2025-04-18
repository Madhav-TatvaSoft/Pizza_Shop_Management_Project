using BLL.Interface;
using DAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

public class TaxFeesController : Controller
{
    private readonly ITaxFeesService _taxFeesService;
    private readonly IUserService _userService;
    private readonly IUserLoginService _userLoginService;

    #region Constructor
    public TaxFeesController(ITaxFeesService taxFeesService, IUserService userService, IUserLoginService userLoginService)
    {
        _userService = userService;
        _userLoginService = userLoginService;
        _taxFeesService = taxFeesService;
    }
    #endregion

    #region Main Tax Fees View
    [PermissionAuthorize("TaxFees.View")]
    public IActionResult TaxFees()
    {
        ViewData["sidebar-active"] = "TaxFees";
        return View();
    }
    #endregion

    #region Pagination Tax
    [PermissionAuthorize("TaxFees.View")]
    public IActionResult PaginationForTax(int pageNumber = 1, string search = "", int pageSize = 3)
    {
        PaginationViewModel<TaxViewModel>? taxList = _taxFeesService.GetTaxList(pageNumber, search, pageSize);
        return PartialView("_TaxListDataPartial", taxList);
    }
    #endregion

    #region Tax CRUD

    #region Add Edit Tax GET
    [PermissionAuthorize("TaxFees.Delete")]
    public IActionResult AddEditTax(long taxid)
    {
        TaxFeesViewModel taxFeesVM = new TaxFeesViewModel();
        if (taxid == 0)
        {
            taxFeesVM.taxVM = new TaxViewModel();
        }
        else
        {
            taxFeesVM.taxVM = _taxFeesService.GetTaxById(taxid);
        }

        return PartialView("_AddEditTaxPartial", taxFeesVM);
    }

    #endregion

    #region Add Edit Tax POST

    // [HttpPost]
    // public async Task<IActionResult> AddEditTax([FromForm] TaxFeesViewModel taxFeesVM)
    // {
    //     string? token = Request.Cookies["AuthToken"];
    //     List<User>? userData = _userService.getUserFromEmail(token);
    //     long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

    //     if (taxFeesVM.taxVM.TaxId == 0)
    //     {
    //         bool addTaxStatus = await _taxFeesService.AddTax(taxFeesVM.taxVM, userId);
    //         if (addTaxStatus)
    //         {
    //             return Json(new { success = true, text = "Tax Added successfully" });
    //         }
    //         return Json(new { success = false, text = "Failed to Add Tax, Check If already exists!" });
    //     }
    //     else
    //     {
    //         bool editTaxStatus = await _taxFeesService.EditTax(taxFeesVM.taxVM, userId);
    //         if (editTaxStatus)
    //         {
    //             return Json(new { success = true, text = "Tax Updated successfully" });
    //         }
    //         return Json(new { success = false, text = "Failed to Update Tax, Check If already exists!" });
    //     }
    // }
    [PermissionAuthorize("TaxFees.AddEdit")]
    [HttpPost]
    public async Task<IActionResult> AddEditTax([FromForm] TaxFeesViewModel taxFeesVM)
    {
        string? token = Request.Cookies["AuthToken"];
        List<User>? userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        bool taxStatus = await _taxFeesService.AddEditTax(taxFeesVM.taxVM, userId);
        return Json(taxStatus
            ? new { success = true, text = taxFeesVM.taxVM.TaxId == 0 ? "Tax Added successfully" : "Tax Updated successfully" }
            : new { success = false, text = $"Failed to {(taxFeesVM.taxVM.TaxId == 0 ? "Add" : "Update")} Tax, Check If already exists!" });
    }
    #endregion

    #region Delete Tax
    [PermissionAuthorize("TaxFees.Delete")]
    [HttpPost]
    public async Task<IActionResult> DeleteTaxAsync(long taxid)
    {
        bool deleteTaxStatus = await _taxFeesService.DeleteTax(taxid);
        if (deleteTaxStatus)
        {
            return Json(new { success = true, text = "Tax Deleted successfully" });
        }
        return Json(new { success = false, text = "Failed to Delete Tax" });
    }
    #endregion

    #endregion

}