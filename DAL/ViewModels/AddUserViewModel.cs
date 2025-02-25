using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;

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


    [Required(ErrorMessage = "Please select a Country")]
    public long? CountryId { get; set; }


    [Required(ErrorMessage = "Please select a State")]
    public long? StateId { get; set; }


    [Required(ErrorMessage = "Please select a City")]
    public long? CityId { get; set; }


    [Required(ErrorMessage = "Please select  Address")]

    public string? Address { get; set; }

    [Required(ErrorMessage = "Please select Zipcode")]
    public long? Zipcode { get; set; }

    public long Phone { get; set; }

    public bool? Status { get; set; }
}
