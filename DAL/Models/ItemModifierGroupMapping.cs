using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class ItemModifierGroupMapping
{
    public long ItemmodifiergroupmappingId { get; set; }

    public long ItemId { get; set; }

    public long ModifierGrpId { get; set; }

    public int Minmodifier { get; set; }

    public int Maxmodifier { get; set; }

    public bool Isdelete { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Modifiergroup ModifierGrp { get; set; } = null!;
}
