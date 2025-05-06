using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class TaxInvoiceMapping
{
    public long TaxInvoiceId { get; set; }

    public long TaxId { get; set; }

    public long InvoiceId { get; set; }

    public string TaxName { get; set; } = null!;

    public decimal TaxAmount { get; set; }

    public bool IsDelete { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Tax Tax { get; set; } = null!;
}
