using Microsoft.EntityFrameworkCore;
using CinemaWeb.ViewModels;
using CinemaDomain.Model;
using CinemaInfrastructure;

namespace CinemaWeb.Services
{
    public interface ISessionService
    {
        Task<(bool IsSuccess, string ErrorMessage)> ValidateSessionAsync(SessionViewModel model);
        Task CreateSessionAsync(SessionViewModel model);
        Task UpdateSessionAsync(SessionViewModel model);
    }

    public class SessionService : ISessionService
    {
        private readonly CinemaDbContext _context;
        private const int CleaningTimeMinutes = 20;

        public SessionService(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ValidateSessionAsync(SessionViewModel model)
        {
            if (!await _context.Films.AnyAsync(f => f.Id == model.FilmId))
                return (false, "Обраний фільм не існує.");

            if (!await _context.Halls.AnyAsync(h => h.Id == model.HallId))
                return (false, "Обраний за не існує.");

            if (model.EndTime <= model.StartTime)
                return (false, "Час завершення має бути пізніше початку.");

            if (model.StartTime < DateTime.Now)
                return (false, "Не можна створювати сеанси у минулому.");

            if ((model.EndTime - model.StartTime).TotalMinutes < 60)
                return (false, "Сеанс не може тривати менше однієї години.");

            if (model.StartTime.Hour < 8)
                return (false, "Кінотеатр ще зачинений (відкриття о 08:00).");

            if (model.EndTime.Date > model.StartTime.Date || (model.EndTime.Hour == 0 && model.EndTime.Minute > 0))
                return (false, "Сеанс має закінчитися до опівночі (00:00).");

            var requestedStart = model.StartTime;
            var requestedEndWithCleaning = model.EndTime.AddMinutes(CleaningTimeMinutes);

            bool hasOverlap = await _context.Sessions.AnyAsync(s =>
                s.Id != model.Id &&
                s.HallId == model.HallId &&
                requestedStart < s.EndTime.AddMinutes(CleaningTimeMinutes) &&
                requestedEndWithCleaning > s.StartTime);

            if (hasOverlap)
                return (false, "У залі вже є сеанс або час на прибирання (20 хв).");

            return (true, string.Empty);
        }

        public async Task CreateSessionAsync(SessionViewModel model)
        {
            var session = new Session
            {
                FilmId = model.FilmId.Value,
                HallId = model.HallId.Value,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                BasePrice = model.BasePrice
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSessionAsync(SessionViewModel model)
        {
            var session = await _context.Sessions.FindAsync(model.Id);
            if (session == null) return;

            session.FilmId = model.FilmId.Value;
            session.HallId = model.HallId.Value;
            session.StartTime = model.StartTime;
            session.EndTime = model.EndTime;
            session.BasePrice = model.BasePrice;

            _context.Entry(session).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
