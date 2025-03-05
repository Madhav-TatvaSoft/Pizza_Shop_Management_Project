using DAL.Models;
using DAL.ViewModels;

namespace BLL.Interface;

public interface IUserLoginService
{
    public string EncryptPassword(string password);
    public Task<List<UserLogin>> GetUserLogins();
    public Task<string> VerifyUserLogin(UserLoginViewModel userLogin);
    public Task<bool> IsSendEmail(UserLoginViewModel userLogin);
    public Task<bool> SendEmail(ForgotPasswordViewModel forgotpassword, string resetLink);
    public Task<bool> ResetPassword(ResetPasswordViewModel resetPassword);
    public string GetProfileImage(string Email);
    public string GetUsername(string Email);
    public long GetUserId(string Email);
}