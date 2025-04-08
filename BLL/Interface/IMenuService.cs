
// using DAL.Models;
// using DAL.ViewModels;

// namespace BLL.Interface;

// public interface IMenuService
// {

    // public List<Category> GetAllCategories();
    // public List<Modifiergroup> GetAllModifierGroupList();

    // Pagination for Items, Modifiers, and  (Add,Edit) Existing Modifiers 
    // public PaginationViewModel<ItemsViewModel> GetMenuItemsByCategory(long? catid, string search = "", int pageNumber = 1, int pageSize = 3);
    // public PaginationViewModel<ModifiersViewModel> GetMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3);
    // public PaginationViewModel<ModifiersViewModel> ExistingGetMenuModifiersByModGroups(string search = "", int pageNumber = 1, int pageSize = 5);

    // CRUD Operations Category

    // public Task<bool> AddCategory(Category category, long userId);
    // public Task<bool> EditCategoryById(Category category, long Cat_Id, long userId);
    // public Task<bool> DeleteCategory(long Cat_Id);

    // CRUD Operations Items

    // public Task<bool> AddItem(AddItemViewModel addItemVM, long userId);
    // public AddItemViewModel GetItemsByItemId(long itemid);
    // public Task<bool> EditItem(AddItemViewModel editItemVM, long userId);
    // public Task<bool> DeleteItem(long itemid);

    // CRUD Operations Modifiers Group
    // public Task<bool> AddModifierGroup(AddModifierGroupViewModel addModifierGroupVM, long userId);

    // public Modifiergroup GetModifierGroupByModifierGroupId(long modgrpid);
    // public List<ModifiersViewModel> GetModifiersByModifierGroupId(long modgrpid);
    // public Task<bool> AddModToModifierGrpAfterEdit(long modgrpid, long modid, long userId);
    // public Task<bool> DeleteModToModifierGrpAfterEdit(long modid,long modgrpid);
    // public Task<bool> EditModifierGroup(AddModifierGroupViewModel editModifierGroupVM, long userId);

    // public Task<bool> DeleteModifierGroup(long modgrpid);

    // CRUD Operations Modifiers
    // public Task<bool> AddModifierItem(AddModifierViewModel addModifierVM, long userId);
    // public AddModifierViewModel GetModifiersByModifierId(long modid);
    // public Task<bool> EditModifierItem(AddModifierViewModel editModifierVM, long userId);
    // public Task<bool> DeleteModifier(long modid);

    // public List<Modifier> GetModifiersByGroup(long modgrpid);

    // public string GetModifiersGroupName(long modgrpid);

// }