using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using BLL.Interface;
using BLL.common;
using Microsoft.Extensions.Configuration;

namespace BLL.Implementation;

public class UserLoginService : IUserLoginService
{
    private readonly PizzaShopDbContext _context;
    private readonly IJWTService _jwtService;
    private readonly IConfiguration _configuration;


    #region User Login Constructor
    public UserLoginService(PizzaShopDbContext context, IJWTService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;

    }
    #endregion

    #region Encrypt Password 
    public string EncryptPassword(string password)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: new byte[0],
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return hashed;
    }
    #endregion

    #region GetUserLogins 
    // Get List of Loginned User With Role
    public async Task<List<UserLogin>> GetUserLogins()
    {
        return await _context.UserLogins.Include(u => u.Role).ToListAsync();
    }
    #endregion

    #region VerifyUserLogin
    //Used to check the credentials of user
    public async Task<string> VerifyUserLogin(UserLoginViewModel userLogin)
    {
        // var user = _context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email && e.Password == EncryptPassword(userLogin.Password));
        UserLogin user = _context.UserLogins.Where(e => e.Email == userLogin.Email).FirstOrDefault();

        if (user != null && user.Isdelete == false)
        {
            if (user.Password == EncryptPassword(userLogin.Password))
            {
                Role roleObj = _context.Roles.FirstOrDefault(e => e.RoleId == user.RoleId);
                string token = _jwtService.GenerateToken(userLogin.Email, roleObj.RoleName);
                return token;
            }
            return null;
        }
        return null;
    }
    #endregion

    #region SendEmail
    //  Used to Send Email
    public async Task<bool> SendEmail(ForgotPasswordViewModel forgotpassword, string resetLink)
    {
        string user = forgotpassword.Email;
        if (user != null)
        {
            try
            {
                MailAddress senderEmail = new MailAddress("tatvasoft.pca106@outlook.com", "sender");
                MailAddress receiverEmail = new MailAddress(forgotpassword.Email, "reciever");
                string password = "P}N^{z-]7Ilp";
                string sub = "Forgot Password";
                string body = EmailTemplate.ResetPasswordEmail(resetLink);
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
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
        return false;
    }
    #endregion

    #region Reset Password
    // Used to If email exists and will update the encrypted password in the DB. 
    public async Task<bool> ResetPassword(ResetPasswordViewModel resetPassword)
    {
        UserLogin data = _context.UserLogins.FirstOrDefault(e => e.Email == resetPassword.Email && e.Isdelete == false);
        if (data != null && data.Isdelete == false)
        {
            data.Password = EncryptPassword(resetPassword.Password);
            _context.Update(data);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    #endregion

    #region Get Profile Image
    public string GetProfileImage(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).ProfileImage;
    }
    #endregion

    #region Get Username
    public string GetUsername(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).Username;
    }
    #endregion

    #region Get UserId
    public long GetUserId(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).UserId;
    }
    #endregion

    #region Get Password
    public string GetPassword(string Email)
    {
        return _context.UserLogins.FirstOrDefault(x => x.Email == Email).Password;
    }
    #endregion

    #region Check Email Exists
    public bool CheckEmailExist(string email)
    {
        return _context.UserLogins.Any(e => e.Email == email && !e.Isdelete);
    }
    #endregion

}