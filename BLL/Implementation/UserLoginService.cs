using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DAL.Models;
using DAL.ViewModels;
using Microsoft.EntityFrameworkCore;
using BLL.Interface;

namespace BLL.Implementation;

public class UserLoginService : IUserLoginService
{
    private readonly PizzaShopDbContext _context;
    private readonly IJWTService _jwtService;

    #region User Login Constructor
    public UserLoginService(PizzaShopDbContext context, IJWTService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
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
                string body = $@"
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
                SmtpClient smtp = new SmtpClient
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
        if (_context.UserLogins.FirstOrDefault(e => e.Email == email && e.Isdelete == false) != null)
        {
            return true;
        }
        return false;
    }
    #endregion

}