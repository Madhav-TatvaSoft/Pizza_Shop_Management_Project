using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class TableSectionService : ITableSectionService
{
    private readonly PizzaShopDbContext _context;

    public TableSectionService(PizzaShopDbContext context)
    {
        _context = context;
    }
    public List<Section> GetAllSections()
    {
        return _context.Sections.Where(x => !x.Isdelete).OrderBy(x => x.SectionId).ToList();
    }
    public PaginationViewModel<TablesViewModel> GetTablesBySection(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        IQueryable<TablesViewModel>? query = _context.Tables
           .Include(x => x.Section)
           .Where(x => x.SectionId == sectionid && x.Isdelete == false).OrderBy(x => x.TableId)
           .Select(x => new TablesViewModel
           {
               TableId = x.TableId,
               SectionId = x.SectionId,
               TableName = x.TableName,
               Capacity = x.Capacity,
               Status = x.Status,
               Isdelete = x.Isdelete
           })
           .AsQueryable();


        //search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(x =>
                x.TableName.ToLower().Contains(lowerSearchTerm) ||
                x.Capacity.ToString().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        // Apply pagination
        List<TablesViewModel>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<TablesViewModel>(items, totalCount, pageNumber, pageSize);
    }

    #region Section CRUD
    public async Task<bool> AddSection(SectionViewModel addsection, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Section? sections = await _context.Sections.FirstOrDefaultAsync(x => x.SectionName.ToLower().Trim() == addsection.SectionName.ToLower().Trim() && !x.Isdelete);
                if (sections != null)
                {
                    return false;
                }
                else
                {
                    Section section = new Section
                    {
                        SectionName = addsection.SectionName,
                        Description = addsection.Description,
                        Isdelete = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    await _context.Sections.AddAsync(section);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

    public SectionViewModel GetSectionById(long sectionid)
    {
        Section? section = _context.Sections.FirstOrDefault(x => x.SectionId == sectionid && x.Isdelete == false);
        if (section != null)
        {
            SectionViewModel sectionVM = new SectionViewModel
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                Description = section.Description,
                Isdelete = section.Isdelete
            };
            return sectionVM;
        }
        return null;
    }

    public async Task<bool> EditSection(SectionViewModel editSection, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (editSection.SectionId == null)
                {
                    return false;
                }
                else
                {
                    Section? isSectionNameExist = await _context.Sections.FirstOrDefaultAsync(x => x.SectionId != editSection.SectionId && x.SectionName.ToLower().Trim() == editSection.SectionName.ToLower().Trim() && x.Isdelete == false);
                    if (isSectionNameExist != null)
                    {
                        return false;
                    }
                    Section? isSectionExist = await _context.Sections.FirstOrDefaultAsync(x => x.SectionId == editSection.SectionId && x.Isdelete == false);
                    if (isSectionExist != null)
                    {
                        isSectionExist.SectionName = editSection.SectionName;
                        isSectionExist.Description = editSection.Description;
                        isSectionExist.ModifiedAt = DateTime.Now;
                        isSectionExist.ModifiedBy = userId;

                        _context.Sections.Update(isSectionExist);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
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

    public async Task<bool> DeleteSection(long sectionid)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Section? sectionToDelete = await _context.Sections.FirstOrDefaultAsync(x => x.SectionId == sectionid && x.Isdelete == false);

                List<Table> existingTables = await _context.Tables.Where(x => x.SectionId == sectionid && x.Isdelete == false).ToListAsync();

                if (existingTables.Count > 0)
                {
                    foreach (var table in existingTables)
                    {
                        table.Isdelete = true;
                        _context.Tables.Update(table);
                        await _context.SaveChangesAsync();
                    }
                }

                // for (int i = 0; i < existingTables.Count; i++)
                // {
                //     existingTables[i].Isdelete = true;
                //     _context.Update(existingTables[i]);
                //     await _context.SaveChangesAsync();
                // }

                if (sectionToDelete != null)
                {
                    sectionToDelete.SectionName = sectionToDelete.SectionName + DateTime.Now;
                    sectionToDelete.Isdelete = true;
                    // sectionToDelete.ModifiedAt = DateTime.Now;
                    // sectionToDelete.ModifiedBy = userId;

                    _context.Sections.Update(sectionToDelete);
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

    public async Task<bool> IsTableOccupiedinSection(long sectionid)
    {
        return _context.Tables.Any(x => x.SectionId == sectionid && !x.Isdelete && (x.Status != "Available"));
    }

    #endregion

    #region Table CRUD

    public async Task<bool> AddTable(TablesViewModel tableVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                if (tableVM.SectionId == null)
                {
                    return false;
                }

                Table? isTableExist = await _context.Tables.FirstOrDefaultAsync(x => x.TableName.ToLower().Trim() == tableVM.TableName.ToLower().Trim() && x.SectionId == tableVM.SectionId && x.Isdelete == false);

                if (isTableExist != null)
                {
                    return false;
                }

                Table table = new Table
                {
                    SectionId = tableVM.SectionId,
                    TableName = tableVM.TableName,
                    Capacity = tableVM.Capacity,
                    Status = tableVM.Status,
                    Isdelete = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                await _context.Tables.AddAsync(table);
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

    public TablesViewModel GetTableById(long tableId, long sectionId)
    {
        Table? table = _context.Tables.FirstOrDefault(x => x.TableId == tableId && x.SectionId == sectionId && x.Isdelete == false);
        if (table != null)
        {
            TablesViewModel tableVM = new TablesViewModel
            {
                TableId = table.TableId,
                SectionId = table.SectionId,
                TableName = table.TableName,
                Capacity = table.Capacity,
                Status = table.Status,
                Isdelete = table.Isdelete
            };
            return tableVM;
        }
        return null;
    }

    public async Task<bool> EditTable(TablesViewModel tableVM, long userId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {

                var isTableNameExist = _context.Tables.FirstOrDefault(x => x.TableId != tableVM.TableId && x.TableName.ToLower().Trim() == tableVM.TableName.ToLower().Trim() && x.Isdelete == false);

                if (isTableNameExist != null)
                {
                    return false;
                }

                var table = _context.Tables.FirstOrDefault(x => x.TableId == tableVM.TableId && x.Isdelete == false);

                if (table != null)
                {
                    table.SectionId = tableVM.SectionId;
                    table.TableName = tableVM.TableName;
                    table.Capacity = tableVM.Capacity;
                    table.Status = tableVM.Status;
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = userId;

                    _context.Tables.Update(table);
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

    public async Task<bool> DeleteTable(long tableId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Table? table = _context.Tables.FirstOrDefault(x => x.TableId == tableId && x.Isdelete == false);
                if (table != null)
                {
                    table.TableName = table.TableName + DateTime.Now;
                    table.Isdelete = true;
                    _context.Tables.Update(table);
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

    public async Task<bool> IsTableOccupied(long tableId)
    {
        return _context.Tables.Any(x => x.TableId == tableId && !x.Isdelete && (x.Status != "Available"));
    }

    #endregion

}