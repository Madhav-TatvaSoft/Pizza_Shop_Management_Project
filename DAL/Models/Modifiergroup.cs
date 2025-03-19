using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Modifiergroup
{
    public long ModifierGrpId { get; set; }

    public string ModifierGrpName { get; set; } = null!;

    public string? Desciption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ItemModifierGroupMapping> ItemModifierGroupMappings { get; } = new List<ItemModifierGroupMapping>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Modifier> Modifiers { get; } = new List<Modifier>();
}
