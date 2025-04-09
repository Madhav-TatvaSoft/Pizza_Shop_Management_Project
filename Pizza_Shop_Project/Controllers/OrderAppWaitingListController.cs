using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;

[PermissionAuthorize("AccountManager")]
public class OrderAppWaitingListController : Controller
{
    public IActionResult OrderAppWaitingList()
    {
        ViewData["orderApp-Active"] = "WaitingList";
        ViewData["Icon"] = "fa-clock";
        return View();
    }
}
