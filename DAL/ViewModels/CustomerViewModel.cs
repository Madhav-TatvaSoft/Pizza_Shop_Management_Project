namespace DAL.ViewModels;

public class CustomerViewModel
{
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;

    public long? PhoneNo { get; set; }

    public string? Email { get; set; }

    public DateOnly CreatedAt { get; set; }

    public int totalOrder { get; set; }
    public int NoOfPerson { get; set; }

}