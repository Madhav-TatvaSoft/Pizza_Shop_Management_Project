using Microsoft.AspNetCore.Mvc;
using BLL.Implementation;
using Microsoft.AspNetCore.Authorization;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DAL.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


public class UserController : Controller
{
    private readonly UserService _userService;

private readonly UserLoginService _userLoginService;

    private readonly JWTService _JWTService;

    public UserController(UserService userService, JWTService JWTService,UserLoginService userLoginService)
    {
        this._userService = userService;
        this._JWTService = JWTService;
        this._userLoginService = userLoginService;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        return View();
    }

    // [Authorize(Roles = "Admin")]
    public IActionResult UserProfile()
    {
        var cookieSavedToken = Request.Cookies["AuthToken"];
        var data = _userService.GetUserProfileDetails(cookieSavedToken);
        var Countries = _userService.GetCountry();
        var States = _userService.GetState();
        var Cities = _userService.GetCity();
        ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
        ViewBag.States = new SelectList(States, "StateId", "StateName");
        ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
        // ViewBag.email = email;
        return View(data[0]);
    }

    [HttpPost]
    public IActionResult UserProfile(User user)
    {
        var token = Request.Cookies["AuthToken"];
        var userEmail = _JWTService.GetClaimValue(token, "email");
        _userService.UpdateUser(user, userEmail);
        return RedirectToAction("UserProfile", "User");
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(ChangePasswordViewModel changepassword)
    {
        var token = Request.Cookies["AuthToken"];
        var userEmail = _JWTService.GetClaimValue(token, "email");


        if (changepassword.CurrentPassword == changepassword.NewPassword)
        {
            ViewBag.Message = "Current Password and New Password cannot be same";
            return View();
        }
        else if (changepassword.NewPassword != changepassword.NewConfirmPassword)
        {
            ViewBag.Message = "New Password and Confirm Password should be same";
            return View();
        }
        else
        {
            changepassword.CurrentPassword = _userLoginService.EncryptPassword(changepassword.CurrentPassword);
            changepassword.NewPassword = _userLoginService.EncryptPassword(changepassword.NewPassword);
            var password_verify = _userService.UserChangePassword(changepassword, userEmail);
            if (password_verify)
            {
                ViewBag.Message = "Password Changed Successfully";
                return RedirectToAction("UserProfile", "User");
            }
            else
            {
                ViewBag.Message = "Current Password is incorrect";
                return View();
            }
        }
    }
    public IActionResult UserLogout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("email");
        return RedirectToAction("VerifyUserLogin", "UserLogin");
    }

    // public IActionResult EditProfile()
    // {
    //     var cookieSavedToken = Request.Cookies["AuthToken"];
    //     var data = _userService.GetUserProfileDetails(cookieSavedToken);
    //     return View(data[0]);
    // }


}


