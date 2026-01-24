using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaInfrastructure;
using CinemaDomain.Model; 


namespace CinemaWeb.Controllers.Admin
{
    public class AdminFilmsController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminFilmsController(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Films.ToListAsync());
        }
    }
}
