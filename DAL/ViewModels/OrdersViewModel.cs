namespace DAL.ViewModels;

public class OrdersViewModel
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public string CustomerName { get; set; }

    public DateOnly OrderDate { get; set; }

    public string Status { get; set; }

    public long? RatingId { get; set; }

    public int rating { get; set; }

    public decimal TotalAmount { get; set; }

    public long PaymentmethodId { get; set; }

    public string PaymentMethodName { get; set; }

    // public long PaymentStatusId { get; set; }

    // public long SectionId { get; set; }

    public bool Isdelete { get; set; }

    // public long TableId { get; set; }
}