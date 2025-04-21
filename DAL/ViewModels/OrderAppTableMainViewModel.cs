using DAL.Models;

namespace DAL.ViewModels;

public class OrderAppTableMainViewModel
{
    public List<OrderAppSectionVM> sectionListVM { get; set; } = null!;
    public List<OrderAppTableVM> tableListVM { get; set; } = null!;
    public WaitingTokenDetailViewModel waitingTokenDetailViewModel { get; set; } = null!;
    public List<WaitingTokenDetailViewModel> WaitingTokenVMList { get; set; } = null!;
    public long SectionId { get; set; }
    public string SectionName { get; set; } = null!;
    public string TableIds { get; set; } = null!;

}
