using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Country
{
    public long CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public virtual ICollection<State> States { get; } = new List<State>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
