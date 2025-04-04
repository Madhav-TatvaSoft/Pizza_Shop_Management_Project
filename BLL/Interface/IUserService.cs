using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserService
{
    List<Country> GetCountry();
    List<State> GetState(long? countryId);
    List<City> GetCity(long? stateId);
    List<AddUserViewModel> GetUserProfileDetails(string cookieSavedToken);
    bool UpdateUserProfile(AddUserViewModel user, string Email);
    bool UserChangePassword(ChangePasswordViewModel changepassword, string Email);
    PaginationViewModel<User> GetUserList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5);
    List<Role> GetRole();
    Task<bool> AddUser(AddUserViewModel userVM, string Email);
    Task<bool> SendEmail(string Password, string Username, string Email);
    List<AddUserViewModel> GetUserByEmail(string email);
    Task<bool> EditUser(AddUserViewModel user, string Email);
    Task<bool> DeleteUser(string Email);
    Task<bool> IsUserNameExists(string Username);
    bool IsUserNameExistsForEdit(string Username, string Email);
    List<User> getUserFromEmail(string token);

}