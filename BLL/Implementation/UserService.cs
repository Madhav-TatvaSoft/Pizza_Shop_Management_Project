using System.Net;
using System.Net.Mail;
using BLL.common;
using BLL.Interface;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BLL.Implementation;

public class UserService : IUserService
{
    private readonly PizzaShopDbContext _context;
    private readonly IJWTService _JWTService;
    private readonly IUserLoginService _userLoginService;
    private readonly IConfiguration _configuration;

    public UserService(PizzaShopDbContext context, IJWTService jwtService, IUserLoginService userLoginService, IConfiguration configuration)
    {
        _context = context;
        _JWTService = jwtService;
        _userLoginService = userLoginService;
        _configuration = configuration;
    }

    #region GetCountry
    public List<Country> GetCountry()
    {
        return _context.Countries.ToList();
    }
    #endregion

    #region GetState
    public List<State> GetState(long? countryId)
    {
        return _context.States.Where(x => x.CountryId == countryId).ToList();
    }
    #endregion

    #region GetCity
    public List<City> GetCity(long? stateId)
    {
        return _context.Cities.Where(x => x.StateId == stateId).ToList();
    }
    #endregion

    #region GetProfileDetails
    public List<AddUserViewModel> GetUserProfileDetails(string cookieSavedToken)
    {
        string Email = _JWTService.GetClaimValue(cookieSavedToken, "email");
        List<AddUserViewModel>? data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email)
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
    #endregion

    #region Update User Profile
    public bool UpdateUserProfile(AddUserViewModel user, string Email)
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
    #endregion

    #region ChangePassword
    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email)
    {
        UserLogin? userdetails = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
        if (userdetails.Password == changepassword.CurrentPassword)
        {
            userdetails.Password = changepassword.NewPassword;
            _context.Update(userdetails);
            _context.SaveChanges();
            return true;
        }
        return false;
    }
    #endregion

    #region GetUserList
    public PaginationViewModel<User> GetUserList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5)
    {
        IQueryable<User>? query = _context.Users
            .Include(u => u.Userlogin)
            .ThenInclude(u => u.Role)
            .Where(u => u.Isdelete == false)
            .AsQueryable();

        // Apply search 
        if (!string.IsNullOrEmpty(search))
        {
            string lowerSearchTerm = search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                u.Userlogin.Email.ToLower().Contains(lowerSearchTerm) ||
                u.Userlogin.Role.RoleName.ToLower().Contains(lowerSearchTerm)
            );
        }

        // Get total records count (before pagination)
        int totalCount = query.Count();

        //sorting
        switch (sortColumn)
        {
            case "Name":
                query = sortDirection == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName);
                break;

            case "Role":
                query = sortDirection == "asc" ? query.OrderBy(u => u.Userlogin.Role.RoleName) : query.OrderByDescending(u => u.Userlogin.Role.RoleName);
                break;
        }

        // Apply pagination
        List<User>? items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginationViewModel<User>(items, totalCount, pageNumber, pageSize);
    }
    #endregion

    #region GetRole
    public List<Role> GetRole()
    {
        return _context.Roles.ToList();
    }
    #endregion

    #region AddUser
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
    #endregion

    #region SendEmail when User is Added
    public async Task<bool> SendEmail(string Password, string Username, string Email)
    {
        if (Email != null && Password != null && Username != null)
        {
            try
            {
                MailAddress senderEmail = new MailAddress("tatvasoft.pca155@outlook.com", "sender");
                MailAddress receiverEmail = new MailAddress(Email, "reciever");
                string password = "P}N^{z-]7Ilp";
                string sub = "User Added";
                string body = EmailTemplate.AddUserEmail(Password, Username);
                SmtpClient smtp = new SmtpClient
                {
                    Host = _configuration["smtp:Host"],
                    Port = int.Parse(_configuration["smtp:Port"]),
                    EnableSsl = bool.Parse(_configuration["smtp:EnableSsl"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = bool.Parse(_configuration["smtp:UseDefaultCredentials"]),
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail))
                {
                    mess.Subject = sub;
                    mess.Body = body;
                    mess.IsBodyHtml = true;
                    await smtp.SendMailAsync(mess);
                }
            }
            catch (Exception exp)
            {
                return false;
            }

            return true;
        }
        return false;
    }
    #endregion

    #region GetUserByEmail In Edit Page
    public List<AddUserViewModel> GetUserByEmail(string email)
    {
        List<AddUserViewModel>? data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == email).Select(
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
    #endregion

    #region EditUser
    public async Task<bool> EditUser(AddUserViewModel user, string Email)
    {
        User? userdetails = _context.Users.Include(x => x.Userlogin).FirstOrDefault(x => x.Userlogin.Email == Email);
        userdetails.FirstName = user.FirstName;
        userdetails.LastName = user.LastName;
        userdetails.Username = user.Username;
        if (userdetails.ProfileImage == null)
        {
            userdetails.ProfileImage = user.Image;
        }
        userdetails.Address = user.Address;
        userdetails.Phone = user.Phone;
        userdetails.Zipcode = user.Zipcode;
        userdetails.CountryId = user.CountryId;
        userdetails.StateId = user.StateId;
        userdetails.CityId = user.CityId;
        userdetails.Userlogin.RoleId = user.RoleId;
        userdetails.Status = user.Status;

        _context.Update(userdetails);
        await _context.SaveChangesAsync();
        return true;
    }
    #endregion

    #region DeleteUser
    public async Task<bool> DeleteUser(string Email)
    {
        UserLogin? userlogin = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
        User? user = _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email);

        userlogin.Isdelete = true;
        _context.Update(userlogin);

        user.Isdelete = true;
        _context.Update(user);

        await _context.SaveChangesAsync();
        return true;
    }
    #endregion

    #region UserNameExists in Adding
    public async Task<bool> IsUserNameExists(string Username)
    {
        User? IsUserNameExists = await _context.Users.FirstOrDefaultAsync(x => x.Username == Username && !x.Isdelete);
        if (IsUserNameExists == null)
        {
            return false;
        }
        return true;
    }
    #endregion

    #region UserNameExists in Editing
    public bool IsUserNameExistsForEdit(string Username, string Email)
    {
        List<User> duplicateUsername = _context.Users.Where(x => x.Username == Username && x.Userlogin.Email != Email && !x.Isdelete).ToList();
        return (duplicateUsername.Count >= 1) ? true : false;
    }
    #endregion

    #region Get User From Email
    public List<User> getUserFromEmail(string token)
    {
        var claims = _JWTService.GetClaimsFromToken(token);
        string? Email = _JWTService.GetClaimValue(token, "email");
        return _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email).ToList();
    }
    #endregion

}