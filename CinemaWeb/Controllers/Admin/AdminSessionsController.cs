using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.ViewModels; 
using CinemaDomain.Model;   
using CinemaInfrastructure;

namespace CinemaWeb.Controllers.Admin
{
    public class AdminSessionsController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminSessionsController(CinemaDbContext context)
        {
            _context = context;
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
        // GET: AdminSessions/Create
        public async Task<IActionResult> Create()
        {
            await PopulateListsAsync();

            // Створюємо дату, обнуляючи секунди та мілісекунди вручну
            var start = DateTime.Now.AddHours(1);
            var end = DateTime.Now.AddHours(3);

            return View(new SessionViewModel
            {
                StartTime = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0),
                EndTime = new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, 0)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionViewModel model)
        {
            // Обнуляємо секунди перед валідацією та збереженням
            model.StartTime = new DateTime(model.StartTime.Year, model.StartTime.Month, model.StartTime.Day, model.StartTime.Hour, model.StartTime.Minute, 0);
            model.EndTime = new DateTime(model.EndTime.Year, model.EndTime.Month, model.EndTime.Day, model.EndTime.Hour, model.EndTime.Minute, 0);

            await RunBusinessValidations(model);

            if (ModelState.IsValid)
            {
                var session = new Session
                {
                    FilmId = model.FilmId,
                    HallId = model.HallId,
                    StartTime = model.StartTime, // Тепер тут гарантовано 00 секунд
                    EndTime = model.EndTime,
                    BasePrice = model.BasePrice
                };
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                Id = session.Id, // Виправлено: звернення до model.Id
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
            if (id != model.Id) return NotFound(); // Виправлено: model.Id

            await RunBusinessValidations(model);

            if (ModelState.IsValid)
            {
                var session = await _context.Sessions.FindAsync(id);
                if (session == null) return NotFound();

                session.FilmId = model.FilmId;
                session.HallId = model.HallId;
                session.StartTime = model.StartTime;
                session.EndTime = model.EndTime;
                session.BasePrice = model.BasePrice;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        private async Task RunBusinessValidations(SessionViewModel model)
        {
            // Перевірка існування фільму та залу
            if (!await _context.Films.AnyAsync(f => f.Id == model.FilmId))
                ModelState.AddModelError("FilmId", "Обраний фільм не існує.");

            if (!await _context.Halls.AnyAsync(h => h.Id == model.HallId))
                ModelState.AddModelError("HallId", "Обраний зал не існує.");

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Час завершення має бути пізніше початку.");
                return;
            }

            if ((model.EndTime - model.StartTime).TotalMinutes < 60)
                ModelState.AddModelError("EndTime", "Сеанс не може тривати менше однієї години.");

            if (model.StartTime.Hour < 8)
                ModelState.AddModelError("StartTime", "Кінотеатр ще зачинений (відкриття о 08:00).");

            if (model.EndTime.Date > model.StartTime.Date || (model.EndTime.Hour == 0 && model.EndTime.Minute > 0))
                ModelState.AddModelError("EndTime", "Сеанс має закінчитися до опівночі.");

            if (model.StartTime < DateTime.Now)
                ModelState.AddModelError("StartTime", "Не можна створювати сеанси у минулому.");

            // Перевірка накладок (Overlap) з урахуванням 20 хв прибирання
            int cleaningTime = 20;
            bool hasOverlap = await _context.Sessions.AnyAsync(s =>
                s.Id != model.Id && // Виправлено: model.Id
                s.HallId == model.HallId &&
                model.StartTime < s.EndTime.AddMinutes(cleaningTime) &&
                model.EndTime.AddMinutes(cleaningTime) > s.StartTime);

            if (hasOverlap)
                ModelState.AddModelError(string.Empty, "У залі вже є сеанс або час на прибирання (20 хв).");
        }

        private async Task PopulateListsAsync()
        {
            ViewBag.Films = new SelectList(await _context.Films.OrderBy(f => f.Name).ToListAsync(), "Id", "Name");
            ViewBag.Halls = new SelectList(await _context.Halls.OrderBy(h => h.Name).ToListAsync(), "Id", "Name");
        }
    }
}
