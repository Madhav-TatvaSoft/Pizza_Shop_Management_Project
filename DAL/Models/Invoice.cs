using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? ModifiedBy { get; set; }

    public long? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<TaxInvoiceMapping> TaxInvoiceMappings { get; } = new List<TaxInvoiceMapping>();
}
