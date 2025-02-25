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

        // GET: UserLogin

        // public async Task<IActionResult> Index()
        // {
        //     var userlogindata = await _userLoginService.GetUserLogins();
        //     return View(userlogindata);
        // }

        // GET: UserLogin/Verify
        public IActionResult VerifyUserLogin()
        {

            if (Request.Cookies.ContainsKey("email"))
            {
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
            ViewBag.message = "Please enter valid credentials";
            return View("VerifyUserLogin");
        }

        public IActionResult ForgotPassword(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }

        // public IActionResult ForgotPassword(string email)
        // {
        //     // Your logic to handle the forgot password request
        //     if (email == null)
        //     {
        //         return BadRequest("Email is required.");
        //     }

        //     // Process the email (e.g., send a password reset link)
        //     // ...

        //     return View();
        // }

        public IActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(ForgotPasswordViewModel forgotpassword)
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
                    return View("VerifyUserLogin");
                }
                else
                {
                    ViewBag.message = "Please try again!!!";
                    return View("ForgotPassword");
                }
            }
            else
            {
                ViewBag.message = "Please enter valid email";
                return View("ForgotPassword");
            }
        }

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
                        return RedirectToAction("VerifyUserLogin");
                    }
                    else
                    {
                        ViewBag.message = "Please try again!!!";
                        return View("ResetPassword");
                    }
                }
                else
                {
                    ViewBag.message = "Password and Confirm Password should be same";
                    return View("ResetPassword");
                }
            }
            return View("ResetPassword");

        }
    }
}
