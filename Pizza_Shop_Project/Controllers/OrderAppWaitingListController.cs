using Microsoft.AspNetCore.Mvc;

namespace Pizza_Shop_Project.Controllers;

public class OrderAppWaitingListController : Controller
{
    public IActionResult OrderAppWaitingList()
    {
        ViewData["orderApp-Active"] = "WaitingList";
        return View();
    }
}
