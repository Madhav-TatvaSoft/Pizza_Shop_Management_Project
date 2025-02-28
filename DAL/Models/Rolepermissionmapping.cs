using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Rolepermissionmapping
{
    public long RolepermissionmappingId { get; set; }

    public long RoleId { get; set; }

    public long PermissionId { get; set; }

    public bool Canview { get; set; }

    public bool Canaddedit { get; set; }

    public bool Candelete { get; set; }

    public bool Permissioncheck { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
