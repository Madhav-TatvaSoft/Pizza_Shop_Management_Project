using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserLoginService
{
    string EncryptPassword(string password);
    Task<List<UserLogin>> GetUserLogins();
    Task<string> VerifyUserLogin(UserLoginViewModel userLogin);
    Task<bool> SendEmail(ForgotPasswordViewModel forgotpassword, string resetLink);
    Task<bool> ResetPassword(ResetPasswordViewModel resetPassword);
    string GetProfileImage(string Email);
    string GetUsername(string Email);
    long GetUserId(string Email);
    string GetPassword(string Email);
    bool CheckEmailExist(string email);
}