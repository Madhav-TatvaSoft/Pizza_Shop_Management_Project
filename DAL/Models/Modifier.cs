using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Modifier
{
    public long ModifierId { get; set; }

    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    public string? Description { get; set; }

    public string? Unit { get; set; }

    public decimal? Rate { get; set; }

    public int Quantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? ModifiedBy { get; set; }

    public long? CreatedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Modifiergroup ModifierGrp { get; set; } = null!;

    public virtual ICollection<Modifierorder> Modifierorders { get; } = new List<Modifierorder>();
}
