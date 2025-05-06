namespace DAL.ViewModels;

public class RatingsViewModel
{
    public long RatingId { get; set; }

    public long CustomerId { get; set; }

    public int? Food { get; set; }

    public int? Service { get; set; }

    public int? Ambience { get; set; }

    public string? Review { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

}