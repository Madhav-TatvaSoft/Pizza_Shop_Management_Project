using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DAL.Models;
using DAL.ViewModels;
using BLL.Implementation;
using Microsoft.AspNetCore.Http;
using Azure;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using BLL.Interface;

namespace BLL.Implementation;

public class UserLoginService : IUserLoginService
{
    private readonly PizzaShopDbContext _context;
    private readonly IJWTService _jwtService;


    public UserLoginService(PizzaShopDbContext context, IJWTService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

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

    public async Task<List<UserLogin>> GetUserLogins()
    {
        var pizzaShopDbContext = _context.UserLogins.Include(u => u.Role);
        return await pizzaShopDbContext.ToListAsync();
    }

    //Used to check the credentials of user
    public async Task<string> VerifyUserLogin(UserLoginViewModel userLogin)
    {
        // var user = _context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email && e.Password == EncryptPassword(userLogin.Password));
        var user = _context.UserLogins.Where(e => e.Email == userLogin.Email).FirstOrDefault();

        if (user != null && user.Isdelete == false)
        {
            if (user.Password == EncryptPassword(userLogin.Password))
            {
                var roleObj = _context.Roles.FirstOrDefault(e => e.RoleId == user.RoleId);
                var token = _jwtService.GenerateToken(userLogin.Email, roleObj.RoleName);
                return token;
            }
            return null;
        }
        return null;
    }

    public async Task<bool> IsSendEmail(UserLoginViewModel userLogin)
    {
        var user = _context.UserLogins.FirstOrDefault(e => e.Email == userLogin.Email);
        if (user != null)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> SendEmail(ForgotPasswordViewModel forgotpassword, string resetLink)
    {
        var user = forgotpassword.Email;
        if (user != null)
        {
            try
            {
                var senderEmail = new MailAddress("tatva.pca42@outlook.com", "sender");
                var receiverEmail = new MailAddress(forgotpassword.Email, "reciever");
                var password = "P}N^{z-]7Ilp";
                var sub = "Forgot Password";
                var body = $@"
                <div style='max-width: 500px; font-family: Arial, sans-serif; border: 1px solid #ddd;'>
                <div style='background: #006CAC; padding: 10px; text-align: center; height:90px; max-width:100%; display: flex; justify-content: center; align-items: center;'>
                    <img src='https://images.vexels.com/media/users/3/128437/isolated/preview/2dd809b7c15968cb7cc577b2cb49c84f-pizza-food-restaurant-logo.png' style='max-width: 50px;' />
                    <span style='color: #fff; font-size: 24px; margin-left: 10px; font-weight: 600;'>PIZZASHOP</span>
                </div>
                <div style='padding: 20px 5px; background-color: #e8e8e8;'>
                    <p>Pizza shop,</p>
                    <p>Please click <a href='{resetLink}' style='color: #1a73e8; text-decoration: none; font-weight: bold;'>here</a>
                        to reset your account password.</p>
                    <p>If you encounter any issues or have any questions, please do not hesitate to contact our support team.</p>
                    <p><strong style='color: orange;'>Important Note:</strong> For security reasons, the link will expire in 24 hours.
                        If you did not request a password reset, please ignore this email or contact our support team immediately.
                    </p>
                </div>
                </div>";
                var smtp = new SmtpClient
                {
                    Host = "mail.etatvasoft.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
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

    public async Task<bool> ResetPassword(ResetPasswordViewModel resetPassword)
    {
        var data = _context.UserLogins.FirstOrDefault(e => e.Email == resetPassword.Email);
        if (data != null)
        {
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

    public string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public string GetPassword(string Email){
        return _context.UserLogins.FirstOrDefault(x => x.Email == Email).Password;
    }
}