using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserService
{
    public List<Country> GetCountry();
    public List<State> GetState(long? countryId);
    public List<City> GetCity(long? stateId);
    public List<AddUserViewModel> GetUserProfileDetails(string cookieSavedToken);
    public bool UpdateUser(AddUserViewModel user, string Email);
    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email);
    public PaginationViewModel<User> GetUserList(string search = "", string sortColumn = "", string sortDirection = "", int pageNumber = 1, int pageSize = 5);
    public List<Role> GetRole();
    public Task<bool> AddUser(AddUserViewModel userVM, String Email);
    public List<AddUserViewModel> GetUserByEmail(string email);
    public bool EditUser(AddUserViewModel user, string Email);
    public Task<bool> DeleteUser(string Email);
    public Task<bool> IsUserNameExists(string Username);
    public bool IsUserNameExistsForEdit(string Username, string Email);

}