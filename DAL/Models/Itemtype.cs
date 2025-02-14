using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Itemtype
{
    public long ItemTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string TypeImage { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual ICollection<Item> Items { get; } = new List<Item>();
}
