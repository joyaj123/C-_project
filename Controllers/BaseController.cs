using Microsoft.AspNetCore.Mvc;

namespace BusBookingSystem.Controllers
{
    public class BaseController : Controller ///IMPORTANT 
    {
        protected bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        protected int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}