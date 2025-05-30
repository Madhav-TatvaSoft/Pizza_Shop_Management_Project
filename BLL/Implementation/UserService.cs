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

    public DashboardViewModel GetDashboardDetails(string Range = "", string startDate = "", string endDate = "")
    {

        DashboardViewModel dashboard = new DashboardViewModel();

        if (Range == "Today")
        {
            startDate = DateTime.Now.ToString("yyyy-MM-dd");
            endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        }
        else if (Range == "Last 7 days")
        {
            startDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        }
        else if (Range == "Last 30 days")
        {
            startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        }
        else if (Range == "Current Month")
        {
            startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            endDate = DateTime.Now.ToString("yyyy-MM-dd");
        }
        else if (Range == "Custom Date")
        {
            startDate = startDate;
            endDate = endDate;
        }

        dashboard.TotalSales = _context.Orders
                                .Where(x => !x.Isdelete && x.Status == "Completed" && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
                                .Sum(x => x.TotalAmount);
        dashboard.TotalOrders = _context.Orders
                                .Where(x => !x.Isdelete && x.Status == "Completed" && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
                                .Count();
        dashboard.AvgOrderValue = Math.Round(_context.Orders
                                    .Where(x => !x.Isdelete && x.Status == "Completed" && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
                                    .AsEnumerable()
                                    .Select(x => x.TotalAmount)
                                    .DefaultIfEmpty(0)
                                    .Average(), 2);

        dashboard.AvgWaitingTime = Math.Round(_context.Waitinglists
            .Where(w => !w.Isdelete && w.CreatedAt.HasValue && w.AssignedAt.HasValue && w.CreatedAt >= DateTime.Parse(startDate) && w.CreatedAt <= DateTime.Parse(endDate))
            .AsEnumerable()
            .Select(w => (w.AssignedAt.Value - w.CreatedAt.Value).TotalMinutes)
            .DefaultIfEmpty(0)
            .Average(), 2);

        dashboard.TopSellingItems = _context.Orderdetails.Include(x => x.Item).Where(x => !x.Isdelete && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
            .GroupBy(x => x.ItemId)
            .Select(g => new SellingItemViewModel
            {
                ItemId = g.Key,
                ItemImage = _context.Items.FirstOrDefault(i => i.ItemId == g.Key).ItemImage,
                ItemName = _context.Items.FirstOrDefault(i => i.ItemId == g.Key).ItemName,
                ItemCount = g.Count()
            })
            .OrderByDescending(x => x.ItemCount)
            .Take(2)
            .ToList();

        dashboard.LeastSellingItems = _context.Orderdetails.Include(x => x.Item).Where(x => !x.Isdelete && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
            .GroupBy(x => x.ItemId)
            .Select(g => new SellingItemViewModel
            {
                ItemId = g.Key,
                ItemImage = _context.Items.FirstOrDefault(i => i.ItemId == g.Key).ItemImage,
                ItemName = _context.Items.FirstOrDefault(i => i.ItemId == g.Key).ItemName,
                ItemCount = g.Count()
            })
            .OrderBy(x => x.ItemCount)
            .Take(2)
            .ToList();

        dashboard.WaitingListCount = _context.Waitinglists.Where(x => !x.Isdelete && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate) && !x.Isassign).Count();

        dashboard.NewCustomerCount = _context.Customers
            .Where(x => !x.Isdelete && x.CreatedAt >= DateTime.Parse(startDate) && x.CreatedAt <= DateTime.Parse(endDate))
            .Count();

        return dashboard;
    }

    public (List<decimal?>, List<int>) GetRevenueAndCustomer(string Range, string startDate, string endDate)
    {
        List<Order> orders = _context.Orders.Where(o => !o.Isdelete && o.Status == "Completed").ToList();
        List<Customer> customers = _context.Customers.Where(c => !c.Isdelete).ToList();
        List<decimal?> RevenueList = new();
        List<int> CustomerList = new();

        switch (Range)
        {
            case "Today":
                for (int i = 0; i <= 23; i++)
                {
                    CustomerList.Add(customers.Where(x => x.CreatedAt?.Date == DateTime.Now.Date && x.CreatedAt?.Hour == i).ToList().Count);
                    RevenueList.Add(orders.Where(x => x.CreatedAt?.Date == DateTime.Now.Date && x.CreatedAt?.Hour == i).Sum(x => x.TotalAmount));
                }
                break;
            case "Last 7 days":
                for (int i = -1 * 6; i <= 0; i++)
                {
                    CustomerList.Add(customers.Where(x => x.CreatedAt?.Date == DateTime.Now.AddDays(i).Date).ToList().Count);
                    RevenueList.Add(orders.Where(x => x.CreatedAt?.Date == DateTime.Now.AddDays(i).Date).Sum(x => x.TotalAmount));
                }

                break;
            case "Last 30 days":
                for (int i = -1 * 29; i <= 0; i++)
                {
                    CustomerList.Add(customers.Where(x => x.CreatedAt?.Date == DateTime.Now.AddDays(i).Date).ToList().Count);
                    RevenueList.Add(orders.Where(x => x.CreatedAt?.Date == DateTime.Now.AddDays(i).Date).Sum(x => x.TotalAmount));
                }
                break;
            case "Current Month":
                for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                {
                    CustomerList.Add(customers.Where(x => x.CreatedAt.Value.Day == i && x.CreatedAt.Value.Month == DateTime.Now.Month).ToList().Count);
                    RevenueList.Add(orders.Where(x => x.CreatedAt.Value.Day == i && x.CreatedAt.Value.Month == DateTime.Now.Month).Sum(x => x.TotalAmount));
                }
                break;
            case "Custom Date":
                if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        CustomerList.Add(customers.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date).ToList().Count);
                        RevenueList.Add(orders.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date).Sum(x => x.TotalAmount));
                    }
                }
                else if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        CustomerList.Add(customers.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).ToList().Count);
                        RevenueList.Add(orders.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).Sum(x => x.TotalAmount));
                    }
                }
                else
                {
                    string[] StartDateList = startDate.Split("-");
                    string[] EndDateList = endDate.Split("-");
                    if (StartDateList[1] == EndDateList[1])
                    {
                        for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                        {
                            CustomerList.Add(customers.Where(x => x.CreatedAt.Value.Day == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date
                                            && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).ToList().Count);
                            RevenueList.Add(orders.Where(x => x.CreatedAt.Value.Day == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date
                                            && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).Sum(x => x.TotalAmount));
                        }
                    }
                    else
                    {
                        for (int i = int.Parse(StartDateList[1]); i <= int.Parse(EndDateList[1]); i++)
                        {
                            CustomerList.Add(customers.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date
                                            && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).ToList().Count);
                            RevenueList.Add(orders.Where(x => x.CreatedAt.Value.Month == i && x.CreatedAt.Value.Date >= DateTime.Parse(startDate).Date
                                            && x.CreatedAt.Value.Date <= DateTime.Parse(endDate).Date).Sum(x => x.TotalAmount));
                        }
                    }
                }
                break;
        }
        return (RevenueList, CustomerList);
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

    #region Profile CRUD
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

    public bool UpdateUserProfile(AddUserViewModel user, string Email)
    {
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
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
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    #endregion

    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email)
    {
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                UserLogin? userdetails = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
                if (userdetails.Password == changepassword.CurrentPassword)
                {
                    userdetails.Password = changepassword.NewPassword;
                    _context.Update(userdetails);
                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

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

    public List<Role> GetRole()
    {
        return _context.Roles.ToList();
    }

    #region User CRUD
    public async Task<bool> AddUser(AddUserViewModel adduser, String Email)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
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
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

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

    public async Task<bool> EditUser(AddUserViewModel user, string Email)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
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
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<bool> DeleteUser(string Email)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                UserLogin? userlogin = _context.UserLogins.FirstOrDefault(x => x.Email == Email);
                User? user = _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email);

                userlogin.Isdelete = true;
                _context.Update(userlogin);

                user.Isdelete = true;
                _context.Update(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    #endregion

    public async Task<bool> IsUserNameExists(string Username)
    {
        User? IsUserNameExists = await _context.Users.FirstOrDefaultAsync(x => x.Username == Username && !x.Isdelete);
        if (IsUserNameExists == null)
        {
            return false;
        }
        return true;
    }

    public bool IsUserNameExistsForEdit(string Username, string Email)
    {
        List<User> duplicateUsername = _context.Users.Where(x => x.Username == Username && x.Userlogin.Email != Email && !x.Isdelete).ToList();
        return (duplicateUsername.Count >= 1) ? true : false;
    }

    public List<User> getUserFromEmail(string token)
    {
        var claims = _JWTService.GetClaimsFromToken(token);
        string? Email = _JWTService.GetClaimValue(token, "email");
        return _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email).ToList();
    }

}