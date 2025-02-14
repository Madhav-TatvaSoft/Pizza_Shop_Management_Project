using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Orderdetail
{
    public long OrderdetailId { get; set; }

    public long OrderId { get; set; }

    public long ItemId { get; set; }

    public int Quantity { get; set; }

    public string? ExtraInstruction { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<Modifierorder> Modifierorders { get; } = new List<Modifierorder>();

    public virtual Order Order { get; set; } = null!;
}
