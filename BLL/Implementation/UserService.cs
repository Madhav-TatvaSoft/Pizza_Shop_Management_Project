using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class UserService : IUserService
{
    private readonly PizzaShopDbContext _context;
    private readonly JWTService _JWTService;
    public UserService(PizzaShopDbContext context, JWTService jwtService)
    {
        _context = context;
        _JWTService = jwtService;
    }

    public List<Country> GetCountry(){
        return _context.Countries.ToList();
    }

    public List<State> GetState(){
        return _context.States.ToList();
    }

    public List<City> GetCity(){
        return _context.Cities.ToList();
    }

    public List<User> GetUserProfileDetails(string cookieSavedToken)
    {
        var Email = _JWTService.GetClaimValue(cookieSavedToken, "email");
        var data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email).ToList();

        return data;
    }

    public bool UpdateUser(User user, string Email)
    {

        User userdetails = _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email);
        userdetails.FirstName = user.FirstName;
        userdetails.LastName = user.LastName;
        userdetails.Username = user.Username;
        userdetails.Address = user.Address;
        userdetails.Phone = user.Phone;
        userdetails.Zipcode = user.Zipcode;
        userdetails.CountryId = user.CountryId;
        userdetails.StateId = user.StateId;
        userdetails.CityId = user.CityId;

        _context.Update(userdetails);
        _context.SaveChanges();
        return true;
    }

    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email)
    {
        var userdetails = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
        if (userdetails.Password == changepassword.CurrentPassword)
        {
            userdetails.Password = changepassword.NewPassword;
            _context.Update(userdetails);
            _context.SaveChanges();
            return true;
        }
        return false;
    }

}

