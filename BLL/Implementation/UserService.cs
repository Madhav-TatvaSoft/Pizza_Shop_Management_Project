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

    // public List<User> GetUserList(string Email)
    // {
    //     return _context.Users.Include(x => x.Userlogin).Include(x => x.Userlogin.Role).ToList();
    // }


    public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PaginatedList(List<T> items, int totalRecords, int page, int pageSize)
    {
        Items = items;
        TotalRecords = totalRecords;
        Page = page;
        PageSize = pageSize;
    }
}
    public async Task<PaginatedList<User>> GetUsersAsync(int page, int pageSize, string search)
    {
        var query = _context.Users.Include(u => u.Userlogin.Role)
                                  .Where(u => !u.Isdelete);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || 
                                     u.Userlogin.Role.RoleName.Contains(search) || 
                                     u.Userlogin.Email.Contains(search));
        }

        int totalRecords = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<User>(users, totalRecords, page, pageSize);
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
        // user.ProfileImage = userVM.ProfileImage;
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

    public bool DeleteUser(int id)
    {
        var user = _context.Users.FirstOrDefault(x => x.UserId == id).Isdelete = true;
        _context.SaveChanges();
        return true;
    }

}