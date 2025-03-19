using DAL.Models;

namespace DAL.ViewModels;

public class TableSectionViewModel
{
    public List<Section> SectionList { get; set; }

    public Section section { get; set; }

    public SectionViewModel sectionVM { get; set; }

    public TablesViewModel tablesVM { get; set; }

    public PaginationViewModel<TablesViewModel> PaginationForTable { get; set; }

}