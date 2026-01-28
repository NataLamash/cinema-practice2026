using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var today = DateTime.Now;
            var nextWeek = today.AddDays(7);

            var filmsQuery = _context.Films
                .Include(f => f.Sessions)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filmsQuery = filmsQuery.Where(f => f.Name.Contains(searchTerm));
            }

            var allFilms = await filmsQuery.ToListAsync();

            var actualFilmsQuery = allFilms
                .Where(f => f.Sessions != null &&
                            f.Sessions.Any(s => s.StartTime >= today && s.StartTime <= nextWeek));

            var expectedFilmsQuery = allFilms
                .Where(f => f.ReleaseDate > today || f.Sessions == null || !f.Sessions.Any())
                .Except(actualFilmsQuery);

            switch (sortOrder)
            {
                case "name":
                    actualFilmsQuery = actualFilmsQuery.OrderBy(f => f.Name);
                    expectedFilmsQuery = expectedFilmsQuery.OrderBy(f => f.Name);
                    break;

                case "date_desc":
                    actualFilmsQuery = actualFilmsQuery.OrderByDescending(f => f.ReleaseDate);
                    expectedFilmsQuery = expectedFilmsQuery.OrderByDescending(f => f.ReleaseDate);
                    break;

                default:
                    actualFilmsQuery = actualFilmsQuery.OrderByDescending(f => f.ReleaseDate);
                    expectedFilmsQuery = expectedFilmsQuery.OrderByDescending(f => f.ReleaseDate);
                    break;
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = searchTerm;

            var viewModel = new ClientFilmViewModel
            {
                SearchTerm = searchTerm,
                ActualFilms = actualFilmsQuery.ToList(),
                ExpectedFilms = expectedFilmsQuery.ToList()
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
                .FirstOrDefaultAsync(f => f.Id == id);

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
                ProducerName = film.Producer?.Name ?? "Невідомий",
                Genres = film.FilmGenres.Select(fg => fg.Genre.Name).ToList(),
                Actors = film.FilmActors.Select(fa => fa.Actor.Name).ToList()
            };

            return View(viewModel);
        }
    }

}

