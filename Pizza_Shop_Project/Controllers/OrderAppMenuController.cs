using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("AccountManager")]
public class OrderAppMenuController : Controller
{
    public IActionResult OrderAppMenu()
    {
        ViewData["orderApp-Active"] = "Menu";
        ViewData["Icon"] = "fa-burger";
        return View();
    }
}
