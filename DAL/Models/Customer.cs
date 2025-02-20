using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Customer
{
    public long CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public int? PhoneNo { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual ICollection<AssignTable> AssignTables { get; } = new List<AssignTable>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Invoice> Invoices { get; } = new List<Invoice>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();

    public virtual ICollection<Rating> Ratings { get; } = new List<Rating>();

    public virtual ICollection<Waitinglist> Waitinglists { get; } = new List<Waitinglist>();
}
