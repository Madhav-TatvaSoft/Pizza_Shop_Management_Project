using Microsoft.AspNetCore.Mvc;
using DAL.ViewModels;
using BLL.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Pizza_Shop_Project.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly IUserLoginService _userLoginService;
        private readonly IJWTService _jwtService;
        public UserLoginController(IUserLoginService userLoginService, IJWTService jwtService)
        {
            this._userLoginService = userLoginService;
            this._jwtService = jwtService;
        }

        #region VerifyUserLogin
        public IActionResult VerifyUserLogin()
        {

            if (Request.Cookies.ContainsKey("email"))
            {
                // TempData["SuccessMessage"] = "Login Successfully";
                return RedirectToAction("Dashboard", "User");
            }
            // ViewData["RoleId"] = new SelectList(_userLoginService.Roles, "RoleId", "RoleId");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> VerifyUserLogin(UserLoginViewModel userLogin)
        {

            var verification_token = await _userLoginService.VerifyUserLogin(userLogin);

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddMinutes(10);

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
                TempData["SuccessMessage"] = "Login Successfully";
                return RedirectToAction("Dashboard", "User");
            }
            TempData["ErrorMessage"] = "Please enter valid credentials";
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
            var userLogin = new UserLoginViewModel();
            userLogin.Email = forgotpassword.Email;
            var getpassword = _userLoginService.GetPassword(userLogin.Email);
            var isSendEmail = await _userLoginService.IsSendEmail(userLogin);
            if (ModelState.IsValid)
            {
                if (isSendEmail)
                {
                    var resetLink = Url.Action("ResetPassword", "UserLogin", new { reset_token = _jwtService.GenerateResetToken(userLogin.Email, getpassword) }, Request.Scheme);
                    var sendEmail = await _userLoginService.SendEmail(forgotpassword, resetLink);
                    if (sendEmail)
                    {
                        TempData["SuccessMessage"] = "Reset password link sent successfully";
                        return View("VerifyUserLogin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Email Server Error.Please try again!";
                        return View("ForgotPassword");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Email does not exists";
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

            var reset_email = _jwtService.GetClaimValue(reset_token, "email");
            var reset_password = _jwtService.GetClaimValue(reset_token, "password");
            var Db_Password = _userLoginService.GetPassword(reset_email);

            if (Db_Password == reset_password)
            {
                ResetPasswordViewModel resetPassData = new ResetPasswordViewModel();
                resetPassData.Email = _jwtService.GetClaimValue(reset_token, "email");
                return View(resetPassData);
            }
            TempData["ErrorMessage"] = "You have already changed the Password once";
            return RedirectToAction("VerifyUserLogin", "UserLogin");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var IsEmailExistsStatus = _userLoginService.CheckEmailExist(resetPassword.Email);

                if (!IsEmailExistsStatus)
                {
                    TempData["ErrorMessage"] = "Email does not exist. Enter existing email to set password.";
                    return View("ResetPassword");
                }

                if (resetPassword.Password == resetPassword.ConfirmPassword)
                {
                    var checkresetpassword = await _userLoginService.ResetPassword(resetPassword);
                    if (checkresetpassword)
                    {
                        TempData["SuccessMessage"] = "Password Reset Successfully";
                        return RedirectToAction("VerifyUserLogin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "";
                        return View("ResetPassword");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Password and Confirm Password should be same";
                    return View("ResetPassword");
                }
            }
            return View("ResetPassword");
        }
        #endregion

    }
}