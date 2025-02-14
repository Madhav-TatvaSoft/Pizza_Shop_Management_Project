using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class User
{
    public long UserId { get; set; }

    public long UserloginId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int Phone { get; set; }

    public string Username { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public string Status { get; set; } = null!;

    public long? CountryId { get; set; }

    public long? StateId { get; set; }

    public long? CityId { get; set; }

    public string? Address { get; set; }

    public long? Zipcode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long? ModifiedBy { get; set; }

    public bool Isdelete { get; set; }

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<User> InverseCreatedByNavigation { get; } = new List<User>();

    public virtual ICollection<User> InverseModifiedByNavigation { get; } = new List<User>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual State? State { get; set; }

    public virtual UserLogin Userlogin { get; set; } = null!;
}
