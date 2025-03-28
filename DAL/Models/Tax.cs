using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Tax
{
    public long TaxId { get; set; }

    public string TaxName { get; set; } = null!;

    public string TaxType { get; set; } = null!;

    public decimal TaxValue { get; set; }

    public bool? Isenable { get; set; }

    public bool? Isdefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? ModifiedBy { get; set; }

    public long? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<TaxInvoiceMapping> TaxInvoiceMappings { get; } = new List<TaxInvoiceMapping>();
}
