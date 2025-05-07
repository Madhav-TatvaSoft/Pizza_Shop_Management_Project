namespace DAL.ViewModels;

public class DashboardViewModel
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AvgOrderValue { get; set; }
    public double AvgWaitingTime { get; set; }
    public List<SellingItemViewModel> TopSellingItems { get; set; }
    public List<SellingItemViewModel> LeastSellingItems { get; set; }
    public int WaitingListCount { get; set; }
    public int NewCustomerCount { get; set; }

    // Something for graph
}
