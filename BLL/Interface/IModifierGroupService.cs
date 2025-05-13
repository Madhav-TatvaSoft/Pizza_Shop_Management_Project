using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IModifierGroupService
{
    Task<List<Modifiergroup>> GetAllModifierGroupList();
    PaginationViewModel<ModifiersViewModel> GetMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3);
    PaginationViewModel<ModifiersViewModel> ExistingGetMenuModifiersByModGroups(string search = "", int pageNumber = 1, int pageSize = 5);
    Task<List<Modifier>> GetModifiersByGroup(long modgrpid);
    string GetModifiersGroupName(long modgrpid);
    Task<bool> AddModifierGroup(AddModifierGroupViewModel addModifierGroupVM, long userId);
    Task<bool> EditModifierGroup(AddModifierGroupViewModel editModifierGroupVM, long userId);
    Task<Modifiergroup> GetModifierGroupByModifierGroupId(long modgrpid);
    Task<List<ModifiersViewModel>> GetModifiersByModifierGroupId(long modgrpid);
    Task<bool> AddModToModifierGrpAfterEdit(long modgrpid, long modid, long userId);
    Task<bool> DeleteModToModifierGrpAfterEdit(long modid, long modgrpid);
    Task<bool> DeleteModifierGroup(long modgrpid);
    bool IsModifierGroupExist(AddModifierGroupViewModel modifierGrpVM);

}