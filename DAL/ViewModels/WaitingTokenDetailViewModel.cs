namespace DAL.ViewModels;

public class WaitingTokenDetailViewModel
{
    public long WaitingId { get; set; }

    public long CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public long? PhoneNo { get; set; }

    public string? Email { get; set; }

    public int NoOfPerson { get; set; }

    public long SectionId { get; set; }

    public string SectionName {get; set;}
}
