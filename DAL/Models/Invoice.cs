using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Invoice
{
    public long InvoiceId { get; set; }

    public string InvoiceNo { get; set; } = null!;

    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public decimal? CgstTax { get; set; }

    public decimal? SgstTax { get; set; }

    public decimal? GstTax { get; set; }

    public decimal? Other { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
