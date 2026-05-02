using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}