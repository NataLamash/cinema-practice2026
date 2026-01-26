using Microsoft.AspNetCore.Mvc;
using CinemaWeb.ViewModels;
using CinemaDomain.Model;
using CinemaInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaWeb.Controllers.Client
{
    public class SessionsController : Controller
    {
        private readonly CinemaDbContext _context;

        public SessionsController(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? filterDate, int? filmId)
        {
            var now = DateTime.Now;
            var nextWeek = now.AddDays(7);

            var query = _context.Sessions
            .Include(s => s.Film)
            .Include(s => s.Hall)
            .Where(s => s.StartTime >= now && s.StartTime <= nextWeek);

            if (filterDate.HasValue)
            {
                query = query.Where(s => s.StartTime.Date == filterDate.Value.Date);
            }

            if (filmId.HasValue)
            {
                query = query.Where(s => s.FilmId == filmId);
            }

            var filmList = await _context.Sessions
            .Where(s => s.StartTime > DateTime.Now)
            .Select(s => s.Film)
            .Distinct()
            .OrderBy(f => f.Name)
            .ToListAsync();

            ViewBag.FilmList = new SelectList(filmList, "Id", "Name", filmId);

            var sessions = await query.OrderBy(s => s.StartTime).ToListAsync();

            var upcomingSessions = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new ClientSessionViewModel
                {
                    Id = s.Id,
                    FilmName = s.Film.Name, 
                    HallName = s.Hall.Name, 
                    StartTime = s.StartTime,
                    BasePrice = s.BasePrice
                })
            .ToListAsync();

            ViewBag.CurrentDate = filterDate;
            ViewBag.CurrentFilmId = filmId;

            return View(upcomingSessions);
        }
    }
}
