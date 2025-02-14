using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Favouriteitem
{
    public long CustomerId { get; set; }

    public long ItemId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long? ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;
}
