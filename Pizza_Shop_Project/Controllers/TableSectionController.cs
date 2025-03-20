using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

public class TableSectionController : Controller
{
    private readonly ITableSectionService _tableSectionService;
    private readonly IUserService _userService;
    private readonly IUserLoginService _userLoginService;

    #region Table Section Constructor
    public TableSectionController(ITableSectionService tableSectionService, IUserService userService, IUserLoginService userLoginService)
    {
        _tableSectionService = tableSectionService;
        _userService = userService;
        _userLoginService = userLoginService;
    }
    #endregion

    #region Main Table Section View
    public IActionResult TableSection(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3)
    {

        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        tableSectionVM.SectionList = _tableSectionService.GetAllSections();

        ViewBag.SectionList = new SelectList(_tableSectionService.GetAllSections(), "SectionId", "SectionName");

        if (sectionid == null)
        {
            tableSectionVM.PaginationForTable = _tableSectionService.GetTablesBySection(tableSectionVM.SectionList[0].SectionId, search, pageNumber, pageSize);
        }

        ViewData["sidebar-active"] = "TableSection";
        return View(tableSectionVM);
    }
    #endregion

    #region Pagination Table
    public IActionResult PaginationForTable(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        try
        {
            TableSectionViewModel tableSectionData = new TableSectionViewModel();
            tableSectionData.SectionList = _tableSectionService.GetAllSections();

            if (sectionid != null)
            {
                tableSectionData.PaginationForTable = _tableSectionService.GetTablesBySection(sectionid, search, pageNumber, pageSize);
            }

            return PartialView("_TableListPartial", tableSectionData.PaginationForTable);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
    #endregion

    #region Add Section

    public IActionResult AddSection()
    {
        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        return PartialView("_AddSectionPartial", tableSectionVM);
    }

    [HttpPost]
    public async Task<IActionResult> AddSection(TableSectionViewModel tableSectionVM)
    {
        string token = Request.Cookies["AuthToken"];
        var userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        var addSectionStatus = await _tableSectionService.AddSection(tableSectionVM.sectionVM, userId);
        if (addSectionStatus)
        {
            return Json(new { success = true, text = "Section Added successfully" });
        }
        return Json(new { success = false, text = "Failed to Add Section" });
    }
    #endregion

    #region Edit Section
    public IActionResult GetSectionById(long sectionId)
    {
        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        tableSectionVM.sectionVM = _tableSectionService.GetSectionById(sectionId);
        return PartialView("_EditSectionPartial", tableSectionVM);
    }

    [HttpPost]
    public async Task<IActionResult> EditSection(TableSectionViewModel tableSectionVM)
    {
        string token = Request.Cookies["AuthToken"];
        var userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        var editSectionStatus = await _tableSectionService.EditSection(tableSectionVM.sectionVM, userId);
        if (editSectionStatus)
        {
            return Json(new { success = true, text = "Section Updated successfully" });
        }
        return Json(new { success = false, text = "Failed to Update Section" });
    }
    #endregion

    #region Delete Section
    public async Task<IActionResult> DeleteSection(long sectionid)
    {
        var deleteSectionStatus = await _tableSectionService.DeleteSection(sectionid);
        if (deleteSectionStatus)
        {
            return Json(new { success = true, text = "Section Deleted successfully" });
        }
        return Json(new { success = false, text = "Failed to Delete Section" });
    }
    #endregion

    #region Get All Section List
    public IActionResult GetAllSections()
    {
        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        tableSectionVM.SectionList = _tableSectionService.GetAllSections();
        return PartialView("_SectionListPartial", tableSectionVM);
    }
    #endregion

    #region Add Table
    public IActionResult AddTable(long sectionid)
    {
        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        tableSectionVM.SectionList = _tableSectionService.GetAllSections();
        tableSectionVM.tablesVM = new TablesViewModel();
        tableSectionVM.tablesVM.SectionId = sectionid;

        return PartialView("_AddTablePartial", tableSectionVM);
    }

    [HttpPost]
    public async Task<IActionResult> AddTable([FromForm] TableSectionViewModel tableSectionVM)
    {
        string token = Request.Cookies["AuthToken"];
        var userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        var addTableStatus = await _tableSectionService.AddTable(tableSectionVM.tablesVM, userId);
        if (addTableStatus)
        {
            return Json(new { success = true, text = "Table Added successfully" });
        }
        return Json(new { success = false, text = "Failed to Add Table" });
    }
    #endregion

    #region Edit Table
    public async Task<IActionResult> GetTableById(long tableId, long sectionId)
    {
        TableSectionViewModel tableSectionVM = new TableSectionViewModel();
        tableSectionVM.SectionList = _tableSectionService.GetAllSections();
        // ViewBag.SectionList = new SelectList(SectionList, "SectionId", "SectionName");
        tableSectionVM.tablesVM = _tableSectionService.GetTableById(tableId, sectionId);
        return PartialView("_EditTablePartial", tableSectionVM);
    }

    [HttpPost]
    public async Task<IActionResult> EditTable([FromForm] TableSectionViewModel tableSectionVM)
    {
        string token = Request.Cookies["AuthToken"];
        var userData = _userService.getUserFromEmail(token);
        long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

        var editTableStatus = await _tableSectionService.EditTable(tableSectionVM.tablesVM, userId);
        if (editTableStatus)
        {
            return Json(new { success = true, text = "Table Updated successfully" });
        }
        return Json(new { success = false, text = "Failed to Update Table" });
    }
    #endregion

    #region Delete Table

    [HttpPost]
    public async Task<IActionResult> DeleteTable(long tableid)
    {
        var deleteTableStatus = await _tableSectionService.DeleteTable(tableid);
        if (deleteTableStatus)
        {
            return Json(new { success = true, text = "Table Deleted successfully" });
        }
        return Json(new { success = false, text = "Failed to Delete Table" });
    }
    #endregion

}