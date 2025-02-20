using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Rating
{
    public long RatingId { get; set; }

    public long CustomerId { get; set; }

    public string? Food { get; set; }

    public string? Service { get; set; }

    public string Ambience { get; set; } = null!;

    public string? Review { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
