using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Permission
{
    public long PermissionId { get; set; }

    public string PermissionsName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Rolepermissionmapping> Rolepermissionmappings { get; } = new List<Rolepermissionmapping>();
}
