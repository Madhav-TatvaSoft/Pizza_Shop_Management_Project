using DAL.ViewModels;

namespace BLL.Interface;

public interface IModifierItemService
{
    Task<bool> AddModifierItem(AddModifierViewModel addModifierVM, long userId);
    AddModifierViewModel GetModifiersByModifierId(long modid);
    Task<bool> EditModifierItem(AddModifierViewModel editModifierVM, long userId);
    Task<bool> DeleteModifier(long modid);
    bool IsModifierExistForAdd(AddModifierViewModel ModifierVM);
    bool IsModifierExistForEdit(AddModifierViewModel ModifierVM);

}