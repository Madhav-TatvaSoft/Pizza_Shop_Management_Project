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

    public UserLoginService(PizzaShopDbContext context, IJWTService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;

    }
    

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

    // Get List of Loginned User With Role
    public async Task<List<UserLogin>> GetUserLogins()
    {
        return await _context.UserLogins.Include(u => u.Role).ToListAsync();
    }

    //Used to check the credentials of user
    public async Task<string> VerifyUserLogin(UserLoginViewModel userLogin)
    {
        // var user = _context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email && e.Password == EncryptPassword(userLogin.Password));
        UserLogin? user = await _context.UserLogins.FirstOrDefaultAsync(e => e.Email == userLogin.Email);

        if (user != null && user.Isdelete == false)
        {
            if (user.Password == EncryptPassword(userLogin.Password))
            {
                Role? roleObj = _context.Roles.SingleOrDefault(e => e.RoleId == user.RoleId);
                string token = _jwtService.GenerateToken(userLogin.Email, roleObj.RoleName);
                return token;
            }
            return null;
        }
        return null;
    }

    //  Used to Send Email
    public async Task<bool> SendEmail(ForgotPasswordViewModel forgotpassword, string resetLink)
    {
        string email = forgotpassword.Email;
        if (email != null)
        {
            try
            {
                MailAddress senderEmail = new MailAddress("tatvasoft.pca155@outlook.com", "sender");
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
            catch (Exception exp)
            {
                return false;
            }

            return true;
        }
        return false;
    }

    // Used to find If email exists and will update the encrypted password in the DB. 
    public async Task<bool> ResetPassword(ResetPasswordViewModel resetPassword)
    {
        UserLogin? data = _context.UserLogins.FirstOrDefault(e => e.Email == resetPassword.Email && !e.Isdelete);
        if (data != null && !data.Isdelete)
        {
            // Check if the new password is the same as the old password
            if (data.Password == EncryptPassword(resetPassword.Password))
            {
                return false;
            }
            data.Password = EncryptPassword(resetPassword.Password);
            _context.Update(data);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public string GetProfileImage(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).ProfileImage;
    }
    
    public string GetUsername(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).Username;
    }
   
    public long GetUserId(string Email)
    {
        return _context.Users.FirstOrDefault(x => x.Userlogin.Email == Email).UserId;
    }
   
    public string GetPassword(string Email)
    {
        return _context.UserLogins.FirstOrDefault(x => x.Email == Email).Password;
    }
    
    public bool CheckEmailExist(string email)
    {
        return _context.UserLogins.Any(e => e.Email == email && !e.Isdelete);
    }

}