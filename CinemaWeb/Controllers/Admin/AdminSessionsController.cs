using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers.Admin
{
    public class AdminSessionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
