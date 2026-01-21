using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers.Client
{
    public class SessionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
