using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class ModifierGroupService : IModifierGroupService
{
    private readonly PizzaShopDbContext _context;

    public ModifierGroupService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public PaginationViewModel<ModifiersViewModel> GetMenuModifiersByModGroups(long? modgrpid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        IQueryable<ModifiersViewModel>? query = _context.Modifiers.Include(x => x.ModifierGrp).Where(x => x.ModifierGrpId == modgrpid && !x.Isdelete).OrderBy(x => x.ModifierId)
           .Select(x => new ModifiersViewModel
           {
               ModifierId = x.ModifierId,
               ModifierName = x.ModifierName,
               ModifierGrpId = x.ModifierGrpId,
               Unit = x.Unit,
               Rate = x.Rate,
               Quantity = x.Quantity,
               Isdelete = x.Isdelete
           })
           .AsQueryable();

        //search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(x => x.ModifierName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        List<ModifiersViewModel>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<ModifiersViewModel>(items, totalCount, pageNumber, pageSize);
    }

    public PaginationViewModel<ModifiersViewModel> ExistingGetMenuModifiersByModGroups(string search = "", int pageNumber = 1, int pageSize = 5)
    {
        IQueryable<ModifiersViewModel>? query = _context.Modifiers.Where(x => !x.Isdelete)
           .Select(x => new ModifiersViewModel
           {
               ModifierId = x.ModifierId,
               ModifierName = x.ModifierName,
               ModifierGrpId = x.ModifierGrpId,
               Unit = x.Unit,
               Rate = x.Rate,
               Quantity = x.Quantity,
               Isdelete = x.Isdelete
           })
           .AsQueryable();

        //search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(x => x.ModifierName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        List<ModifiersViewModel> items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<ModifiersViewModel>(items, totalCount, pageNumber, pageSize);
    }

    #region Get 
    public async Task<List<Modifiergroup>> GetAllModifierGroupList()
    {
        return await _context.Modifiergroups.Where(x => !x.Isdelete).OrderBy(x => x.ModifierGrpId).ToListAsync();
    }

    public async Task<List<Modifier>> GetModifiersByGroup(long modgrpid)
    {
        return await _context.Modifiers.Where(x => x.ModifierGrpId == modgrpid && !x.Isdelete).ToListAsync();
    }

    public string GetModifiersGroupName(long modgrpid)
    {
        return _context.Modifiergroups.SingleOrDefault(x => x.ModifierGrpId == modgrpid && !x.Isdelete).ModifierGrpName;
    }

    #endregion

    #region Modifier Group CRUD
    public async Task<bool> AddModifierGroup(AddModifierGroupViewModel addModifierGroupVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (addModifierGroupVM == null)
                {
                    return false;
                }

                Modifiergroup modifiergroup = new();
                // modifiergroup.ModifierGrpId = addModifierGroupVM.ModifierGrpId;
                modifiergroup.ModifierGrpName = addModifierGroupVM.ModifierGrpName;
                modifiergroup.Desciption = addModifierGroupVM.Desciption;
                modifiergroup.CreatedBy = userId;

                await _context.AddAsync(modifiergroup);
                await _context.SaveChangesAsync();

                if (addModifierGroupVM.Temp_Ids != null)
                {
                    string[] modifierTempId = addModifierGroupVM.Temp_Ids.Split(",");

                    Modifiergroup? addedModifiergroup = await _context.Modifiergroups.FirstOrDefaultAsync(x => x.ModifierGrpName == addModifierGroupVM.ModifierGrpName && !x.Isdelete);


                    for (int i = 0; i < modifierTempId.Length; i++)
                    {

                        Modifier? modifierExist = await _context.Modifiers.FirstOrDefaultAsync(x => x.ModifierId == int.Parse(modifierTempId[i]) && !x.Isdelete);

                        Modifier modifier = new();

                        modifier.ModifierGrpId = addedModifiergroup.ModifierGrpId;
                        modifier.ModifierName = modifierExist.ModifierName;
                        modifier.Unit = modifierExist.Unit;
                        modifier.Rate = modifierExist.Rate;
                        modifier.Quantity = modifierExist.Quantity;
                        modifier.Description = modifierExist.Description;
                        modifier.CreatedBy = userId;

                        await _context.AddAsync(modifier);
                        await _context.SaveChangesAsync();
                    }
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<Modifiergroup> GetModifierGroupByModifierGroupId(long modgrpid)
    {
        var modifierGroup = await _context.Modifiergroups.FirstOrDefaultAsync(x => x.ModifierGrpId == modgrpid && !x.Isdelete);

        if (modifierGroup == null)
        {
            return null;
        }

        return modifierGroup;
    }

    public async Task<List<ModifiersViewModel>> GetModifiersByModifierGroupId(long modgrpid)
    {
        List<ModifiersViewModel>? modifiers = await _context.Modifiers.Where(x => x.ModifierGrpId == modgrpid && !x.Isdelete)
        .Select(x => new ModifiersViewModel
        {
            ModifierGrpId = x.ModifierGrpId,
            ModifierId = x.ModifierId,
            ModifierName = x.ModifierName,
            Rate = x.Rate,
            Quantity = x.Quantity,
            Unit = x.Unit,
            Isdelete = x.Isdelete
        }).ToListAsync();
        return modifiers;
    }

    public async Task<bool> AddModToModifierGrpAfterEdit(long modgrpid, long modid, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Modifier? existingModifier = await _context.Modifiers.FirstOrDefaultAsync(x => x.ModifierId == modid && !x.Isdelete);

                if (existingModifier != null)
                {
                    Modifier modifier = new Modifier();
                    modifier.ModifierGrpId = modgrpid;
                    modifier.ModifierName = existingModifier.ModifierName;
                    modifier.Rate = existingModifier.Rate;
                    modifier.Quantity = existingModifier.Quantity;
                    modifier.Unit = existingModifier.Unit;
                    modifier.Description = existingModifier.Description;
                    modifier.CreatedBy = userId;

                    await _context.Modifiers.AddAsync(modifier);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<bool> DeleteModToModifierGrpAfterEdit(long modid, long modgrpid)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Modifier? existingModifier = await _context.Modifiers.FirstOrDefaultAsync(x => x.ModifierId == modid && x.ModifierGrpId == modgrpid && !x.Isdelete);

                if (existingModifier == null)
                {
                    return false;
                }

                existingModifier.Isdelete = true;
                existingModifier.ModifiedAt = DateTime.Now;
                _context.Update(existingModifier);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<bool> EditModifierGroup(AddModifierGroupViewModel editModifierGroupVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Modifiergroup? ModifierGroup = await _context.Modifiergroups.FirstOrDefaultAsync(x => x.ModifierGrpId == editModifierGroupVM.ModifierGrpId && !x.Isdelete);

                if (ModifierGroup == null)
                {
                    return false;
                }
                ModifierGroup.ModifierGrpId = editModifierGroupVM.ModifierGrpId;
                ModifierGroup.ModifierGrpName = editModifierGroupVM.ModifierGrpName;
                ModifierGroup.Desciption = editModifierGroupVM.Desciption;
                ModifierGroup.ModifiedAt = DateTime.Now;
                ModifierGroup.ModifiedBy = userId;

                _context.Modifiergroups.Update(ModifierGroup);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public async Task<bool> DeleteModifierGroup(long modgrpid)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                List<Modifier> existingModifiers = await _context.Modifiers.Where(x => x.ModifierGrpId == modgrpid && !x.Isdelete).ToListAsync();

                List<ItemModifierGroupMapping> existingItemModifierGroupMappings = await _context.ItemModifierGroupMappings.Where(x => x.ModifierGrpId == modgrpid && !x.Isdelete).ToListAsync();

                for (int i = 0; i < existingItemModifierGroupMappings.Count; i++)
                {
                    existingItemModifierGroupMappings[i].Isdelete = true;
                    _context.Update(existingItemModifierGroupMappings[i]);
                    _context.SaveChanges();
                }

                for (int i = 0; i < existingModifiers.Count; i++)
                {
                    existingModifiers[i].Isdelete = true;
                    existingModifiers[i].ModifiedAt = DateTime.Now;
                    _context.Update(existingModifiers[i]);
                    _context.SaveChanges();
                }

                Modifiergroup modifierGroupToDelete = await _context.Modifiergroups.FirstOrDefaultAsync(x => x.ModifierGrpId == modgrpid && !x.Isdelete);

                if (modifierGroupToDelete == null)
                {
                    return false;
                }

                modifierGroupToDelete.Isdelete = true;
                modifierGroupToDelete.ModifiedAt = DateTime.Now;
                _context.Update(modifierGroupToDelete);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    #endregion

    #region Check Modifier Group Exist

    public bool IsModifierGroupExist(AddModifierGroupViewModel modifierGrpVM)
    {
        if (modifierGrpVM.ModifierGrpId == 0)
        {
            return _context.Modifiergroups.Any(x => x.ModifierGrpName.ToLower().Trim() == modifierGrpVM.ModifierGrpName.ToLower().Trim() && !x.Isdelete);
        }
        else
        {
            return _context.Modifiergroups.Any(x => x.ModifierGrpId != modifierGrpVM.ModifierGrpId && x.ModifierGrpName.ToLower().Trim() == modifierGrpVM.ModifierGrpName.ToLower().Trim() && !x.Isdelete);
        }
    }
    
    #endregion

}