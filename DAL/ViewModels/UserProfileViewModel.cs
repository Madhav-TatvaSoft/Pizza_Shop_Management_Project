namespace DAL.ViewModels;

public class UserProfileViewModel
{
    public string FirstName { get; set; } 

    public string LastName { get; set; } 

    public string Username { get; set; } 

    public long Phone { get; set; }

    public string CountryName { get; set; } = null!;
    public string StateName { get; set; } = null!;
    public string CityName { get; set; } = null!;

     public string? Address { get; set; } = null!;

    public long? Zipcode { get; set; } = null!;
}
