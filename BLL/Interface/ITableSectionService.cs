using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface ITableSectionService
{
    List<Section> GetAllSections();
    PaginationViewModel<TablesViewModel> GetTablesBySection(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3);

    Task<bool> AddSection(SectionViewModel addsection, long userId);
    SectionViewModel GetSectionById(long sectionid);
    Task<bool> EditSection(SectionViewModel editSection, long userId);

    Task<bool> DeleteSection(long sectionid);

    Task<bool> AddTable(TablesViewModel tableVM, long userId);

    TablesViewModel GetTableById(long tableId, long sectionId);

    Task<bool> EditTable(TablesViewModel tableVM, long userId);

    Task<bool> DeleteTable(long tableId);
    Task<bool> IsTableOccupied(long tableId);
    Task<bool> IsTableOccupiedinSection(long sectionid);


}