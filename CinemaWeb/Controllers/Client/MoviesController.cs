using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CinemaWeb.Controllers.Client
{
    public class MoviesController : Controller
    {
        private readonly CinemaDbContext _context;

        public MoviesController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string searchTerm, string sortOrder)
        {
            var today = DateTime.Now.Date;
            var nextWeek = today.AddDays(7);

            var filmsQuery = _context.Films
                .Include(f => f.Sessions)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filmsQuery = filmsQuery.Where(f => f.Name.Contains(searchTerm));
            }

            var allFilms = await filmsQuery.ToListAsync();

            var actualFilms = allFilms
                .Where(f => f.Sessions != null &&
                            f.Sessions.Any(s => s.StartTime.Date >= today && s.StartTime.Date <= nextWeek))
                .ToList();

            var actualIds = actualFilms.Select(a => a.Id).ToHashSet();


            var expectedFilms = allFilms
                .Where(f => !actualIds.Contains(f.Id))
                .ToList();

            switch (sortOrder)
            {
                case "name":
                    actualFilms = actualFilms.OrderBy(f => f.Name).ToList();
                    expectedFilms = expectedFilms.OrderBy(f => f.Name).ToList();
                    break;

                case "date_desc":
                    actualFilms = actualFilms.OrderByDescending(f => f.ReleaseDate).ToList();
                    expectedFilms = expectedFilms.OrderByDescending(f => f.ReleaseDate).ToList();
                    break;

                default:
                    actualFilms = actualFilms.OrderByDescending(f => f.ReleaseDate).ToList();
                    expectedFilms = expectedFilms.OrderByDescending(f => f.ReleaseDate).ToList();
                    break;
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = searchTerm;

            var viewModel = new ClientFilmViewModel
            {
                SearchTerm = searchTerm,
                ActualFilms = actualFilms,
                ExpectedFilms = expectedFilms
            };

            return View(viewModel);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var film = await _context.Films
                .Include(f => f.Producer)
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(f => f.FilmActors).ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCompanies).ThenInclude(fc => fc.Company)
                .FirstOrDefaultAsync(f => f.Id == id);

            var now = DateTime.Now;
            var nextWeek = now.AddDays(7);

            var sessions = await _context.Sessions
                .Include(s => s.Hall)
                .Where(s => s.FilmId == id && s.StartTime >= now && s.StartTime <= nextWeek)
                .OrderBy(s => s.StartTime)
                .Take(5)
                .Select(s => new ClientSessionDto
                {
                    SessionId = s.Id,
                    StartTime = s.StartTime,
                    HallName = s.Hall.Name,
                    BasePrice = s.BasePrice
                })
                .ToListAsync();

            if (film == null)
            {
                return NotFound();
            }


            var viewModel = new ClientFilmDetailsViewModel
            {
                Id = film.Id,
                Title = film.Name,
                Description = film.Description,
                DurationMinutes = film.DurationMinutes,
                AllowedMinAge = film.AllowedMinAge,
                PosterUrl = film.PosterUrl,
                TrailerUrl = film.TrailerUrl,
                ReleaseDate = film.ReleaseDate,
                ProducerName = film.Producer?.Name,
                Genres = film.FilmGenres.Select(fg => fg.Genre.Name).ToList(),
                Actors = film.FilmActors.Select(fa => fa.Actor.Name).ToList(),
                Companies = film.FilmCompanies.Select(fc => fc.Company.Name).ToList(),
                UpcomingSessions = sessions
            };

            return View(viewModel);
        }
    }

}
