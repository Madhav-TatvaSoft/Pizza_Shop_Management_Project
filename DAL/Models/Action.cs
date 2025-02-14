using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Action
{
    public long ActionId { get; set; }

    public long Users { get; set; }

    public long Rolepermission { get; set; }

    public long Menu { get; set; }

    public long Tablesection { get; set; }

    public long Taxfee { get; set; }

    public long Orders { get; set; }

    public long Customers { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public virtual Permission CustomersNavigation { get; set; } = null!;

    public virtual Permission MenuNavigation { get; set; } = null!;

    public virtual Permission OrdersNavigation { get; set; } = null!;

    public virtual Permission RolepermissionNavigation { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; } = new List<Role>();

    public virtual Permission TablesectionNavigation { get; set; } = null!;

    public virtual Permission TaxfeeNavigation { get; set; } = null!;

    public virtual Permission UsersNavigation { get; set; } = null!;
}
