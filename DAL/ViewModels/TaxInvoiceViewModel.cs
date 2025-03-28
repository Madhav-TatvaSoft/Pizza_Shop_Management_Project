namespace DAL.ViewModels;

public class TaxInvoiceViewModel
{
    public string TaxName { get; set; } = null!;

    public string TaxType { get; set; } = null!;

    public decimal TaxValue { get; set; }

}
