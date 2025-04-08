using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class OrderAppMenuController : Controller
{
    public IActionResult OrderAppMenu()
    {
        ViewData["orderApp-Active"] = "Menu";
        return View();
    }
}
