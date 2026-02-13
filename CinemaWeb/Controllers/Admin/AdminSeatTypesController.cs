using CinemaDomain.Model;
using CinemaInfrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaWeb.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminSeatTypesController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminSeatTypesController(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var seatTypes = await _context.SeatTypes.ToListAsync();
            return View(seatTypes);
        }

        public IActionResult Create() => View();

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var seatType = await _context.SeatTypes.FindAsync(id);
            if (seatType == null) return NotFound();
            return View(seatType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeatType seatType)
        {
            if (seatType.MarkUpInPercentage < 0 || seatType.MarkUpInPercentage > 500)
            {
                ModelState.AddModelError("MarkUpInPercentage", "Націнка повинна бути в межах від 0% до 500%.");
            }

            if (await _context.SeatTypes.AnyAsync(st => st.Name == seatType.Name))
            {
                ModelState.AddModelError("Name", "Тип місця з такою назвою вже існує.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(seatType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(seatType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SeatType seatType)
        {
            if (id != seatType.Id) return NotFound();

            if (seatType.MarkUpInPercentage < 0 || seatType.MarkUpInPercentage > 500)
            {
                ModelState.AddModelError("MarkUpInPercentage", "Націнка повинна бути в межах від 0% до 500%.");
            }

            if (await _context.SeatTypes.AnyAsync(st => st.Name == seatType.Name && st.Id != id))
            {
                ModelState.AddModelError("Name", "Інший тип місця вже має таку назву.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(seatType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(seatType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var seatType = await _context.SeatTypes
                .Include(st => st.Seats)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (seatType == null) return NotFound();

            if (seatType.Seats != null && seatType.Seats.Any())
            {
                TempData["Error"] = "Неможливо видалити тип місця, оскільки він використовується в залах.";
                return RedirectToAction(nameof(Index));
            }

            _context.SeatTypes.Remove(seatType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeatTypeExists(int id) => _context.SeatTypes.Any(e => e.Id == id);
    }
}
