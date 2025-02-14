using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class State
{
    public long StateId { get; set; }

    public string StateName { get; set; } = null!;

    public long CountryId { get; set; }

    public virtual ICollection<City> Cities { get; } = new List<City>();

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<User> Users { get; } = new List<User>();
}
