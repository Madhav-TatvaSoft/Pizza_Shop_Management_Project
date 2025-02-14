using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Category
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual ICollection<Item> Items { get; } = new List<Item>();
}
