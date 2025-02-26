using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Models;
using DAL.ViewModels;
using System.Net.Mail;
using System.Net;
using BLL.Implementation;
using Microsoft.AspNetCore.Authorization;


namespace Pizza_Shop_Project.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly UserLoginService _userLoginService;


        public UserLoginController(UserLoginService userLoginService)
        {
            this._userLoginService = userLoginService;
        }

        #region VerifyUserLogin
        public IActionResult VerifyUserLogin()
        {

            if (Request.Cookies.ContainsKey("email"))
            {
                TempData["SuccessMessage"] = "Login Successfully";
                return RedirectToAction("UserListData", "User");
            }
            // ViewData["RoleId"] = new SelectList(_userLoginService.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: UserLogin/Create

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> VerifyUserLogin(UserLoginViewModel userLogin)
        {

            var verification_token = await _userLoginService.VerifyUserLogin(userLogin);

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddHours(10);

            if (verification_token != null)
            {
                //JWT token
                Response.Cookies.Append("AuthToken", verification_token, option);

                if (userLogin.Remember_me)
                {
                    Response.Cookies.Append("email", userLogin.Email, option);
                    TempData["SuccessMessage"] = "Login Successfully";
                    return RedirectToAction("UserListData", "User");

                }
                HttpContext.Session.SetString("email", userLogin.Email);
                TempData["SuccessMessage"] = "Login Successfully";
                return RedirectToAction("UserListData", "User");
            }
            TempData["ErrorMessage"] = "Please enter valid credentials";
            return RedirectToAction("VerifyUserLogin","UserLogin");
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
            var isSendEmail = await _userLoginService.IsSendEmail(userLogin);
            if (isSendEmail)
            {
                var resetLink = Url.Action("ResetPassword", "UserLogin", new { email = forgotpassword.Email }, Request.Scheme);
                var sendEmail = await _userLoginService.SendEmail(forgotpassword, resetLink);
                if (sendEmail)
                {
                    TempData["SuccessMessage"] = "Reset password link sent successfully";
                    return View("VerifyUserLogin");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please try again!!!";
                    return View("ForgotPassword");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Email does not exists";
                return View("ForgotPassword");
            }
        }
        #endregion

        #region ResetPassword
        public IActionResult ResetPassword(string Email)
        {
            var resetPassword = new ResetPasswordViewModel();
            resetPassword.Email = Email;
            return View("ResetPassword");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
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
                        TempData["ErrorMessage"] = "Please try again!!!";
                        return View("ResetPassword");
                    }
                }
                else
                {
                    ViewBag.reset = "Password and Confirm Password should be same";
                    return View("ResetPassword");
                }
            }
            return View("ResetPassword");
        }
    #endregion

    }
}