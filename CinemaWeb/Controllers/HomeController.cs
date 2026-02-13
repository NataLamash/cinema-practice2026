using CinemaInfrastructure;
using CinemaWeb.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaDbContext _context;

        public HomeController(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayFilms = await _context.Films
                .AsNoTracking()
                .Where(f => f.Sessions.Any(s => s.StartTime >= today && s.StartTime < tomorrow))
                .OrderByDescending(f => f.ReleaseDate)
                .ThenByDescending(f => f.Id)
                .Take(1)
                .Select(f => new FilmCardVm
                {
                    Id = f.Id,
                    Name = f.Name,
                    PosterUrl = f.PosterUrl,
                    ReleaseDate = f.ReleaseDate
                })
                .ToListAsync();

            var actualFilms = await _context.Films
                .AsNoTracking()
                .OrderByDescending(f => f.ReleaseDate)
                .ThenByDescending(f => f.Id)
                .Take(12)
                .Select(f => new FilmCardVm
                {
                    Id = f.Id,
                    Name = f.Name,
                    PosterUrl = f.PosterUrl,
                    ReleaseDate = f.ReleaseDate
                })
                .ToListAsync();

            var vm = new HomeIndexVm
            {
                TodayFilms = todayFilms,
                ActualFilms = actualFilms
            };

            return View(vm);
        }


        public IActionResult Privacy()
        {
            return View();
        }
    }
}
