using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CinemaWeb.Controllers.Admin
{
    public class AdminFilmsController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminFilmsController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: AdminFilms
        public async Task<IActionResult> Index()
        {
            var films = await _context.Films
                .Include(f => f.Producer)
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .OrderByDescending(f => f.Id) // Свіжі зверху
                .ToListAsync();
            return View(films);
        }

        // GET: AdminFilms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var film = await _context.Films
                .Include(f => f.Producer)
                .Include(f => f.FilmCompanies).ThenInclude(fc => fc.Company)
                .Include(f => f.FilmGenres).ThenInclude(fg => fg.Genre)
                .Include(f => f.FilmActors).ThenInclude(fa => fa.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (film == null) return NotFound();

            return View(film);
        }

        // GET: AdminFilms/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new FilmFormViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        // POST: AdminFilms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmFormViewModel model)
        {
            if (CheckNameDuplication(model.Name))
            {
                ModelState.AddModelError("Name", "Фільм з такою назвою вже існує!");
            }

            if (ModelState.IsValid)
            {
                var film = new Film
                {
                    Name = model.Name,
                    Description = model.Description,
                    ReleaseDate = model.ReleaseDate,
                    DurationMinutes = model.DurationMinutes,
                    AllowedMinAge = model.AllowedMinAge,
                    PosterUrl = model.PosterUrl,
                    TrailerUrl = model.TrailerUrl,
                    ProducerId = model.ProducerId
                };

                _context.Films.Add(film);
                await _context.SaveChangesAsync();

                if (model.SelectedGenreIds.Any())
                {
                    foreach (var genreId in model.SelectedGenreIds)
                    {
                        _context.FilmGenres.Add(new FilmGenre { FilmId = film.Id, GenreId = genreId });
                    }
                }

                if (model.SelectedActorIds.Any())
                {
                    foreach (var actorId in model.SelectedActorIds)
                    {
                        _context.FilmActors.Add(new FilmActor { FilmId = film.Id, ActorId = actorId, CharacterName = "TBD" });
                    }
                }

                if (model.SelectedCompanyIds.Any())
                {
                    foreach (var companyId in model.SelectedCompanyIds)
                    {
                        _context.FilmCompanies.Add(new FilmCompany { FilmId = film.Id, CompanyId = companyId });
                    }
                }

                await _context.SaveChangesAsync();
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

            var viewModel = new FilmFormViewModel
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

        // POST: AdminFilms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FilmFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var filmToUpdate = await _context.Films
                    .Include(f => f.FilmGenres)
                    .Include(f => f.FilmActors)
                    .Include(f => f.FilmCompanies)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (filmToUpdate == null) return NotFound();

                if (filmToUpdate.Name != model.Name && CheckNameDuplication(model.Name))
                {
                    ModelState.AddModelError("Name", "Фільм з такою назвою вже існує!");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                filmToUpdate.Name = model.Name;
                filmToUpdate.Description = model.Description;
                filmToUpdate.ReleaseDate = model.ReleaseDate;
                filmToUpdate.DurationMinutes = model.DurationMinutes;
                filmToUpdate.AllowedMinAge = model.AllowedMinAge;
                filmToUpdate.PosterUrl = model.PosterUrl;
                filmToUpdate.TrailerUrl = model.TrailerUrl;
                filmToUpdate.ProducerId = model.ProducerId;

                filmToUpdate.FilmGenres.Clear();
                foreach (var genreId in model.SelectedGenreIds)
                {
                    filmToUpdate.FilmGenres.Add(new FilmGenre { FilmId = id, GenreId = genreId });
                }

                filmToUpdate.FilmActors.Clear();
                foreach (var actorId in model.SelectedActorIds)
                {
                    filmToUpdate.FilmActors.Add(new FilmActor { FilmId = id, ActorId = actorId, CharacterName = "Updated" });
                }

                filmToUpdate.FilmCompanies.Clear();
                foreach (var companyId in model.SelectedCompanyIds)
                {
                    filmToUpdate.FilmCompanies.Add(new FilmCompany { FilmId = id, CompanyId = companyId });
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(filmToUpdate.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: AdminFilms/Delete/5
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

        private async Task PopulateDropdowns(FilmFormViewModel model)
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

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}
