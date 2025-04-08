using Microsoft.AspNetCore.Mvc;
using DAL.ViewModels;
using BLL.Interface;
using Microsoft.AspNetCore.Authorization;
using BLL.common;

namespace Pizza_Shop_Project.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly IUserLoginService _userLoginService;
        private readonly IJWTService _jwtService;

        #region UserLogin Constructor
        public UserLoginController(IUserLoginService userLoginService, IJWTService jwtService)
        {
            this._userLoginService = userLoginService;
            this._jwtService = jwtService;
        }
        #endregion

        #region VerifyUserLogin
        public IActionResult VerifyUserLogin()
        {

            if (Request.Cookies.ContainsKey("email"))
            {
                return RedirectToAction("Dashboard", "User");
            }
            // ViewData["RoleId"] = new SelectList(_userLoginService.Roles, "RoleId", "RoleId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> VerifyUserLogin(UserLoginViewModel userLogin)
        {

            string? verification_token = await _userLoginService.VerifyUserLogin(userLogin);

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddHours(30);

            if (verification_token != null)
            {
                //JWT token
                Response.Cookies.Append("AuthToken", verification_token, option);
                Response.Cookies.Append("profileImage", _userLoginService.GetProfileImage(userLogin.Email), option);
                Response.Cookies.Append("username", _userLoginService.GetUsername(userLogin.Email), option);

                if (userLogin.Remember_me)
                {
                    Response.Cookies.Append("email", userLogin.Email, option);
                }

                TempData["SuccessMessage"] = NotificationMessage.LoginSuccess;
                return RedirectToAction("Dashboard", "User");
            }

            TempData["ErrorMessage"] = NotificationMessage.InvalidCredentials;
            return RedirectToAction("VerifyUserLogin", "UserLogin");
        }

        #endregion

        #region GetEmail
        public string GetEmail(string Email)
        {
            ForgotPasswordViewModel forgotPasswordViewModel = new ForgotPasswordViewModel();
            forgotPasswordViewModel.Email = Email;
            TempData["Email"] = Email;
            return Email;
        }
        #endregion

        #region ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotpassword)
        {
            UserLoginViewModel? userLogin = new UserLoginViewModel();
            userLogin.Email = forgotpassword.Email;
            string? getpassword = _userLoginService.GetPassword(userLogin.Email);
            bool CheckEmailExists = _userLoginService.CheckEmailExist(userLogin.Email);
            if (ModelState.IsValid)
            {
                if (CheckEmailExists)
                {
                    string? resetLink = Url.Action("ResetPassword", "UserLogin", new { reset_token = _jwtService.GenerateResetToken(userLogin.Email, getpassword) }, Request.Scheme);
                    bool sendEmail = await _userLoginService.SendEmail(forgotpassword, resetLink);
                    if (sendEmail)
                    {
                        TempData["SuccessMessage"] = NotificationMessage.EmailSentSuccessfully;
                        return View("VerifyUserLogin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = NotificationMessage.EmailSendingFailed;
                        return View("ForgotPassword");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = NotificationMessage.DoesNotExists.Replace("{0}", "Email");
                    return View("ForgotPassword");
                }
            }
            return View();
        }
        #endregion

        #region ResetPassword
        public IActionResult ResetPassword(string reset_token)
        {
            // var resetPassword = new ResetPasswordViewModel();
            // resetPassword.Email = _userLoginService.Base64Decode(Email);

            string? reset_email = _jwtService.GetClaimValue(reset_token, "email");
            string? reset_password = _jwtService.GetClaimValue(reset_token, "password");
            string? Db_Password = _userLoginService.GetPassword(reset_email);

            if (Db_Password == reset_password)
            {
                ResetPasswordViewModel resetPassData = new ResetPasswordViewModel();
                resetPassData.Email = _jwtService.GetClaimValue(reset_token, "email");
                return View(resetPassData);
            }
            TempData["ErrorMessage"] = NotificationMessage.ResetPasswordChangedError;
            return RedirectToAction("VerifyUserLogin", "UserLogin");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                bool IsEmailExistsStatus = _userLoginService.CheckEmailExist(resetPassword.Email);

                if (!IsEmailExistsStatus)
                {
                    TempData["ErrorMessage"] = NotificationMessage.DoesNotExists.Replace("{0}", "Email");
                    return View("ResetPassword");
                }

                if (resetPassword.Password == resetPassword.ConfirmPassword)
                {
                    bool checkresetpassword = await _userLoginService.ResetPassword(resetPassword);
                    if (checkresetpassword)
                    {
                        TempData["SuccessMessage"] = NotificationMessage.PasswordChanged;
                        return RedirectToAction("VerifyUserLogin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = NotificationMessage.PasswordChangeFailed;
                        return View("ResetPassword");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = NotificationMessage.PasswordCheck;
                    return View("ResetPassword");
                }
            }
            return View("ResetPassword");
        }
        #endregion

    }
}