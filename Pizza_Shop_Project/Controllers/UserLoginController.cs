using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL.Models;
using BLL_Business_Logic_Layer_;
using DAL.ViewModels;

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

        // GET: UserLogin/Details/5
        // public async Task<IActionResult> Details(long? id)
        // {
        //     if (id == null || _context.UserLogins == null)
        //     {
        //         return NotFound();
        //     }

        //     var userLogin = await _context.UserLogins
        //         .Include(u => u.Role)
        //         .FirstOrDefaultAsync(m => m.UserloginId == id);
        //     if (userLogin == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(userLogin);
        // }

        // GET: UserLogin/Create
        public IActionResult VerifyUserLogin()
        {
            if (Request.Cookies.ContainsKey("email"))
            {
                return RedirectToAction("Index", "UserLogin");
            }
            // ViewData["RoleId"] = new SelectList(_userLoginService.Roles, "RoleId", "RoleId");
            return View();
        }

        // POST: UserLogin/Create

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> VerifyUserLogin(UserLoginViewModel userLogin)
        {


            var verification = await _userLoginService.VerifyUserLogin(userLogin);
            if (verification)
            {
                if (userLogin.Remember_me)
                {
                    CookieOptions option = new CookieOptions();
                    option.Expires = DateTime.Now.AddMinutes(1);
                    Response.Cookies.Append("email", userLogin.Email, option);
                }
                return RedirectToAction("Index", "UserLogin");
            }
            ViewBag.message = "Please enter valid credentials";
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public String GetEmail(String Email)
        {
            TempData["email"] = Email;
            return Email;
        }
    }
}
