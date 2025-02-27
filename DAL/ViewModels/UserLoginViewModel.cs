using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class UserLoginViewModel
{
    public string Email { get; set; }

    public string Password { get; set; }
    public bool Remember_me { get; set; }
}