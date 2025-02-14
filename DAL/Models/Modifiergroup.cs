using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Modifiergroup
{
    public long ModifierGrpId { get; set; }

    public string ModifierGrpName { get; set; } = null!;

    public string? Desciption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual ICollection<Modifier> Modifiers { get; } = new List<Modifier>();
}
