using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers.Admin
{
    public class AdminFilmsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
