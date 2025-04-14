namespace DAL.ViewModels;

public class OrderAppTableVM
{
    public long TableId { get; set; }
    public long SectionId { get; set; }
    public string TableName { get; set; } = null!;
    public int Capacity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime TableTime {get; set;}
    public decimal OrderAmount{get; set;}
}
