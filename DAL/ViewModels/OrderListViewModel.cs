namespace DAL.ViewModels;

public class OrderListViewModel
{
    public long OrderId { get; set; }
    public DateOnly OrderDate { get; set; }
    public string Paymentstatus { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string? OrderType { get; set; }
    public int NoOfItems { get; set; }

}