using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class OrderAppController : Controller
{
    public IActionResult OrderApp()
    {
        ViewData["orderApp-Active"] = "Table";
        return View();
    }
}