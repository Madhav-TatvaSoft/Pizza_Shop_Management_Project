using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class UserLogin
{
    public long UserloginId { get; set; }

    public long RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<User> Users { get; } = new List<User>();
}