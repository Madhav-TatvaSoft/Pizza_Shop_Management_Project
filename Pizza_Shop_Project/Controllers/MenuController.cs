using BLL.Implementation;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;

        public MenuController(MenuService menuService)
        {
            _menuService = menuService;
        }

        #region Full-Menu
        public IActionResult Menu()
        {
            MenuViewModel MenuVM = new();
            MenuVM.categories = _menuService.GetAllCategories();
            return View(MenuVM);
        }
        #endregion

        #region Add-Category
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (await _menuService.AddCategory(category))
            {
                TempData["SuccessMessage"] = "Category added successfully";
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = "Failed to add category";
            return RedirectToAction("Menu");
        }
        #endregion

        #region Edit-Category
        public async Task<IActionResult> EditCategoryById(Category category)
        {

            var Cat_Id = category.CategoryId;
            if (await _menuService.EditCategoryById(category, Cat_Id))
            {
                //change
                TempData["SuccessMessage"] = "Category Updated successfully";
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = "Failed to Update category";
            return RedirectToAction("Menu");
        }
        #endregion

        #region Delete-Category
        public async Task<IActionResult> DeleteCategory(long Cat_Id)
        {
            var categoryDeleteStatus = await _menuService.DeleteCategory(Cat_Id);


            if (categoryDeleteStatus)
            {
                TempData["SuccessMessage"] = "Category deleted successfully";
                return RedirectToAction("Menu", "Menu");
            }
            TempData["ErrorMessage"] = "Failed to delete category";
            return RedirectToAction("Menu", "Menu");
        }
        #endregion

        #region Get-Items
        


        #endregion
    }
}