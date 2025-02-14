using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Permission
{
    public long PermissionId { get; set; }

    public string PermissionsName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Action> ActionCustomersNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionMenuNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionOrdersNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionRolepermissionNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionTablesectionNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionTaxfeeNavigations { get; } = new List<Action>();

    public virtual ICollection<Action> ActionUsersNavigations { get; } = new List<Action>();
}
