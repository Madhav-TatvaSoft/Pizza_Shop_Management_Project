using BLL.Interface;
// using BLL.Helpers;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class UserService : IUserService
{
    private readonly PizzaShopDbContext _context;
    private readonly JWTService _JWTService;

    private readonly UserLoginService _userLoginService;
    public UserService(PizzaShopDbContext context, JWTService jwtService, UserLoginService userLoginService)
    {
        _context = context;
        _JWTService = jwtService;
        _userLoginService = userLoginService;
    }

    public List<Country> GetCountry()
    {

        return _context.Countries.ToList();
    }

    public List<State> GetState(long? countryId)
    {
        return _context.States.Where(x => x.CountryId == countryId).ToList();
    }

    public List<City> GetCity(long? stateId)
    {
        return _context.Cities.Where(x => x.StateId == stateId).ToList();
    }

    public List<AddUserViewModel> GetUserProfileDetails(string cookieSavedToken)
    {
        var Email = _JWTService.GetClaimValue(cookieSavedToken, "email");
        var data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email)
        .Select(
            x => new AddUserViewModel
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.Username,
                Phone = x.Phone,
                RoleId = x.Userlogin.RoleId,
                Email = x.Userlogin.Email,
                Image = x.ProfileImage,
                StateId = x.StateId,
                CityId = x.CityId,
                Status = x.Status,
                Address = x.Address,
                Zipcode = x.Zipcode,
                CountryId = x.CountryId
            }
        ).ToList();

        return data;
    }

    public bool UpdateUser(AddUserViewModel user, string Email)
    {

        User userdetails = _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email);
        userdetails.FirstName = user.FirstName;
        userdetails.LastName = user.LastName;
        userdetails.Username = user.Username;
        userdetails.Address = user.Address;
        if (user.Image != null)
        {
            userdetails.ProfileImage = user.Image;
        }
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

    // public async Task<(List<User>, int)> GetUserList(string search, int PageNo, int PageSize)
    // {
    //     var query = _context.Users.Include(x => x.Userlogin).ThenInclude(u => u.Role).Where(u => u.Isdelete == false);

    //     // Apply search filter
    //     if (!string.IsNullOrEmpty(search))
    //     {
    //         string lowerSearchTerm = search.ToLower();
    //         query = query.Where(u =>
    //             u.FirstName.ToLower().Contains(lowerSearchTerm) ||
    //             u.Userlogin.Email.ToLower().Contains(lowerSearchTerm) ||
    //             u.Userlogin.Role.RoleName.ToLower().Contains(lowerSearchTerm)
    //         );
    //     }

    //     int TotalRecord = await query.CountAsync();
    //     var users = await query
    //                            .OrderBy(u => u.FirstName)
    //                            .Skip((PageNo - 1) * PageSize)
    //                            .Take(PageSize)
    //                            .ToListAsync();

    //     // return _context.Users.Include(x => x.Userlogin).Include(x => x.Userlogin.Role).ToList();
    //     return (users, TotalRecord);
    // }

    public List<User> GetUserList(string searchTerm, int pageNumber, int pageSize, out int totalRecords)
{
    var query = _context.Users
        .Include(u => u.Userlogin)
        .ThenInclude(l => l.Role)
       .Where(u => u.Isdelete == false);// Exclude deleted users

    // Apply search filter
    if (!string.IsNullOrEmpty(searchTerm))
    {
        string lowerSearchTerm = searchTerm.ToLower();
        query = query.Where(u => 
            u.FirstName.ToLower().Contains(lowerSearchTerm) ||
            u.LastName.ToLower().Contains(lowerSearchTerm) ||
            u.Userlogin.Email.ToLower().Contains(lowerSearchTerm) ||
            u.Phone.ToString().Contains(lowerSearchTerm) ||
            u.Userlogin.Role.RoleName.ToLower().Contains(lowerSearchTerm)
        );
    }

    // Get total records count (before pagination)
    totalRecords = query.Count();

    // Apply pagination
    var users = query
        .OrderBy(u => u.FirstName) // Sorting by name
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return users;
}


    public List<Role> GetRole()
    {
        return _context.Roles.ToList();
    }

    public async Task<bool> AddUser(AddUserViewModel adduser, String Email)
    {
        if (_context.UserLogins.Any(x => x.Email == adduser.Email))
        {
            return false;
        }

        UserLogin userlogin = new UserLogin();
        userlogin.Email = adduser.Email;
        userlogin.Password = _userLoginService.EncryptPassword(adduser.Password);
        userlogin.RoleId = adduser.RoleId;

        await _context.AddAsync(userlogin);
        await _context.SaveChangesAsync();

        User user = new User();
        user.UserloginId = userlogin.UserloginId;
        user.FirstName = adduser.FirstName;
        user.LastName = adduser.LastName;
        user.Phone = adduser.Phone;
        user.Username = adduser.Username;
        user.ProfileImage = adduser.Image;
        // user.Status = userVM.Status;
        user.CountryId = adduser.CountryId;
        user.StateId = adduser.StateId;
        user.CityId = adduser.CityId;
        user.Address = adduser.Address;
        user.Zipcode = adduser.Zipcode;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public List<AddUserViewModel> GetUserByEmail(string email)
    {
        var data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == email).Select(
            x => new AddUserViewModel
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.Username,
                Phone = x.Phone,
                RoleId = x.Userlogin.RoleId,
                Email = x.Userlogin.Email,
                // ProfileImage = x.Image,
                StateId = x.StateId,
                CityId = x.CityId,
                Status = x.Status,
                Address = x.Address,
                Zipcode = x.Zipcode,
                CountryId = x.CountryId
            }
        ).ToList();
        return data;
    }


    public bool EditUser(AddUserViewModel user, string Email)
    {
        var userdetails = _context.Users.Include(x => x.Userlogin).FirstOrDefault(x => x.Userlogin.Email == Email);
        userdetails.FirstName = user.FirstName;
        userdetails.LastName = user.LastName;
        userdetails.Username = user.Username;
        userdetails.ProfileImage = user.Image;
        userdetails.Address = user.Address;
        userdetails.Phone = user.Phone;
        userdetails.Zipcode = user.Zipcode;
        userdetails.CountryId = user.CountryId;
        userdetails.StateId = user.StateId;
        userdetails.CityId = user.CityId;
        userdetails.Userlogin.RoleId = user.RoleId;
        userdetails.Status = user.Status;

        _context.Update(userdetails);
        _context.SaveChanges();
        return true;
    }
    public async Task<bool> DeleteUser(string Email)
    {
        var userlogin = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
        var user = _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email);

        userlogin.Isdelete = true;
        _context.Update(userlogin);

        user.Isdelete = true;
        _context.Update(user);

        await _context.SaveChangesAsync();
        return true;
    }

}