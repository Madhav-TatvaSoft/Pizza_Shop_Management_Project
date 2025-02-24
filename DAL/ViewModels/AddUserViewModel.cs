using Microsoft.AspNetCore.Http;

namespace DAL.ViewModels;

public class AddUserViewModel
{
    public long UserId { get; set; }

    public long UserloginId { get; set; }

    public long RoleId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } 

    public string Password { get; set; }

    public string Image { get; set; }

    public IFormFile ProfileImage { get; set; }

    public long? CountryId { get; set; }

    public long? StateId { get; set; }

    public long? CityId { get; set; }

    public string? Address { get; set; }

    public long? Zipcode { get; set; }

    public int Phone { get; set; }
}
