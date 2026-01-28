using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaWeb.Controllers.Admin
{
    public class AdminFilmsController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminFilmsController(CinemaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var films = await _context.Films
                .Include(f => f.Producer)
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .OrderByDescending(f => f.Id)
                .ToListAsync();
            return View(films);
        }

        // GET: AdminFilms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Producer)
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(f => f.FilmActors).ThenInclude(fa => fa.Actor)
                .Include(f => f.FilmCompanies).ThenInclude(fc => fc.Company)
                .Select(f => new AdminFilmDetailsViewModel
                {
                    Id = f.Id,
                    Title = f.Name,
                    Description = f.Description,
                    DurationMinutes = f.DurationMinutes,
                    AllowedMinAge = f.AllowedMinAge,
                    PosterUrl = f.PosterUrl,
                    TrailerUrl = f.TrailerUrl,
                    ReleaseDate = f.ReleaseDate,
                    ProducerName = f.Producer.Name,
                    Genres = f.FilmGenres.Select(g => g.Genre.Name).ToList(),
                    Actors = f.FilmActors.Select(a => a.Actor.Name).ToList(),
                    Companies = f.FilmCompanies.Select(c => c.Company.Name).ToList()
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new AdminFilmFormViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminFilmFormViewModel model)
        {
            if (CheckNameDuplication(model.Name))
            {
                ModelState.AddModelError("Name", "Фільм з такою назвою вже існує!");
            }

            if (ModelState.IsValid)
            {
                string? posterPath = null;

                if (model.PosterFile != null)
                {
                    posterPath = await SaveImageAsync(model.PosterFile);
                }

                var film = new Film
                {
                    Name = model.Name,
                    Description = model.Description,
                    ReleaseDate = model.ReleaseDate,
                    DurationMinutes = model.DurationMinutes,
                    AllowedMinAge = model.AllowedMinAge ?? 0,
                    PosterUrl = posterPath,
                    TrailerUrl = model.TrailerUrl,
                    ProducerId = model.ProducerId
                };

                _context.Films.Add(film);
                await _context.SaveChangesAsync();

                await UpdateRelations(film.Id, model);

                TempData["SuccessMessage"] = "Фільм створено успішно!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .Include(f => f.FilmGenres)
                .Include(f => f.FilmActors)
                .Include(f => f.FilmCompanies)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (film == null) return NotFound();

            var viewModel = new AdminFilmFormViewModel
            {
                Id = film.Id,
                Name = film.Name,
                Description = film.Description,
                ReleaseDate = film.ReleaseDate,
                DurationMinutes = film.DurationMinutes,
                AllowedMinAge = film.AllowedMinAge,
                PosterUrl = film.PosterUrl,
                TrailerUrl = film.TrailerUrl,
                ProducerId = film.ProducerId,
                SelectedGenreIds = film.FilmGenres.Select(fg => fg.GenreId).ToList(),
                SelectedActorIds = film.FilmActors.Select(fa => fa.ActorId).ToList(),
                SelectedCompanyIds = film.FilmCompanies.Select(fc => fc.CompanyId).ToList()
            };

            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminFilmFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove(nameof(model.ProducersList));
            ModelState.Remove(nameof(model.GenresList));
            ModelState.Remove(nameof(model.ActorsList));
            ModelState.Remove(nameof(model.CompaniesList));
            ModelState.Remove(nameof(model.SelectedGenreIds));
            ModelState.Remove(nameof(model.SelectedActorIds));
            ModelState.Remove(nameof(model.SelectedCompanyIds));
            ModelState.Remove(nameof(model.PosterUrl));

            if (ModelState.IsValid)
            {
                var filmToUpdate = await _context.Films
                    .Include(f => f.FilmGenres)
                    .Include(f => f.FilmActors)
                    .Include(f => f.FilmCompanies)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (filmToUpdate == null) return NotFound();

                filmToUpdate.Name = model.Name;
                filmToUpdate.Description = model.Description;
                filmToUpdate.ReleaseDate = model.ReleaseDate;
                filmToUpdate.DurationMinutes = model.DurationMinutes;
                filmToUpdate.AllowedMinAge = model.AllowedMinAge ?? 0;
                filmToUpdate.TrailerUrl = model.TrailerUrl;
                filmToUpdate.ProducerId = model.ProducerId;

                if (model.PosterFile != null)
                {
                    filmToUpdate.PosterUrl = await SaveImageAsync(model.PosterFile);
                }
                filmToUpdate.FilmGenres.Clear();
                filmToUpdate.FilmActors.Clear();
                filmToUpdate.FilmCompanies.Clear();
                await _context.SaveChangesAsync();

                await UpdateRelations(id, model);

                TempData["SuccessMessage"] = "Film updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        private async Task UpdateRelations(int filmId, AdminFilmFormViewModel model)
        {
            if (model.SelectedGenreIds != null)
                foreach (var id in model.SelectedGenreIds) _context.FilmGenres.Add(new FilmGenre { FilmId = filmId, GenreId = id });

            if (model.SelectedActorIds != null)
                foreach (var id in model.SelectedActorIds) _context.FilmActors.Add(new FilmActor { FilmId = filmId, ActorId = id, CharacterName = "TBD" });

            if (model.SelectedCompanyIds != null)
                foreach (var id in model.SelectedCompanyIds) _context.FilmCompanies.Add(new FilmCompany { FilmId = filmId, CompanyId = id } as dynamic); // cast workaround or specific logic

            await _context.SaveChangesAsync();
        }

        private async Task PopulateDropdowns(AdminFilmFormViewModel model)
        {
            model.ProducersList = new SelectList(await _context.Producers.ToListAsync(), "Id", "Name");
            model.GenresList = new SelectList(await _context.Genres.ToListAsync(), "Id", "Name");
            model.ActorsList = new SelectList(await _context.Actors.ToListAsync(), "Id", "Name");
            model.CompaniesList = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
        }

        private bool CheckNameDuplication(string name)
        {
            return _context.Films.Any(f => f.Name == name);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film != null)
            {

                var hasSessions = await _context.Sessions.AnyAsync(s => s.FilmId == id);
                if (hasSessions)
                {
                    TempData["ErrorMessage"] = "Cannot delete film because it has related sessions.";
                    return RedirectToAction(nameof(Index));
                }

                var ratings = _context.FilmRatings.Where(r => r.FilmId == id);
                _context.FilmRatings.RemoveRange(ratings);

                if (!string.IsNullOrEmpty(film.PosterUrl) && !film.PosterUrl.StartsWith("http"))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, film.PosterUrl.TrimStart('/'));

                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine($"Could not delete file: {ex.Message}");
                        }
                    }
                }
                _context.Films.Remove(film);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Film deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Film not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
