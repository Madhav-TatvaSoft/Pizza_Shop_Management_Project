using DAL.Models;

namespace DAL.ViewModels;

public class OrderAppKOTViewModel
{
    public List<Category> categoryList { get; set; }
    public long OrderId { get; set; }
    public string? ExtraInstruction { get; set; }
    public DateTime OrderDate { get; set; }
    public List<Table> tableList { get; set; } = null!;
    public long SectionId { get; set; }
    public string SectionName { get; set; } = null!;
    public List<ItemOrderViewModel> itemOrderVM { get; set; } = null!;

}
