using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Implementation;

public class UserService
{
    private readonly PizzaShopDbContext _context;
    private readonly JWTService _JWTService;
    public UserService(PizzaShopDbContext context, JWTService jwtService)
    {
        _context = context;
        _JWTService = jwtService;
    }


    public List<User> GetUserProfileDetails(string cookieSavedToken)
    {
        var Email = _JWTService.GetClaimValue(cookieSavedToken, "email");
        var data = _context.Users.Include(x => x.Userlogin).Where(x => x.Userlogin.Email == Email).ToList();

        return data;
    }
    public Task<User> UpdateUser(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
        return Task.FromResult(user);
    }
}

