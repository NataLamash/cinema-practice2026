using CinemaDomain.Model;
using CinemaInfrastructure;
using CinemaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly UserManager<User> _userManager;

        public BookingController(CinemaDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Booking/Confirm
        [HttpPost]
        public async Task<IActionResult> Confirm([FromBody] BookingRequestDto request)
        {
            if (request == null || request.SeatIds == null || !request.SeatIds.Any())
            {
                return BadRequest("Не обрано місця.");
            }

            // Отримуємо поточного користувача
            // Увага: переконайтеся, що ClaimsIdentity налаштовано правильно, інакше User може бути null
            // Для тесту можна тимчасово захардкодити userId, якщо авторизація ще не налаштована
            var userEmail = User.Identity?.Name;
            var currentUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (currentUser == null)
            {
                return Unauthorized("User not found in database.");
            }

            // Починаємо транзакцію (Atomicity)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Отримуємо сеанс та інформацію про залу
                var session = await _context.Sessions
                    .Include(s => s.Hall)
                    .FirstOrDefaultAsync(s => s.Id == request.SessionId);

                if (session == null)
                {
                    return NotFound("Session not found.");
                }

                // 2. Отримуємо вибрані місця з БД (разом з типом місця для ціни)
                var seats = await _context.Seats
                    .Include(s => s.SeatType)
                    .Where(s => request.SeatIds.Contains(s.Id))
                    .ToListAsync();

                // Валідація: чи всі місця знайдені
                if (seats.Count != request.SeatIds.Count)
                {
                    return BadRequest("Some seats define in request do not exist.");
                }

                // Валідація: чи належать місця залу цього сеансу
                if (seats.Any(s => s.HallId != session.HallId))
                {
                    return BadRequest("Selected seats do not belong to the session's hall.");
                }

                // 3. Перевірка: чи місця не зайняті (Check before Insert)
                // Це "м'яка" перевірка для користувача. "Жорстка" буде на рівні БД.
                var takenSeats = await _context.Tickets
                    .Where(t => t.SessionId == session.Id && request.SeatIds.Contains(t.SeatId))
                    .Select(t => t.SeatId)
                    .ToListAsync();

                if (takenSeats.Any())
                {
                    // Можна повернути конкретні номери місць, щоб підсвітити їх на UI
                    return Conflict($"Seats with IDs {string.Join(", ", takenSeats)} are already taken.");
                }

                // 4. Створення Order
                // Знаходимо початковий статус "Reserved" або "Created"
                // Якщо в БД ще немає статусів, це впаде, тому переконайтеся, що Seed спрацював
                var status = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Name == "Reserved")
                             ?? await _context.OrderStatuses.FirstAsync();

                var order = new Order
                {
                    UserId = currentUser.Id,
                    StatusId = status.Id,
                    OrderDate = DateTime.UtcNow,
                    // Tickets додамо пізніше, EF сам все зв'яже
                };

                _context.Orders.Add(order);
                // Зберігаємо, щоб отримати Order.Id (хоча EF Core може зробити це і без збереження, 
                // але для надійності порядку краще зберегти заголовок замовлення)
                await _context.SaveChangesAsync();

                decimal totalPrice = 0;
                var tickets = new List<Ticket>();

                // 5. Створення Tickets і розрахунок ціни
                foreach (var seat in seats)
                {
                    // === Price Calculation (Fallback logic) ===
                    // Беремо базову ціну сеансу
                    decimal ticketPrice = session.BasePrice;

                    // Якщо є націнка за тип місця (наприклад VIP +30%)
                    if (seat.SeatType != null && seat.SeatType.MarkUpInPercentage > 0)
                    {
                        ticketPrice += ticketPrice * (seat.SeatType.MarkUpInPercentage.Value / 100m);
                    }

                    // Округлимо до 2 знаків
                    ticketPrice = Math.Round(ticketPrice, 2);
                    totalPrice += ticketPrice;

                    var ticket = new Ticket
                    {
                        OrderId = order.Id,
                        SessionId = session.Id,
                        SeatId = seat.Id,
                        // Увага: У твоїй моделі Ticket немає поля Price/PurchasePrice.
                        // Зазвичай воно ТРЕБА, щоб зафіксувати ціну покупки.
                        // Я додам коментар, що це поле варто було б додати в Ticket.cs
                        // Поки що ціна живе тільки в Order або агрегується.
                    };
                    tickets.Add(ticket);
                }

                // Додаємо квитки
                _context.Tickets.AddRange(tickets);

                // Оскільки в Order немає поля TotalPrice в твоїй моделі (судячи з коду),
                // ми його не пишемо. Але якщо воно є - треба оновити order.TotalPrice = totalPrice;

                // 6. Фінальне збереження
                await _context.SaveChangesAsync();

                // 7. Commit Transaction
                await transaction.CommitAsync();

                return Ok(new { Message = "Booking confirmed!", OrderId = order.Id });
            }
            catch (DbUpdateException ex)
            {
                // 8. Обробка конфліктів (Unique Constraint Violation)
                // Якщо два юзери одночасно пройшли перевірку takenSeats,
                // база даних викине помилку тут через унікальний індекс [SessionId, SeatId]
                await transaction.RollbackAsync();

                // Логування помилки (ex)
                return Conflict("One or more seats were booked by another user just now. Please try again.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while processing your booking.");
            }
        }
    }
}
