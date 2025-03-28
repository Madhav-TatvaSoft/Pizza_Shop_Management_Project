namespace DAL.ViewModels;

public class CustomerViewModel
{
    public string CustomerName { get; set; } = null!;

    public long? PhoneNo { get; set; }

    public string? Email { get; set; }

    public int totalOrder { get; set; }

}
