using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL_Business_Logic_Layer_;

public class UserLoginService
{
    private readonly PizzaShopDbContext _context;

    public UserLoginService(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserLogin>> GetUserLogins()
    {
        var pizzaShopDbContext = _context.UserLogins.Include(u => u.Role);
        return await pizzaShopDbContext.ToListAsync();
    }
    public async Task<bool> VerifyUserLogin(UserLoginViewModel userLogin)
    {
        if (_context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email && e.Password == userLogin.Password) != null)
        {
            return true;
        }
        return false;
    }
}
