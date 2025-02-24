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

    public UserController(UserService userService, JWTService JWTService, UserLoginService userLoginService)
    {
        this._userService = userService;
        this._JWTService = JWTService;
        this._userLoginService = userLoginService;
    }

    // [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        return View();
    }

    public JsonResult GetStates(long? countryId)
    {
        var states = _userService.GetState(countryId);
        return Json(new SelectList(states, "StateId", "StateName"));
    }

    public JsonResult GetCities(long? stateId)
    {
        var cities = _userService.GetCity(stateId);
        return Json(new SelectList(cities, "CityId", "CityName"));
    }

    // [Authorize(Roles = "Admin")]
    public IActionResult UserProfile()
    {
        var cookieSavedToken = Request.Cookies["AuthToken"];
        var data = _userService.GetUserProfileDetails(cookieSavedToken);
        var Countries = _userService.GetCountry();
        var States = _userService.GetState(data[0].CountryId);
        var Cities = _userService.GetCity(data[0].StateId);
        ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
        ViewBag.States = new SelectList(States, "StateId", "StateName");
        ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");
        return View(data[0]);
    }

    [HttpPost]
    public IActionResult UserProfile(User user)
    {
        var token = Request.Cookies["AuthToken"];
        var userEmail = _JWTService.GetClaimValue(token, "email");

        if (user.CountryId == null)
        {
            TempData["CountryError"] = "Please select a country";
        }
        if (user.StateId == null)
        {
            TempData["StateError"] = "Please select a state";
        }
        if (user.CityId == null)
        {
            TempData["CityError"] = "Please select a city";
        }

        _userService.UpdateUser(user, userEmail);
        return RedirectToAction("UserListData", "User");
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

    // public IActionResult UserListData()
    // {
    //     var cookieSavedToken = Request.Cookies["AuthToken"];
    //     var data = _userService.GetUserList(cookieSavedToken);
    //     return View(data);
    // }

    public IActionResult UserListData()
    {
        var token = Request.Cookies["AuthToken"];
        var Email = _JWTService.GetClaimValue(token, "email");
        var users = _userService.GetUserList(Email);
        return View(users);
    }

    public IActionResult AddUser()
    {
        var Roles = _userService.GetRole();
        var Countries = _userService.GetCountry();
        var States = _userService.GetState(-1);
        var Cities = _userService.GetCity(-1);
        ViewBag.Roles = new SelectList(Roles, "RoleId", "RoleName");
        ViewBag.Countries = new SelectList(Countries, "CountryId", "CountryName");
        ViewBag.States = new SelectList(States, "StateId", "StateName");
        ViewBag.Cities = new SelectList(Cities, "CityId", "CityName");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(AddUserViewModel user)
    {
        if (user.StateId == -1 && user.CityId == -1)
        {
            TempData["stateErrorMessage"] = "Please select a state";
            TempData["cityErrorMessage"] = "Please select a city";
            return RedirectToAction("AddUser", "User");
        }
        if (user.StateId == -1)
        {
            TempData["stateErrorMessage"] = "Please select a state";
            return RedirectToAction("AddUser", "User");
        }
        if (user.CityId == -1)
        {
            TempData["cityErrorMessage"] = "Please select a city";
            return RedirectToAction("AddUser", "User");
        }

        var token = Request.Cookies["AuthToken"];
        var Email = _JWTService.GetClaimValue(token, "email");

        if (!await _userService.AddUser(user, Email))
        {
            ViewBag.Message = "Email already exists";
            return View();
        }
        // _userService.AddUser(user, Email);
        return RedirectToAction("UsersList", "User");
        // return View();
    }

    public IActionResult DeleteUser(int id)
    {
        _userService.DeleteUser(id);
        return RedirectToAction("UserListData", "User");
    }

}