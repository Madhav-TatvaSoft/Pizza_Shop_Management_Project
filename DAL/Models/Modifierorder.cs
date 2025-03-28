using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Modifierorder
{
    public long ModifierOrderId { get; set; }

    public long OrderdetailId { get; set; }

    public long ModifierId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? ModifiedBy { get; set; }

    public long? CreatedBy { get; set; }

    public int? ModifierQuantity { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Modifier Modifier { get; set; } = null!;

    public virtual Orderdetail Orderdetail { get; set; } = null!;
}
