using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Role
{
    public long RoleId { get; set; }

    public string RoleName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Rolepermissionmapping> Rolepermissionmappings { get; } = new List<Rolepermissionmapping>();

    public virtual ICollection<UserLogin> UserLogins { get; } = new List<UserLogin>();
}
