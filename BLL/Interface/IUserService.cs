using DAL.Models;
using DAL.ViewModels;
// using Implementation.UserService; // Add the namespace where 'PaginatedList<>' is defined

namespace BLL.Interface;

public interface IUserService
{
    public List<Country> GetCountry();
    public List<State> GetState(long? countryId);
    public List<City> GetCity(long? stateId);
    public List<AddUserViewModel> GetUserProfileDetails(string cookieSavedToken);
    public bool UpdateUser(AddUserViewModel user, string Email);
    public bool UserChangePassword(ChangePasswordViewModel changepassword, string Email);

    // public Task<PaginatedList<User>> GetUsersAsync(int page, int pageSize, string search); // Remove the 'Implementation.UserService.' prefix

    public List<AddUserViewModel> GetUserByEmail(string email);

    public bool EditUser(AddUserViewModel user, string Email);

    public Task<bool> DeleteUser(string Email);
    public Task<bool> AddUser(AddUserViewModel userVM, String Email);

    // public  Task<(List<User>, int)> GetUserList(string search, int PageNo, int PageSize);
    public List<User> GetUserList(string searchTerm, int pageNumber, int pageSize, out int totalRecords);

}
