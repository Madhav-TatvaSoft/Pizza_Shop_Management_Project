using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizza_Shop_Project.Authorization;

namespace Pizza_Shop_Project.Controllers;


[PermissionAuthorize("AccountManager")]
public class OrderAppTableController : Controller
{
    public IActionResult OrderAppTable()
    {
        ViewData["orderApp-Active"] = "Table";
        ViewData["Icon"] = "fa-table";
        return View();
    }
}
