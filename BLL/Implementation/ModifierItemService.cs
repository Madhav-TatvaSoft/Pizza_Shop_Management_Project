using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class ModifierItemService : IModifierItemService
{
    private readonly PizzaShopDbContext _context;

    #region Constructor
    public ModifierItemService(PizzaShopDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Add Modifier
    public async Task<bool> AddModifierItem(AddModifierViewModel addModifierVM, long userId)
    {
        if (addModifierVM == null)
        {
            return false;
        }

        Modifier modifier = new Modifier();
        modifier.ModifierGrpId = addModifierVM.ModifierGrpId;
        modifier.ModifierName = addModifierVM.ModifierName;
        modifier.Rate = addModifierVM.Rate;
        modifier.Quantity = addModifierVM.Quantity;
        modifier.Unit = addModifierVM.Unit;
        modifier.Description = addModifierVM.Description;
        modifier.CreatedBy = userId;

        await _context.Modifiers.AddAsync(modifier);
        await _context.SaveChangesAsync();
        return true;

    }
    #endregion

    #region Get Modifiers By ModifierId
    public AddModifierViewModel GetModifiersByModifierId(long modid)
    {
        Modifier? modifier = _context.Modifiers.FirstOrDefault(x => x.ModifierId == modid && x.Isdelete == false);
        AddModifierViewModel addModifierVM = new();
        {
            addModifierVM.ModifierGrpId = modifier.ModifierGrpId;
            addModifierVM.ModifierId = modifier.ModifierId;
            addModifierVM.ModifierName = modifier.ModifierName;
            addModifierVM.Description = modifier.Description;
            addModifierVM.Quantity = (int)modifier.Quantity;
            addModifierVM.Rate = modifier.Rate;
            addModifierVM.Unit = modifier.Unit;
        }
        return addModifierVM;
    }
    #endregion

    #region Edit Modifier
    public async Task<bool> EditModifierItem(AddModifierViewModel editModifierVM, long userId)
    {
        if (editModifierVM.ModifierGrpId == null)
        {
            return false;
        }
        else
        {
            // var existingModifier = await _context.Modifiers.FirstOrDefaultAsync(x => x.ModifierName == editModifierVM.ModifierName && x.Isdelete == false);
            // if (existingModifier != null)
            // {
            //     return false;
            // }
            Modifier? modifier = _context.Modifiers.FirstOrDefault(x => x.ModifierId == editModifierVM.ModifierId && x.Isdelete == false);
            modifier.ModifierGrpId = editModifierVM.ModifierGrpId;
            modifier.ModifierName = editModifierVM.ModifierName;
            modifier.Rate = editModifierVM.Rate;
            modifier.Quantity = editModifierVM.Quantity;
            modifier.Unit = editModifierVM.Unit;
            modifier.Description = editModifierVM.Description;
            modifier.ModifiedAt = DateTime.Now;
            modifier.ModifiedBy = userId;

            _context.Modifiers.Update(modifier);
            await _context.SaveChangesAsync();
            return true;
        }
    }
    #endregion

    #region Delete Modifier
    public async Task<bool> DeleteModifier(long modid)
    {
        Modifier? modofierToDelete = _context.Modifiers.FirstOrDefault(x => x.ModifierId == modid && x.Isdelete == false);
        if (modofierToDelete != null)
        {
            modofierToDelete.ModifierName = modofierToDelete.ModifierName + DateTime.Now;
            modofierToDelete.Isdelete = true;
            _context.Modifiers.Update(modofierToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    #endregion

    #region Check If Exists
    public bool IsModifierExistForAdd(AddModifierViewModel ModifierVM)
    {
        return _context.Modifiers.Any(x => x.ModifierName.ToLower().Trim() == ModifierVM.ModifierName.ToLower().Trim() && x.ModifierGrpId == ModifierVM.ModifierGrpId && !x.Isdelete);
    }
    public bool IsModifierExistForEdit(AddModifierViewModel ModifierVM)
    {
        return _context.Modifiers.Any(x => x.ModifierId != ModifierVM.ModifierId && x.ModifierName.ToLower().Trim() == ModifierVM.ModifierName.ToLower().Trim() && x.ModifierGrpId == ModifierVM.ModifierGrpId && !x.Isdelete);
    }

    #endregion
}