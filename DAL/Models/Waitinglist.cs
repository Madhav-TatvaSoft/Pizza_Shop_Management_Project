using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Waitinglist
{
    public long WaitingId { get; set; }

    public long CustomerId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public int NoOfPerson { get; set; }

    public long TableId { get; set; }

    public bool Isassign { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Table Table { get; set; } = null!;
}
