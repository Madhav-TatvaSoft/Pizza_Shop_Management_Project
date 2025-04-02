namespace DAL.ViewModels;

public class TaxInvoiceViewModel
{
    public long TaxId { get; set; }
    public string TaxName { get; set; } = null!;

    public string TaxType { get; set; } = null!;

    public decimal TaxValue { get; set; }

    public long InvoiceId{get; set;}

}