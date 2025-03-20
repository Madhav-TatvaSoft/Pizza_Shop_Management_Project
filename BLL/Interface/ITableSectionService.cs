using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface ITableSectionService
{
    public List<Section> GetAllSections();
    public PaginationViewModel<TablesViewModel> GetTablesBySection(long? sectionid, string search = "", int pageNumber = 1, int pageSize = 3);

    public Task<bool> AddSection(SectionViewModel addsection, long userId);
    public SectionViewModel GetSectionById(long sectionid);
    public Task<bool> EditSection(SectionViewModel editSection, long userId);

    public Task<bool> DeleteSection(long sectionid);

    public Task<bool> AddTable(TablesViewModel tableVM, long userId);

    public TablesViewModel GetTableById(long tableId,long sectionId);

    public Task<bool> EditTable(TablesViewModel tableVM, long userId);

    public Task<bool> DeleteTable(long tableId);

}