using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly CinemaDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(CinemaDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/Booking/Confirm
        // Request body example:
        //     {
        //        "sessionId": 1,
        //        "seatIds": [10, 11, 12]
        //     }
        [HttpPost("Confirm")]
        public async Task<IActionResult> Confirm([FromBody] BookingRequestDto request)
        {
            if (request == null || request.SeatIds == null || !request.SeatIds.Any())
                return BadRequest(new {Message = "Місця не обрано."});

            var userId = _userManager.GetUserId(User);

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.AzureIdentityId == userId);

            if (appUser == null)
            {
                return StatusCode(403, new { Message = "Ваш профіль не знайдено." });
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var session = await _context.Sessions
                        .Include(s => s.Hall)
                        .FirstOrDefaultAsync(s => s.Id == request.SessionId);

                    if (session == null) return NotFound(new { Message = "Сеанс не знайдено." } );

                    var seats = await _context.Seats
                        .Include(s => s.SeatType)
                        .Where(s => request.SeatIds.Contains(s.Id))
                        .ToListAsync();

                    if (seats.Count != request.SeatIds.Count) return BadRequest(new { Message = "Вказано не правильні місця." });
                    if (seats.Any(s => s.HallId != session.HallId)) return BadRequest(new { Message = "Вказано місця не правильного залу." });


                    var takenSeats = await _context.Tickets
                        .AnyAsync(t => t.SessionId == session.Id && request.SeatIds.Contains(t.SeatId));

                    if (takenSeats)
                    {
                        await transaction.RollbackAsync();
                        return Conflict(new { Message = "Місця вже заброньовано." });
                    }

                    var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Зарезервовано")
                                 ?? await _context.OrderStatuses.FirstAsync();

                    var order = new Order
                    {
                        UserId = appUser.Id,
                        StatusId = status.Id,
                        OrderDate = DateTime.UtcNow,
                        TotalPrice = 0
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    decimal totalPrice = 0;
                    var tickets = new List<Ticket>();

                    foreach (var seat in seats)
                    {
                        decimal price = session.BasePrice;
                        if (seat.SeatType?.MarkUpInPercentage > 0)
                        {
                            price += price * (seat.SeatType.MarkUpInPercentage.Value / 100m);
                        }
                        price = Math.Round(price, 2);

                        tickets.Add(new Ticket
                        {
                            OrderId = order.Id,
                            SessionId = session.Id,
                            SeatId = seat.Id,
                            PurchasePrice = price
                        });
                        totalPrice += price;
                    }

                    _context.Tickets.AddRange(tickets);

                    order.TotalPrice = totalPrice;
                    _context.Orders.Update(order);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "Success", OrderId = order.Id, order.TotalPrice });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ex.Message);
                }
            });
        }
    }
}
