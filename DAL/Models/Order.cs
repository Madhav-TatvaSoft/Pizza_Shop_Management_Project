using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Order
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public long TableId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public long? RatingId { get; set; }

    public decimal TotalAmount { get; set; }

    public long PaymentmethodId { get; set; }

    public long PaymentStatusId { get; set; }

    public long SectionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? ModifiedBy { get; set; }

    public long? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; } = new List<Invoice>();

    public virtual ICollection<Kot> Kots { get; } = new List<Kot>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; } = new List<Orderdetail>();

    public virtual Paymentstatus PaymentStatus { get; set; } = null!;

    public virtual Paymentmethod Paymentmethod { get; set; } = null!;

    public virtual Rating? Rating { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual Table Table { get; set; } = null!;
}
