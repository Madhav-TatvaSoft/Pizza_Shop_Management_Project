using DAL.Models;

namespace DAL.ViewModels;

public class OrderDetailViewModel
{
    // Order Details
    public long InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = null!;
    public long OrderId { get; set; }
    public DateTime? OrderDate { get; set; }

    // New Added
    public string Status { get; set; } = null!;
    public string? OrderInstruction { get; set; }
    public int ReadyQuantity { get; set; }
    public long PaymentmethodId { get; set; }

    // new one 
    public long RatingId { get; set; }

    // Customer Details
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public long? PhoneNo { get; set; }
    public string? Email { get; set; }
    public int NoOfPerson { get; set; }

    // Tables Section Details
    public List<Table> tableList { get; set; } = null!;
    public long SectionId { get; set; }
    public string SectionName { get; set; } = null!;

    // List of View Models
    public List<ItemOrderViewModel> itemOrderVM { get; set; } = null!;
    public List<TaxInvoiceViewModel> taxInvoiceVM { get; set; } = null!;

    // Extra Fields
    public decimal SubTotalAmountOrder { get; set; }
    public decimal TotalAmountOrder { get; set; }
    public DateTime? ModifiedOn { get; set; }

    // Rating
    public RatingsViewModel ratingVM { get; set; }

}