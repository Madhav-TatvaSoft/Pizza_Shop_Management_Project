namespace DAL.ViewModels;

public class CustomerHistoryViewModel
{
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public long PhoneNo { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal MaxOrder { get; set; }
    public decimal AvgBill { get; set; }
    public int visits { get; set; }
    public List<OrderListViewModel>? orderList { get; set; }
}