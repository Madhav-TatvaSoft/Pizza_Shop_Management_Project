using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class City
{
    public long CityId { get; set; }

    public string CityName { get; set; } = null!;

    public long StateId { get; set; }

    public virtual State State { get; set; } = null!;

    public virtual ICollection<User> Users { get; } = new List<User>();
}
