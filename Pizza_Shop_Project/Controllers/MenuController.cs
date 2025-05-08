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
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IModifierGroupService _modifierGroupService;
        private readonly IModifierItemService _modifierItemService;
        private readonly IUserService _userService;
        private readonly IUserLoginService _userLoginService;

        public MenuController(IUserLoginService userLoginService, IUserService userService, ICategoryService categoryService, IItemService itemService, IModifierGroupService modifierGroupService, IModifierItemService modifierItemService)
        {
            _userLoginService = userLoginService;
            _userService = userService;
            _categoryService = categoryService;
            _itemService = itemService;
            _modifierGroupService = modifierGroupService;
            _modifierItemService = modifierItemService;
        }

        #region Main-Menu-View
        [PermissionAuthorize("Menu.View")]
        public async Task<IActionResult> Menu(long? catid, long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel MenuVM = new();
            MenuVM.categoryList = await _categoryService.GetAllCategories();
            MenuVM.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "CategoryName");


            ViewBag.modifierGroupList = new SelectList(await _modifierGroupService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            if (catid == null)
            {
                MenuVM.PaginationForItemByCategory = _itemService.GetMenuItemsByCategory(MenuVM.categoryList[0].CategoryId, search, pageNumber, pageSize);
            }

            if (modgrpid == null)
            {
                MenuVM.PaginationForModifiersByModGroups = _modifierGroupService.GetMenuModifiersByModGroups(MenuVM.modifierGroupList[0].ModifierGrpId, search, pageNumber, pageSize);
            }

            ViewData["sidebar-active"] = "Menu";
            return View(MenuVM);
        }
        #endregion

        [PermissionAuthorize("Menu.View")]
        public async Task<IActionResult> PaginationMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            MenuViewModel menuData = new MenuViewModel();
            menuData.categoryList = await _categoryService.GetAllCategories();

            if (catid != null)
            {
                menuData.PaginationForItemByCategory = _itemService.GetMenuItemsByCategory(catid, search, pageNumber, pageSize);
            }
            return PartialView("_ItemPartialView", menuData.PaginationForItemByCategory);
        }

        #region Category CRUD

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> AddCategory(Category category)
        {
            bool IsCategoryNameExists = _categoryService.IsCategoryExistForAdd(category);
            if (IsCategoryNameExists)
            {
                TempData["ErrorMessage"] = NotificationMessage.AlreadyExists.Replace("{0}", "Category");
                return RedirectToAction("Menu");
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (await _categoryService.AddCategory(category, userId))
            {
                TempData["SuccessMessage"] = NotificationMessage.EntityCreated.Replace("{0}", "Category");
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Category");
            return RedirectToAction("Menu");
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> EditCategoryById(Category category)
        {
            bool IsCategoryNameExists = _categoryService.IsCategoryExistForEdit(category);
            if (IsCategoryNameExists)
            {
                TempData["ErrorMessage"] = NotificationMessage.AlreadyExists.Replace("{0}", "Category");
                return RedirectToAction("Menu");
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            long Cat_Id = category.CategoryId;

            if (await _categoryService.EditCategory(category, Cat_Id, userId))
            {
                TempData["SuccessMessage"] = NotificationMessage.EntityUpdated.Replace("{0}", "Category");
                return RedirectToAction("Menu");
            }
            TempData["ErrorMessage"] = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "Category");
            return RedirectToAction("Menu");
        }

        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteCategory(long Cat_Id)
        {
            if (Cat_Id == null)
            {
                TempData["ErrorMessage"] = NotificationMessage.DoesNotExists.Replace("{0}", "Category");
                return RedirectToAction("Menu", "Menu");
            }

            if (await _categoryService.DeleteCategory(Cat_Id))
            {
                TempData["SuccessMessage"] = NotificationMessage.EntityDeleted.Replace("{0}", "Category");
                return RedirectToAction("Menu", "Menu");
            }
            TempData["ErrorMessage"] = NotificationMessage.EntityDeletedFailed.Replace("{0}", "Category");
            return RedirectToAction("Menu", "Menu");
        }
        #endregion

        #region Item CRUD

        // Get the Add item Modal 
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> GetItems()
        {
            MenuViewModel MenuVM = new MenuViewModel();
            MenuVM.categoryList = await _categoryService.GetAllCategories();
            MenuVM.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(await _modifierGroupService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");
            return PartialView("_AddItemPartial", MenuVM);
        }

        // Get the Modifiers by Group in Add Item
        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> GetModifiersByGroup(string data)
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
                    MenuVM.addItems.itemModifiersVM[i].modifiersList = await _modifierGroupService.GetModifiersByGroup(deItems.ModifierGrpId);
                    MenuVM.addItems.itemModifiersVM[i].ModifierGrpName = _modifierGroupService.GetModifiersGroupName(deItems.ModifierGrpId);
                    i++;
                }
            }

            MenuVM.categoryList = await _categoryService.GetAllCategories();
            MenuVM.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();

            ViewBag.categoryList = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(await _modifierGroupService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            return PartialView("_ModifierByGroup", MenuVM);
        }

        // Add Item Post
        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromForm] MenuViewModel MenuVm)
        {
            bool IsItemNameExists = _itemService.IsItemExistForAdd(MenuVm.addItems);
            if (IsItemNameExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Item") });
            }

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
                    string fileName = ImageTemplate.UploadImage(MenuVm.addItems.ItemFormImage, path);
                    MenuVm.addItems.ItemImage = $"/uploads/{fileName}";
                }
                else
                {
                    return Json(new { success = false, text = NotificationMessage.ImageFormat });
                }
            }

            bool addItemStatus = await _itemService.AddItem(MenuVm.addItems, userId);

            if (addItemStatus)
            {
                return Json(new { success = true, text = NotificationMessage.EntityCreated.Replace("{0}", "Item") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Item") });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> GetItemsByItemId(long itemid)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            MenuVM.categoryList = await _categoryService.GetAllCategories();
            MenuVM.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(await _modifierGroupService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");
            MenuVM.addItems = _itemService.GetItemsByItemId(itemid);
            return PartialView("_EditItemPartial", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> EditModifiersByGroup(string data)
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
                    MenuVM.addItems.itemModifiersVM[i].modifiersList = await _modifierGroupService.GetModifiersByGroup(deItems.ModifierGrpId);
                    MenuVM.addItems.itemModifiersVM[i].ModifierGrpName = _modifierGroupService.GetModifiersGroupName(deItems.ModifierGrpId);
                    i++;
                }
            }

            MenuVM.categoryList = await _categoryService.GetAllCategories();
            MenuVM.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.categoryList = new SelectList(await _categoryService.GetAllCategories(), "CategoryId", "CategoryName");
            ViewBag.modifierGroupList = new SelectList(await _modifierGroupService.GetAllModifierGroupList(), "ModifierGrpId", "ModifierGrpName");

            return PartialView("_EditModifierByGroup", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditItem([FromForm] MenuViewModel MenuVm)
        {
            bool IsItemNameExists = _itemService.IsItemExistForEdit(MenuVm.addItems);
            if (IsItemNameExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Item") });
            }

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
                    string fileName = ImageTemplate.UploadImage(MenuVm.addItems.ItemFormImage, path);
                    MenuVm.addItems.ItemImage = $"/uploads/{fileName}";
                }
                else
                {
                    return Json(new { success = false, text = NotificationMessage.ImageFormat });
                }
            }

            bool editItemStatus = await _itemService.EditItem(MenuVm.addItems, userId);

            if (editItemStatus)
            {
                return Json(new { success = true, text = NotificationMessage.EntityUpdated.Replace("{0}", "Item") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "Item") });
        }

        [PermissionAuthorize("Menu.Delete")]
        public async Task<IActionResult> DeleteItem(long itemid)
        {
            if (itemid == null)
            {
                TempData["ErrorMessage"] = NotificationMessage.DoesNotExists.Replace("{0}", "Item");
                return RedirectToAction("Menu", "Menu");
            }
            else
            {

                if (await _itemService.DeleteItem(itemid))
                {
                    TempData["SuccessMessage"] = NotificationMessage.EntityDeleted.Replace("{0}", "Item");
                    return RedirectToAction("Menu", "Menu");
                }
                TempData["ErrorMessage"] = NotificationMessage.EntityDeletedFailed.Replace("{0}", "Item");
                return RedirectToAction("Menu", "Menu");
            }

        }

        #endregion

        #region Pagination-Menu-Modifier
        [PermissionAuthorize("Menu.View")]
        public async Task<IActionResult> PaginationMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
        {
            try
            {
                MenuViewModel menuData = new MenuViewModel();
                menuData.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();

                if (modgrpid != null)
                {
                    menuData.PaginationForModifiersByModGroups = _modifierGroupService.GetMenuModifiersByModGroups(modgrpid, search, pageNumber, pageSize);
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
        [PermissionAuthorize("Menu.View")]
        public async Task<IActionResult> ExistingPaginationMenuModifiersByModGroups(string search = "", int pageNumber = 1, int pageSize = 3)
        {
            try
            {
                MenuViewModel menuData = new MenuViewModel();
                menuData.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();

                menuData.PaginationForModifiersByModGroups = _modifierGroupService.ExistingGetMenuModifiersByModGroups(search, pageNumber, pageSize);

                return PartialView("_ExistingModifierPartial", menuData.PaginationForModifiersByModGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

        #region Modifier Group CRUD

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModifierGroup(MenuViewModel MenuVM)
        {
            bool IsModifierGrpExists = _modifierGroupService.IsModifierGroupExistForAdd(MenuVM.addModifierGroupVM);

            if (IsModifierGrpExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Modifier Group") });
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool addModifierGroupStatus = await _modifierGroupService.AddModifierGroup(MenuVM.addModifierGroupVM, userId);
            if (addModifierGroupStatus)
            {
                return Json(new { success = true, text = NotificationMessage.EntityCreated.Replace("{0}", "Modifier Group") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Modifier Group") });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> GetModifierGroupByModifierGroupId(long modgrpid)
        {
            List<ModifiersViewModel>? modifiers = await _modifierGroupService.GetModifiersByModifierGroupId(modgrpid);
            Modifiergroup? modifierGroup = await _modifierGroupService.GetModifierGroupByModifierGroupId(modgrpid);
            return Json(new { modifiers, modifierGroup });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModToModifierGrpAfterEdit(long modgrpid, long modid)
        {
            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool addModToModifierGrpStatus = await _modifierGroupService.AddModToModifierGrpAfterEdit(modgrpid, modid, userId);
            if (addModToModifierGrpStatus)
            {
                return Json(new { });
            }
            return Json(new { });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> DeleteModToModifierGrpAfterEdit(long modid, long modgrpid)
        {
            bool deleteModToModifierGrpStatus = await _modifierGroupService.DeleteModToModifierGrpAfterEdit(modid, modgrpid);
            if (deleteModToModifierGrpStatus)
            {
                return Json(new { });
            }
            return Json(new { });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditModifierGroup(MenuViewModel MenuVM)
        {
            bool IsModifierGrpExists = _modifierGroupService.IsModifierGroupExistForEdit(MenuVM.addModifierGroupVM);

            if (IsModifierGrpExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Modifier Group") });
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            bool editModifierGroupStatus = await _modifierGroupService.EditModifierGroup(MenuVM.addModifierGroupVM, userId);
            if (editModifierGroupStatus)
            {
                return Json(new { grpId = MenuVM.addModifierGroupVM.ModifierGrpId, success = true, text = NotificationMessage.EntityUpdated.Replace("{0}", "Modifier Group") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "Modifier Group") });
        }

        [PermissionAuthorize("Menu.Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteModifierGroup(long modgrpid)
        {
            if (modgrpid == null)
            {
                return Json(new { success = false, text = NotificationMessage.DoesNotExists.Replace("{0}", "Modifier Group") });
            }

            if (await _modifierGroupService.DeleteModifierGroup(modgrpid))
            {
                return Json(new { success = true, text = NotificationMessage.EntityDeleted.Replace("{0}", "Modifier Group") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityDeletedFailed.Replace("{0}", "Modifier Group") });
        }

        #endregion

        #region Get All ModifierGroup List
        [PermissionAuthorize("Menu.View")]
        public async Task<IActionResult> GetAllModifierGroupList()
        {
            MenuViewModel MenuData = new();
            MenuData.modifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            return PartialView("_ModifierGroupPartial", MenuData);
        }
        #endregion

        #region Modifier CRUD

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> AddModifierItem()
        {
            MenuViewModel MenuVM = new MenuViewModel();
            List<Modifiergroup>? ModifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.modifierGroupList = new SelectList(ModifierGroupList, "ModifierGrpId", "ModifierGrpName");
            return PartialView("_AddModifierPartial", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> AddModifierItem([FromForm] MenuViewModel MenuVm)
        {
            bool IsModifierNameExists = _modifierItemService.IsModifierExistForAdd(MenuVm.addModifier);

            if (IsModifierNameExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Modifier") });
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (await _modifierItemService.AddModifierItem(MenuVm.addModifier, userId))
            {
                return Json(new { success = true, text = NotificationMessage.EntityCreated.Replace("{0}", "Modifier") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityCreatedFailed.Replace("{0}", "Modifier") });
        }

        [PermissionAuthorize("Menu.AddEdit")]
        public async Task<IActionResult> GetModifiersByModifierId(long modid)
        {
            MenuViewModel MenuVM = new MenuViewModel();
            List<Modifiergroup>? ModifierGroupList = await _modifierGroupService.GetAllModifierGroupList();
            ViewBag.modifierGroupList = new SelectList(ModifierGroupList, "ModifierGrpId", "ModifierGrpName");
            MenuVM.addModifier = _modifierItemService.GetModifiersByModifierId(modid);
            return PartialView("_EditModifierPartial", MenuVM);
        }

        [PermissionAuthorize("Menu.AddEdit")]
        [HttpPost]
        public async Task<IActionResult> EditModifierItem([FromForm] MenuViewModel MenuVm)
        {
            bool IsModifierNameExists = _modifierItemService.IsModifierExistForEdit(MenuVm.addModifier);
            if (IsModifierNameExists)
            {
                return Json(new { success = false, text = NotificationMessage.AlreadyExists.Replace("{0}", "Modifier") });
            }

            string token = Request.Cookies["AuthToken"];
            List<User>? userData = _userService.getUserFromEmail(token);
            long userId = _userLoginService.GetUserId(userData[0].Userlogin.Email);

            if (await _modifierItemService.EditModifierItem(MenuVm.addModifier, userId))
            {
                return Json(new { success = true, text = NotificationMessage.EntityUpdated.Replace("{0}", "Modifier") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityUpdatedFailed.Replace("{0}", "Modifier") });
        }

        [PermissionAuthorize("Menu.Delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteModifier(long modid)
        {
            if (modid == null)
            {
                return Json(new { success = false, text = NotificationMessage.DoesNotExists.Replace("{0}", "Modifier") });
            }

            if (await _modifierItemService.DeleteModifier(modid))
            {
                return Json(new { success = true, text = NotificationMessage.EntityDeleted.Replace("{0}", "Modifier") });
            }
            return Json(new { success = false, text = NotificationMessage.EntityDeletedFailed.Replace("{0}", "Modifier") });
        }

        #endregion

    }
}