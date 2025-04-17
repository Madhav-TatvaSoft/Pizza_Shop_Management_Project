namespace DAL.ViewModels;

public class OrderAppWaitingViewModel
{
    public List<OrderAppSectionVM> sectionVMList { get; set; } = null!;
    public List<WaitingTokenDetailViewModel> WaitingTokenVMList { get; set; } = null!;
}
