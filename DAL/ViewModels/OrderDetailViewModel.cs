using DAL.Models;

namespace DAL.ViewModels;

public class OrderDetailViewModel
{
    // Order Details
    public string InvoiceNo { get; set; } = null!;
    public long OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = null!;

    // Customer Details
    public CustomerViewModel customerVM { get; set; } = null!;
    public int NoOfPerson { get; set; }

    // Tables Section Details
    public List<Table> tableList { get; set; } = null!;
    public string SectionName { get; set; } = null!;

    // List of View Models
    public List<ItemOrderViewModel> itemOrderVM { get; set; } = null!;
    public List<TaxInvoiceViewModel> taxInvoiceVM { get; set; } = null!;

    // Extra Fields
    public int SubTotalAmount { get; set; }
    public int TotalAmount { get; set; }

}
