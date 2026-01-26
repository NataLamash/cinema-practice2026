using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers.Client
{
    public class MoviesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details()
        {
            return View();
        }
    }
}
