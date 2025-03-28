using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class ModifierGroupMapping
{
    public long MappingId { get; set; }

    public long ModifierId { get; set; }

    public long ModifierGrpId { get; set; }

    public bool Isdelete { get; set; }

    public virtual Modifier Modifier { get; set; } = null!;

    public virtual Modifiergroup ModifierGrp { get; set; } = null!;
}
