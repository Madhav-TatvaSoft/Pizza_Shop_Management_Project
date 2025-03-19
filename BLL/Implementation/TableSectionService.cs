using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class TableSectionService : ITableSectionService
{
    private readonly PizzaShopDbContext _context;

    #region Table Section Constructor
    public TableSectionService(PizzaShopDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Get All Sections
    public List<Section> GetAllSections()
    {
        return _context.Sections.Where(x => x.Isdelete == false).ToList();
    }
    #endregion

    #region Pagination Model for Items
    public PaginationViewModel<TablesViewModel> GetTablesBySection(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3)
    {
        var query = _context.Tables
           .Include(x => x.Section)
           .Where(x => x.SectionId == sectionid && x.Isdelete == false)
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
        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<TablesViewModel>(items, totalCount, pageNumber, pageSize);
    }
    #endregion

    #region Add Section
    public async Task<bool> AddSection(SectionViewModel addsection, long userId)
    {
        var isSectionExist = await _context.Sections.FirstOrDefaultAsync(x => x.SectionName == addsection.SectionName && x.Isdelete == false && x.Description == addsection.Description);
        if (isSectionExist != null)
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
            return true;
        }
    }
    #endregion

    #region Get Section By Id
    public Section GetSectionById(long sectionid)
    {
        return _context.Sections.FirstOrDefault(x => x.SectionId == sectionid && x.Isdelete == false);
    }
    #endregion

    #region Edit Section
    public async Task<bool> EditSection(SectionViewModel editSection, long userId)
    {
        if (editSection.SectionId == null)
        {
            return false;
        }
        else
        {
            var isSectionExist = await _context.Sections.FirstOrDefaultAsync(x => x.SectionId == editSection.SectionId && x.Isdelete == false);
            if (isSectionExist != null)
            {
                isSectionExist.SectionName = editSection.SectionName;
                isSectionExist.Description = editSection.Description;
                isSectionExist.ModifiedAt = DateTime.Now;
                isSectionExist.ModifiedBy = userId;

                _context.Sections.Update(isSectionExist);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Delete Section
    public async Task<bool> DeleteSection(long sectionId)
    {
        Section sectionToDelete = await _context.Sections.FirstOrDefaultAsync(x => x.SectionId == sectionId && x.Isdelete == false);

        List<Table> existingTables = await _context.Tables.Where(x => x.SectionId == sectionId && x.Isdelete == false).ToListAsync();

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
            return true;
        }
        return false;
    }
    #endregion

    #region Add Table
    public async Task<bool> AddTable(TablesViewModel tableVM, long userId)
    {
        if (tableVM.SectionId == null)
        {
            return false;
        }

        var isTableExist = await _context.Tables.FirstOrDefaultAsync(x => x.TableName == tableVM.TableName && x.SectionId == tableVM.SectionId && x.Isdelete == false);

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
        return true;

    }
    #endregion

    #region Get Table By Id
    public TablesViewModel GetTableById(long tableId)
    {
        var table = _context.Tables.FirstOrDefault(x => x.TableId == tableId && x.Isdelete == false);
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
    #endregion

    #region Edit Table
    public async Task<bool> EditTable(TablesViewModel tableVM, long userId)
    {
        // var isTableExist = _context.Tables.FirstOrDefault(x => x.TableId == tableVM.TableId && x.Isdelete == false);
        // if (isTableExist != null)
        // {
        //     isTableExist.TableName = tableVM.TableName;
        //     isTableExist.Capacity = tableVM.Capacity;
        //     isTableExist.Status = tableVM.Status;

        //     _context.Tables.Update(isTableExist);
        //     _context.SaveChanges();
        //     return true;
        // }
        // return false;
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
            return true;
        }
        return false;
    }
    #endregion

    #region Delete Table
    public async Task<bool> DeleteTable(long tableId)
    {
        var table = _context.Tables.FirstOrDefault(x => x.TableId == tableId && x.Isdelete == false);
        if (table != null)
        {
            table.TableName = table.TableName + DateTime.Now;
            table.Isdelete = true;
            _context.Tables.Update(table);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    #endregion

}