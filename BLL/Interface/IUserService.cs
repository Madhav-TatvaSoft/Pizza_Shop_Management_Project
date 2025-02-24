using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserService
{
    public List<Country> GetCountry();
    public List<State> GetState(long? countryId);
    public List<City> GetCity(long? stateId);
    public List<User> GetUserProfileDetails(string cookieSavedToken);
    public bool UpdateUser(User user, string Email);
    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email);

    public Task<bool> AddUser(AddUserViewModel userVM , String Email);

}
