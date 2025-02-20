using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Kot
{
    public long KotId { get; set; }

    public long OrderId { get; set; }

    public bool Isready { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;
}
