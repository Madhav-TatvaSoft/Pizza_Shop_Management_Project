using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class User
{
    public long UserId { get; set; }

    public long UserloginId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public long Phone { get; set; }

    public string Username { get; set; } = null!;

    public string? ProfileImage { get; set; }

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

    public bool? Status { get; set; }

    public virtual ICollection<AssignTable> AssignTableCreatedByNavigations { get; } = new List<AssignTable>();

    public virtual ICollection<AssignTable> AssignTableModifiedByNavigations { get; } = new List<AssignTable>();

    public virtual ICollection<Category> CategoryCreatedByNavigations { get; } = new List<Category>();

    public virtual ICollection<Category> CategoryModifiedByNavigations { get; } = new List<Category>();

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Customer> CustomerCreatedByNavigations { get; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerModifiedByNavigations { get; } = new List<Customer>();

    public virtual ICollection<User> InverseCreatedByNavigation { get; } = new List<User>();

    public virtual ICollection<User> InverseModifiedByNavigation { get; } = new List<User>();

    public virtual ICollection<Invoice> InvoiceCreatedByNavigations { get; } = new List<Invoice>();

    public virtual ICollection<Invoice> InvoiceModifiedByNavigations { get; } = new List<Invoice>();

    public virtual ICollection<Item> ItemCreatedByNavigations { get; } = new List<Item>();

    public virtual ICollection<Item> ItemModifiedByNavigations { get; } = new List<Item>();

    public virtual ICollection<Itemtype> ItemtypeCreatedByNavigations { get; } = new List<Itemtype>();

    public virtual ICollection<Itemtype> ItemtypeModifiedByNavigations { get; } = new List<Itemtype>();

    public virtual ICollection<Kot> KotCreatedByNavigations { get; } = new List<Kot>();

    public virtual ICollection<Kot> KotModifiedByNavigations { get; } = new List<Kot>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Modifier> ModifierCreatedByNavigations { get; } = new List<Modifier>();

    public virtual ICollection<Modifier> ModifierModifiedByNavigations { get; } = new List<Modifier>();

    public virtual ICollection<Modifiergroup> ModifiergroupCreatedByNavigations { get; } = new List<Modifiergroup>();

    public virtual ICollection<Modifiergroup> ModifiergroupModifiedByNavigations { get; } = new List<Modifiergroup>();

    public virtual ICollection<Modifierorder> ModifierorderCreatedByNavigations { get; } = new List<Modifierorder>();

    public virtual ICollection<Modifierorder> ModifierorderModifiedByNavigations { get; } = new List<Modifierorder>();

    public virtual ICollection<Order> OrderCreatedByNavigations { get; } = new List<Order>();

    public virtual ICollection<Order> OrderModifiedByNavigations { get; } = new List<Order>();

    public virtual ICollection<Orderdetail> OrderdetailCreatedByNavigations { get; } = new List<Orderdetail>();

    public virtual ICollection<Orderdetail> OrderdetailModifiedByNavigations { get; } = new List<Orderdetail>();

    public virtual ICollection<Rating> RatingCreatedByNavigations { get; } = new List<Rating>();

    public virtual ICollection<Rating> RatingModifiedByNavigations { get; } = new List<Rating>();

    public virtual ICollection<Section> SectionCreatedByNavigations { get; } = new List<Section>();

    public virtual ICollection<Section> SectionModifiedByNavigations { get; } = new List<Section>();

    public virtual State? State { get; set; }

    public virtual ICollection<Table> TableCreatedByNavigations { get; } = new List<Table>();

    public virtual ICollection<Table> TableModifiedByNavigations { get; } = new List<Table>();

    public virtual ICollection<Tax> TaxCreatedByNavigations { get; } = new List<Tax>();

    public virtual ICollection<Tax> TaxModifiedByNavigations { get; } = new List<Tax>();

    public virtual UserLogin Userlogin { get; set; } = null!;

    public virtual ICollection<Waitinglist> WaitinglistCreatedByNavigations { get; } = new List<Waitinglist>();

    public virtual ICollection<Waitinglist> WaitinglistModifiedByNavigations { get; } = new List<Waitinglist>();
}
