using BLL.Implementation;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;

        private readonly UserLoginService _userLoginService;

        public MenuController(MenuService menuService, UserLoginService userLoginService)
        {
            _menuService = menuService;
            _userLoginService = userLoginService;
        }

        #region Main-Menu-View
        public IActionResult Menu(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel MenuVM = new();
            MenuVM.categoryList = _menuService.GetAllCategories();

            if (catid == null)
            {
                MenuVM.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(MenuVM.categoryList[0].CategoryId, search, pageNumber, pageSize);
            }

            if (catid != null)
            {
                MenuVM.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(catid, search, pageNumber, pageSize);
            }
            return View(MenuVM);
        }
        #endregion

        #region Pagination-Menu-Item

        public IActionResult PaginationMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel menuData = new MenuViewModel();
            menuData.categoryList = _menuService.GetAllCategories();

            if (catid != null)
            {
                menuData.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(catid, search, pageNumber, pageSize);
            }
            return PartialView("_ItemPartialView", menuData.PaginationForItemByCategory);
        }
        #endregion

        #region Add-Category
        public async Task<IActionResult> AddCategory(Category category)
        {
            string Email = Request.Cookies["Email"];
            long userId = _userLoginService.GetUserId(Email);

            if (await _menuService.AddCategory(category, userId))
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
            string Email = Request.Cookies["Email"];
            long userId = _userLoginService.GetUserId(Email);

            var Cat_Id = category.CategoryId;

            if (await _menuService.EditCategoryById(category, Cat_Id, userId))
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

        #region Add-Items-From-Modal
        public async Task<IActionResult> AddItem(MenuViewModel MenuVm)
        {
            string Email = Request.Cookies["Email"];
            long userId = _userLoginService.GetUserId(Email);

            if (MenuVm.addItems.ItemFormImage != null)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                //create folder if not exist
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = $"{Guid.NewGuid()}_{MenuVm.addItems.ItemFormImage.FileName}";
                string fileNameWithPath = Path.Combine(path, fileName);

                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    MenuVm.addItems.ItemFormImage.CopyTo(stream);
                }
                MenuVm.addItems.ItemImage = $"/uploads/{fileName}";
            }

            var addItemStatus = await _menuService.AddItem(MenuVm.addItems, userId);

            if (addItemStatus)
            {
                TempData["SuccessMessage"] = "Item added successfully";
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = "Failed to add Item";
            return RedirectToAction("Menu");
        }
        #endregion

    }
}