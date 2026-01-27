using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.ViewModels;
using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.Services; 

namespace CinemaWeb.Controllers.Admin
{
    public class AdminSessionsController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly ISessionService _sessionService;

        public AdminSessionsController(CinemaDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // GET: AdminSessions
        public async Task<IActionResult> Index()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Film)
                .Include(s => s.Hall)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
            return View(sessions);
        }

        // GET: AdminSessions/Create
        public async Task<IActionResult> Create()
        {
            await PopulateListsAsync();

            var start = DateTime.Now.AddHours(1);
            var end = DateTime.Now.AddHours(3);

            return View(new SessionViewModel
            {
                StartTime = ClearSeconds(start),
                EndTime = ClearSeconds(end)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionViewModel model)
        {
            model.StartTime = ClearSeconds(model.StartTime);
            model.EndTime = ClearSeconds(model.EndTime);

            if (ModelState.IsValid)
            {
                var (isSuccess, errorMessage) = await _sessionService.ValidateSessionAsync(model);

                if (isSuccess)
                {
                    await _sessionService.CreateSessionAsync(model);
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, errorMessage);
            }

            await PopulateListsAsync();
            return View(model);
        }

        // GET: AdminSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            var model = new SessionViewModel
            {
                Id = session.Id,
                FilmId = session.FilmId,
                HallId = session.HallId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                BasePrice = session.BasePrice
            };

            await PopulateListsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SessionViewModel model)
        {
            if (id != model.Id) return NotFound();

            model.StartTime = ClearSeconds(model.StartTime);
            model.EndTime = ClearSeconds(model.EndTime);

            if (ModelState.IsValid)
            {
                var (isSuccess, errorMessage) = await _sessionService.ValidateSessionAsync(model);

                if (isSuccess)
                {
                    await _sessionService.UpdateSessionAsync(model);
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, errorMessage);
            }

            await PopulateListsAsync();
            return View(model);
        }

        // POST: AdminSessions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var session = await _context.Sessions
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            if (session.Tickets != null && session.Tickets.Any())
            {
                TempData["Error"] = "Неможливо видалити сеанс, на який уже продано квитки.";
                return RedirectToAction(nameof(Index));
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateListsAsync()
        {
            ViewBag.Films = new SelectList(await _context.Films.OrderBy(f => f.Name).ToListAsync(), "Id", "Name");
            ViewBag.Halls = new SelectList(await _context.Halls.OrderBy(h => h.Name).ToListAsync(), "Id", "Name");
        }

        private DateTime ClearSeconds(DateTime dt)
            => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
    }
}
