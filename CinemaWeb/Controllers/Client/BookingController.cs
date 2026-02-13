using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaInfrastructure;
using CinemaWeb.ViewModels.Booking;

namespace CinemaWeb.Controllers.Client
{
    public class BookingController : Controller
    {
        private readonly CinemaDbContext _context;

        public BookingController(CinemaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeats(int sessionId, string? error = null)
        {
            var vm = await BuildSelectSeatsViewModel(sessionId);
            if (vm == null) return NotFound();

            vm.ErrorMessage = error;
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Seat)
                        .ThenInclude(s => s.SeatType)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Session)
                        .ThenInclude(s => s.Film)
                .Include(o => o.Tickets)
                    .ThenInclude(t => t.Session)
                        .ThenInclude(s => s.Hall)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int sessionId, int[] selectedSeatIds)
        {
            if (selectedSeatIds == null || selectedSeatIds.Length == 0)
                return RedirectToAction(nameof(SelectSeats), new { sessionId, error = "Оберіть хоча б одне місце." });

            var takenSeatIds = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.SessionId == sessionId)
                .Select(t => t.SeatId)
                .ToListAsync();

            var takenSet = takenSeatIds.ToHashSet();
            var filteredSelected = selectedSeatIds.Where(id => !takenSet.Contains(id)).Distinct().ToArray();

            if (filteredSelected.Length == 0)
                return RedirectToAction(nameof(SelectSeats), new { sessionId, error = "Обрані місця вже зайняті. Спробуйте інші." });

            var session = await _context.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            var seats = await _context.Seats
                .AsNoTracking()
                .Include(s => s.SeatType)
                .Where(s => filteredSelected.Contains(s.Id))
                .OrderBy(s => s.Row)
                .ThenBy(s => s.NumberInRow)
                .ToListAsync();

            var confirm = new ConfirmBookingViewModel
            {
                SessionId = sessionId
            };

            foreach (var s in seats)
            {
                var markup = s.SeatType?.MarkUpInPercentage ?? 0m;
                var price = session.BasePrice * (1m + (markup / 100m));
                price = Math.Round(price, 2, MidpointRounding.AwayFromZero);

                confirm.Seats.Add(new ConfirmSeatVm
                {
                    SeatId = s.Id,
                    Row = s.Row,
                    Number = s.NumberInRow,
                    SeatTypeName = s.SeatType?.Name ?? "Unknown",
                    Price = price
                });
            }

            confirm.TotalPrice = confirm.Seats.Sum(x => x.Price);
            return View(confirm);
        }

        private async Task<SelectSeatsViewModel?> BuildSelectSeatsViewModel(int sessionId)
        {
            var session = await _context.Sessions
                .AsNoTracking()
                .Include(s => s.Film)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            var seats = await _context.Seats
                .AsNoTracking()
                .Include(s => s.SeatType)
                .Where(s => s.HallId == session.HallId)
                .OrderBy(s => s.Row)
                .ThenBy(s => s.NumberInRow)
                .ToListAsync();

            var takenSeatIds = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.SessionId == sessionId)
                .Select(t => t.SeatId)
                .ToListAsync();

            var takenSet = takenSeatIds.ToHashSet();

            var vm = new SelectSeatsViewModel
            {
                SessionId = sessionId,
                FilmTitle = session.Film?.Name,
                StartTime = session.StartTime,
                HallId = session.HallId,
                HallName = session.Hall?.Name,
                BasePrice = session.BasePrice
            };

            var grouped = seats.GroupBy(s => s.Row).OrderBy(g => g.Key);

            foreach (var rowGroup in grouped)
            {
                var rowVm = new SeatRowVm { RowNumber = rowGroup.Key };

                foreach (var seat in rowGroup.OrderBy(s => s.NumberInRow))
                {
                    var markup = seat.SeatType?.MarkUpInPercentage ?? 0m;
                    var price = session.BasePrice * (1m + (markup / 100m));
                    price = Math.Round(price, 2, MidpointRounding.AwayFromZero);

                    rowVm.Seats.Add(new SeatVm
                    {
                        SeatId = seat.Id,
                        Row = seat.Row,
                        Number = seat.NumberInRow,
                        SeatTypeId = seat.SeatTypeId,
                        SeatTypeName = seat.SeatType?.Name ?? "Unknown",
                        Price = price,
                        IsTaken = takenSet.Contains(seat.Id)
                    });
                }

                vm.Rows.Add(rowVm);
            }

            return vm;
        }
    }
}
