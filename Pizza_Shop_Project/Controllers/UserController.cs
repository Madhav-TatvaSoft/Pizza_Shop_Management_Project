using Microsoft.AspNetCore.Mvc;
using BLL.Implementation;
using Microsoft.AspNetCore.Authorization;


public class UserController : Controller
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        this._userService = userService;
    }
    public async Task<IActionResult> UserProfile()
    {
        var cookieSavedToken = Request.Cookies["AuthToken"];
        var data = _userService.GetUserProfileDetails(cookieSavedToken);
        // ViewBag.email = email;
        return View(data[0]);
    }

    public IActionResult UserLogout()
    {
        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("email");
        return RedirectToAction("VerifyUserLogin", "UserLogin");
    }

    public IActionResult EditProfile()
    {
        var cookieSavedToken = Request.Cookies["AuthToken"];
        var data = _userService.GetUserProfileDetails(cookieSavedToken);
        return View(data[0]);
    }
    
    // [Authorize (Roles = "Admin")]
    // public IActionResult Dashboard()
    // {
    //     return View();
    // }
}