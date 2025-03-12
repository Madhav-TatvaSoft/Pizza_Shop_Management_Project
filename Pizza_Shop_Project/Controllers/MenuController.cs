using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;

        private readonly IUserLoginService _userLoginService;

        public MenuController(IMenuService menuService, IUserLoginService userLoginService, IUserService userService)
        {
            _menuService = menuService;
            _userLoginService = userLoginService;
            _userService = userService;
        }

        #region Main-Menu-View
        [PermissionAuthorize("Menu.View")]
        public IActionResult Menu(long? catid, long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel MenuVM = new();
            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();

            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");
            if (catid == null)
            {
                MenuVM.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(MenuVM.categoryList[0].CategoryId, search, pageNumber, pageSize);
            }

            if (catid != null)
            {
                MenuVM.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(catid, search, pageNumber, pageSize);
            }
            if (modgrpid == null)
            {
                MenuVM.PaginationForModifiersByModGroups = _menuService.GetMenuModifiersByModGroups(MenuVM.modifierGroupList[0].ModifierGrpId, search, pageNumber, pageSize);
            }
            if (modgrpid != null)
            {
                MenuVM.PaginationForModifiersByModGroups = _menuService.GetMenuModifiersByModGroups(modgrpid, search, pageNumber, pageSize);
            }

            ViewData["sidebar-active"] = "Menu";
            return View(MenuVM);
        }
        #endregion

        #region Pagination-Menu-Item
        [PermissionAuthorize("Menu.View")]
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
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            string token = Request.Cookies["AuthToken"];
            var userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

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
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> EditCategoryById(Category category)
        {
            string token = Request.Cookies["AuthToken"];
            var userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            var Cat_Id = category.CategoryId;

            if (await _menuService.EditCategoryById(category, Cat_Id, userId))
            {
                //change
                TempData["SuccessMessage"] = "Category Updated successfully";
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = "Failed to Update category, Check if Category already exists?";
            return RedirectToAction("Menu");
        }
        #endregion

        #region Delete-Category
        [PermissionAuthorize("Menu.Delete")]
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
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            var userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (MenuVm.addItems.ItemFormImage != null)
            {
                var extension = MenuVm.addItems.ItemFormImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
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
                else
                {
                    TempData["ErrorMessage"] = "The Image format is not supported. Fill the form again !";
                    return RedirectToAction("Menu", "Menu");
                }
            }

            var addItemStatus = await _menuService.AddItem(MenuVm.addItems, userId);

            if (addItemStatus)
            {
                TempData["SuccessMessage"] = "Item added successfully";
                return Json(new { });
            }
            TempData["ErrorMessage"] = "Failed to add Item";
            return RedirectToAction("Menu");
        }
        #endregion

        #region Delete-Items-From-Modal
        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteItem(long itemid)
        {
            var isDeleted = await _menuService.DeleteItem(itemid);

            if (!isDeleted)
            {
                TempData["ErrorMessage"] = "Item cannot be deleted";
                return RedirectToAction("Menu", "Menu");
            }
            TempData["SuccessMessage"] = "Item deleted successfully";
            return RedirectToAction("Menu", "Menu");
        }
        #endregion

        #region Edit-Items-From-Modal
        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetItemsByItemId(long itemid)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.addItems = _menuService.GetItemsByItemId(itemid);
            return PartialView("_EditItemPartial", MenuVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            var userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (MenuVm.addItems.ItemFormImage != null)
            {
                var extension = MenuVm.addItems.ItemFormImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
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
                else
                {
                    TempData["ErrorMessage"] = "The Image format is not supported. Fill the form again !";
                    return RedirectToAction("Menu", "Menu");
                }
            }

            var editItemStatus = await _menuService.EditItem(MenuVm.addItems, userId);

            if (editItemStatus)
            {
                // TempData["SuccessMessage"] = "Item Updated successfully";
                return Json(new { });
            }
            TempData["ErrorMessage"] = "Failed to Update Item";
            return RedirectToAction("Menu");
        }
        #endregion

        #region Pagination-Menu-Modifier
        [PermissionAuthorize("Menu.View")]

        public IActionResult PaginationMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel menuData = new MenuViewModel();
            menuData.modifierGroupList = _menuService.GetAllModifierGroupList();

            if (modgrpid != null)
            {
                menuData.PaginationForModifiersByModGroups = _menuService.GetMenuModifiersByModGroups(modgrpid, search, pageNumber, pageSize);
            }
            return PartialView("_ModifierPartial", menuData.PaginationForModifiersByModGroups);
        }
        #endregion
    
        #region Delete-Modifiers-From-Modal
        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteModifier(long modid)
        {
            var isDeleted = await _menuService.DeleteModifier(modid);

            if (!isDeleted)
            {
                TempData["ErrorMessage"] = "Modifier cannot be deleted";
                return RedirectToAction("Menu", "Menu");
            }
            TempData["SuccessMessage"] = "Modifier deleted successfully";
            return RedirectToAction("Menu", "Menu");
        }
        #endregion
        
    }
}