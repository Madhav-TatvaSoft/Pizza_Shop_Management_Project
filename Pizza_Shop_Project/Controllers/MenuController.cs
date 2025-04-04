using BLL.common;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly IUserLoginService _userLoginService;

        #region Menu Constructor
        public MenuController(IMenuService menuService, IUserLoginService userLoginService, IUserService userService)
        {
            _menuService = menuService;
            _userLoginService = userLoginService;
            _userService = userService;
        }
        #endregion

        #region Main-Menu-View
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.View")]
        public IActionResult Menu(long? catid, long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel MenuVM = new();
            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(_menuService.GetAllCategories(), "CategoryId", "CategoryName");


            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            if (catid == null)
            {
                MenuVM.PaginationForItemByCategory = _menuService.GetMenuItemsByCategory(MenuVM.categoryList[0].CategoryId, search, pageNumber, pageSize);
            }

            if (modgrpid == null)
            {
                MenuVM.PaginationForModifiersByModGroups = _menuService.GetMenuModifiersByModGroups(MenuVM.modifierGroupList[0].ModifierGrpId, search, pageNumber, pageSize);
            }

            ViewData["sidebar-active"] = "Menu";
            return View(MenuVM);
        }
        #endregion

        #region Pagination-Menu-Item
        [Authorize(Roles = "Admin")]
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

        #region Category CRUD

        #region Add-Category
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (await _menuService.AddCategory(category, userId))
            {
                TempData["SuccessMessage"] = NotificationMessage.EntityCreated.Replace("{0}", "Category");
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Category");
            return RedirectToAction("Menu");
        }
        #endregion

        #region Edit-Category
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> EditCategoryById(Category category)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            long Cat_Id = category.CategoryId;

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
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteCategory(long Cat_Id)
        {
            bool categoryDeleteStatus = await _menuService.DeleteCategory(Cat_Id);

            if (categoryDeleteStatus)
            {
                TempData["SuccessMessage"] = "Category deleted successfully";
                return RedirectToAction("Menu", "Menu");
            }
            TempData["ErrorMessage"] = "Failed to delete category";
            return RedirectToAction("Menu", "Menu");
        }
        #endregion

        #endregion

        #region Item CRUD

        #region Add-Items-From-Modal

        // Get the Add item Modal 
        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetItems()
        {
            MenuViewModel MenuVM = new MenuViewModel();
            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(_menuService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");
            return PartialView("_AddItemPartial", MenuVM);
        }

        // Get the Modifiers by Group in Add Item
        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetModifiersByGroup(string data)
        {
            MenuViewModel MenuVM = new MenuViewModel();

            List<ItemModifierViewModel> deserializedData = JsonConvert.DeserializeObject<List<ItemModifierViewModel>>(data);

            if (deserializedData != null)
            {
                MenuVM.addItems = MenuVM.addItems ?? new AddItemViewModel();
                MenuVM.addItems.itemModifiersVM = MenuVM.addItems.itemModifiersVM ?? new List<ItemModifierViewModel>();

                int i = 0;

                foreach (ItemModifierViewModel deItems in deserializedData)
                {
                    MenuVM.addItems.itemModifiersVM.Add(deItems);
                    MenuVM.addItems.itemModifiersVM[i].modifiersList = _menuService.GetModifiersByGroup(deItems.ModifierGrpId);
                    MenuVM.addItems.itemModifiersVM[i].ModifierGrpName = _menuService.GetModifiersGroupName(deItems.ModifierGrpId);
                    i++;
                }
            }

            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();

            ViewBag.categoryList = new SelectList(_menuService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            return PartialView("_ModifierByGroup", MenuVM);
        }

        // Add Item Post
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            List<ItemModifierViewModel> deserializedData = JsonConvert.DeserializeObject<List<ItemModifierViewModel>>(MenuVm.itemData);

            if (deserializedData != null)
            {
                MenuVm.addItems = MenuVm.addItems ?? new AddItemViewModel();
                MenuVm.addItems.itemModifiersVM = MenuVm.addItems.itemModifiersVM ?? new List<ItemModifierViewModel>();

                foreach (ItemModifierViewModel deItems in deserializedData)
                {
                    MenuVm.addItems.itemModifiersVM.Add(deItems);
                }
            }

            // Code For image upload
            if (MenuVm.addItems.ItemFormImage != null)
            {
                string[]? extension = MenuVm.addItems.ItemFormImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{MenuVm.addItems.ItemFormImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (FileStream stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        MenuVm.addItems.ItemFormImage.CopyTo(stream);
                    }
                    MenuVm.addItems.ItemImage = $"/uploads/{fileName}";
                }
                else
                {
                    return Json(new { success = false, text = "The Image format is not supported. Fill the form again !" });
                }
            }

            bool addItemStatus = await _menuService.AddItem(MenuVm.addItems, userId);

            if (addItemStatus)
            {
                return Json(new { success = true, text = "Item added successfully" });
            }
            return Json(new { success = false, text = "Failed to add Item" });
        }
        #endregion

        #region Edit-Items-From-Modal

        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetItemsByItemId(long itemid)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(_menuService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");
            MenuVM.addItems = _menuService.GetItemsByItemId(itemid);
            return PartialView("_EditItemPartial", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult EditModifiersByGroup(string data)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            List<ItemModifierViewModel> deserializedData = JsonConvert.DeserializeObject<List<ItemModifierViewModel>>(data);

            if (deserializedData != null)
            {
                MenuVM.addItems = MenuVM.addItems ?? new AddItemViewModel();
                MenuVM.addItems.itemModifiersVM = MenuVM.addItems.itemModifiersVM ?? new List<ItemModifierViewModel>();
                int i = 0;
                foreach (ItemModifierViewModel deItems in deserializedData)
                {
                    MenuVM.addItems.itemModifiersVM.Add(deItems);
                    MenuVM.addItems.itemModifiersVM[i].modifiersList = _menuService.GetModifiersByGroup(deItems.ModifierGrpId);
                    MenuVM.addItems.itemModifiersVM[i].ModifierGrpName = _menuService.GetModifiersGroupName(deItems.ModifierGrpId);
                    i++;
                }
            }

            MenuVM.categoryList = _menuService.GetAllCategories();
            MenuVM.modifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(_menuService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(_menuService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            return PartialView("_EditModifierByGroup", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            List<ItemModifierViewModel> deserializedData = JsonConvert.DeserializeObject<List<ItemModifierViewModel>>(MenuVm.itemData);

            if (deserializedData != null)
            {
                MenuVm.addItems = MenuVm.addItems ?? new AddItemViewModel();
                MenuVm.addItems.itemModifiersVM = MenuVm.addItems.itemModifiersVM ?? new List<ItemModifierViewModel>();

                foreach (ItemModifierViewModel deItems in deserializedData)
                {
                    MenuVm.addItems.itemModifiersVM.Add(deItems);
                }
            }

            if (MenuVm.addItems.ItemFormImage != null)
            {
                string[]? extension = MenuVm.addItems.ItemFormImage.FileName.Split(".");
                if (extension[extension.Length - 1] == "jpg" || extension[extension.Length - 1] == "jpeg" || extension[extension.Length - 1] == "png")
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    //create folder if not exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string fileName = $"{Guid.NewGuid()}_{MenuVm.addItems.ItemFormImage.FileName}";
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (FileStream stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        MenuVm.addItems.ItemFormImage.CopyTo(stream);
                    }
                    MenuVm.addItems.ItemImage = $"/uploads/{fileName}";
                }
                else
                {
                    return Json(new { success = false, text = "The Image format is not supported. Fill the form again !" });
                }
            }

            bool editItemStatus = await _menuService.EditItem(MenuVm.addItems, userId);

            if (editItemStatus)
            {
                return Json(new { success = true, text = "Item Updated successfully" });
            }
            return Json(new { success = false, text = "Failed to Update Item" });
        }
        #endregion

        #region Delete-Items-From-Modal
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteItem(long itemid)
        {
            bool isDeleted = await _menuService.DeleteItem(itemid);

            if (isDeleted)
            {
                return Json(new { success = true, text = "Item Deleted successfully" });
            }
            return Json(new { success = false, text = "Failed to delete Item" });

        }
        #endregion

        #endregion

        #region Pagination-Menu-Modifier
        // [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.View")]

        public IActionResult PaginationMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            try
            {
                MenuViewModel menuData = new MenuViewModel();
                menuData.modifierGroupList = _menuService.GetAllModifierGroupList();

                if (modgrpid != null)
                {
                    menuData.PaginationForModifiersByModGroups = _menuService.GetMenuModifiersByModGroups(modgrpid, search, pageNumber, pageSize);
                }

                return PartialView("_ModifierPartial", menuData.PaginationForModifiersByModGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Existing-Pagination-Menu-Modifier
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.View")]

        public IActionResult ExistingPaginationMenuModifiersByModGroups(string search = "", int pageNumber = 1, int pageSize = 3)
        {
            try
            {
                MenuViewModel menuData = new MenuViewModel();
                menuData.modifierGroupList = _menuService.GetAllModifierGroupList();

                menuData.PaginationForModifiersByModGroups = _menuService.ExistingGetMenuModifiersByModGroups(search, pageNumber, pageSize);

                return PartialView("_ExistingModifierPartial", menuData.PaginationForModifiersByModGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Modifier Group CRUD

        #region Add Modifier Group POST
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModifierGroup(MenuViewModel MenuVM)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool addModifierGroupStatus = await _menuService.AddModifierGroup(MenuVM.addModifierGroupVM, userId);
            if (addModifierGroupStatus)
            {
                return Json(new { success = true, text = "Modifier Group Added successfully" });
            }
            return Json(new { success = false, text = "Failed to Add Modifier Group" });
        }
        #endregion

        #region Edit Modifier Group 

        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetModifierGroupByModifierGroupId(long modgrpid)
        {
            List<ModifiersViewModel>? modifiers = _menuService.GetModifiersByModifierGroupId(modgrpid);
            Modifiergroup? modifierGroup = _menuService.GetModifierGroupByModifierGroupId(modgrpid);
            return Json(new { modifiers, modifierGroup });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModToModifierGrpAfterEdit(long modgrpid, long modid)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool addModToModifierGrpStatus = await _menuService.AddModToModifierGrpAfterEdit(modgrpid, modid, userId);
            if (addModToModifierGrpStatus)
            {
                return Json(new { success = true, text = "Modifier Added to Modifier Group successfully" });
            }
            return Json(new { success = false, text = "Failed to Add Modifier to Modifier Group" });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> DeleteModToModifierGrpAfterEdit(long modid, long modgrpid)
        {
            bool deleteModToModifierGrpStatus = await _menuService.DeleteModToModifierGrpAfterEdit(modid, modgrpid);
            if (deleteModToModifierGrpStatus)
            {
                return Json(new { success = true, text = "Modifier Deleted from Modifier Group successfully" });
            }
            return Json(new { success = false, text = "Failed to Delete Modifier from Modifier Group" });
        }

        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditModifierGroup(MenuViewModel MenuVM)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool editModifierGroupStatus = await _menuService.EditModifierGroup(MenuVM.addModifierGroupVM, userId);
            if (editModifierGroupStatus)
            {
                return Json(new { grpId = MenuVM.addModifierGroupVM.ModifierGrpId, success = true, text = "Modifier Group Updated successfully" });
            }
            return Json(new { success = false, text = "Failed to Update Modifier Group" });
        }

        #endregion

        #region Delete Modifier Group POST
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteModifierGroup(long modgrpid)
        {
            bool deletemodifiergrpStatus = await _menuService.DeleteModifierGroup(modgrpid);
            if (deletemodifiergrpStatus)
            {
                return Json(new { success = true, text = "Modifier Group Deleted successfully" });
            }
            return Json(new { success = false, text = "Failed to Delete Modifier Group" });
        }
        #endregion

        #endregion

        #region Get All ModifierGroup List
        public IActionResult GetAllModifierGroupList()
        {
            MenuViewModel MenuData = new();
            MenuData.modifierGroupList = _menuService.GetAllModifierGroupList();
            return PartialView("_ModifierGroupPartial", MenuData);
        }
        #endregion

        #region Modifier CRUD

        #region Add Modifier Item

        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult AddModifierItem()
        {
            MenuViewModel MenuVM = new MenuViewModel();
            List<Modifiergroup>? ModifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.modifierGroupList = new SelectList(ModifierGroupList, "ModifierGrpId", "ModifierGrpName");
            return PartialView("_AddModifierPartial", MenuVM);
        }

        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModifierItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool addModifierStatus = await _menuService.AddModifierItem(MenuVm.addModifier, userId);

            if (addModifierStatus)
            {
                // TempData["SuccessMessage"] = "Modifier added successfully";
                return Json(new { success = true, text = "Modifier Added successfully" });
            }
            // TempData["ErrorMessage"] = "Failed to add Modifier";
            return Json(new { success = false, text = "Failed to Add Modifier" });
        }
        #endregion

        #region Edit Modifier Item
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        public IActionResult GetModifiersByModifierId(long modid)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            List<Modifiergroup>? ModifierGroupList = _menuService.GetAllModifierGroupList();
            ViewBag.modifierGroupList = new SelectList(ModifierGroupList, "ModifierGrpId", "ModifierGrpName");
            MenuVM.addModifier = _menuService.GetModifiersByModifierId(modid);
            return PartialView("_EditModifierPartial", MenuVM);
        }

        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditModifierItem([FromForm] MenuViewModel MenuVm)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool editModifierStatus = await _menuService.EditModifierItem(MenuVm.addModifier, userId);

            if (editModifierStatus)
            {
                // TempData["SuccessMessage"] = "Modifier Updated successfully";
                return Json(new { success = true, text = "Modifier Updated successfully" });
            }
            // TempData["ErrorMessage"] = "Failed to Update Modifier";
            return Json(new { success = false, text = "Failed to Update Modifier" });
        }
        #endregion

        #region Delete Modifiers From Modal
        [Authorize(Roles = "Admin")]
        [PermissionAuthorize("Menu.Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteModifier(long modid)
        {
            bool isDeleted = await _menuService.DeleteModifier(modid);

            if (isDeleted)
            {
                // TempData["SuccessMessage"] = "Modifier deleted successfully";
                return Json(new { success = true, text = "Modifier Deleted Successfully" });
            }
            // TempData["ErrorMessage"] = "Modifier cannot be deleted";
            return Json(new { success = false, text = "Failed to Delete Modifier" });
        }
        #endregion

        #endregion

    }
}