using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.ViewModels;

namespace CinemaWeb.Controllers.Client
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly CinemaDbContext _context;

        public AccountController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Orders
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = await _context.Set<User>()
                                            .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null)
            {
                return RedirectToAction("Logout", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.Status)
                .Where(o => o.UserId == currentUser.Id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status != null ? o.Status.Name : "N/A",
                    TotalPrice = o.TotalPrice,
                    TicketsCount = o.Tickets.Count
                })
                .ToListAsync();

            return View(orders);
        }

        // GET: /Account/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login", "Account");

            var currentUser = await _context.Set<User>()
                                            .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null) return RedirectToAction("Logout", "Account");

            var order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Film)
                .Include(o => o.Tickets).ThenInclude(t => t.Session).ThenInclude(s => s.Hall)
                .Include(o => o.Tickets).ThenInclude(t => t.Seat)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != currentUser.Id)
            {
                return Forbid();
            }

            var viewModel = new OrderDetailsViewModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status != null ? order.Status.Name : "N/A",
                Tickets = order.Tickets.Select(t => new TicketViewModel
                {
                    FilmTitle = t.Session.Film.Name,
                    SessionStart = t.Session.StartTime,
                    HallName = t.Session.Hall.Name,
                    Row = t.Seat.Row,
                    SeatNumber = t.Seat.NumberInRow,
                    Price = t.PurchasePrice
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
