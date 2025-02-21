using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserService
{
    public List<Country> GetCountry();
    public List<State> GetState();
    public List<City> GetCity();
    public List<User> GetUserProfileDetails(string cookieSavedToken);
    public bool UpdateUser(User user, string Email);
    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email);

}
