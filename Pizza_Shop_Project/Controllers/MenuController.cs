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

        public IActionResult Menu()
        {
            return View();
        }

        public async Task<IActionResult> AddCategory(Category category)
        {

            if (!await _menuService.AddCategory(category))
            {
                //change
                TempData["SuccessMessage"] = "Category added successfully";
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = "Failed to add category.Try Again !";
            return RedirectToAction("Menu");
        }

    }
}
