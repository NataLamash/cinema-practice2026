using CinemaDomain.Model;
using CinemaInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaWeb.Services
{
    public static class SeedData
    {

        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<CinemaDbContext>();

            
            //context.Tickets.RemoveRange(context.Tickets);
            //context.Orders.RemoveRange(context.Orders);
            //context.OrderStatuses.RemoveRange(context.OrderStatuses);
            //context.Sessions.RemoveRange(context.Sessions);
            //context.Seats.RemoveRange(context.Seats);
            //context.SeatTypes.RemoveRange(context.SeatTypes);
            //context.Halls.RemoveRange(context.Halls);
            //context.HallTypes.RemoveRange(context.HallTypes);
            //context.FilmGenres.RemoveRange(context.FilmGenres);
            //context.Genres.RemoveRange(context.Genres);
            //context.FilmRatings.RemoveRange(context.FilmRatings);
            //context.FilmActors.RemoveRange(context.FilmActors);
            //context.FilmCompanies.RemoveRange(context.FilmCompanies);
            //context.Films.RemoveRange(context.Films);
            //await context.SaveChangesAsync();

            await EnsureAdminCreated(serviceProvider, configuration, context);

            await EnsureReferenceData(context);

            await EnsureFilmsAndSessions(context);
        }

        private static async Task EnsureAdminCreated(IServiceProvider serviceProvider, IConfiguration configuration, CinemaDbContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@cinema.com";
            var adminPassword = configuration["AdminPassword"];

            var identityUser = await userManager.FindByEmailAsync(adminEmail);

            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(identityUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "Admin");
                }
            }

            if (identityUser != null)
            {
                if (!context.AppUsers.Any(u => u.AzureIdentityId == identityUser.Id))
                {
                    var appAdmin = new User
                    {
                        AzureIdentityId = identityUser.Id,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "System",
                        RegistrationDate = DateTime.UtcNow,
                        DateOfBirth = new DateTime(2000, 1, 1)
                    };

                    context.AppUsers.Add(appAdmin);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task EnsureReferenceData(CinemaDbContext context)
        {
            if (!context.OrderStatuses.Any())
            {
                context.OrderStatuses.AddRange(
                    new OrderStatus { Name = "Заброньовано" },
                    new OrderStatus { Name = "Оплачено" },     
                    new OrderStatus { Name = "Скасовано" }     
                );
                await context.SaveChangesAsync();
            }

            if (!context.HallTypes.Any())
            {
                context.HallTypes.AddRange(
                    new HallType { Name = "Звичайний", Description = "Стандартний кінозал" },
                    new HallType { Name = "IMAX", Description = "Величезний екран та об'ємний звук" },
                    new HallType { Name = "VIP", Description = "Зал підвищеного комфорту" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.SeatTypes.Any())
            {
                context.SeatTypes.AddRange(
                    new SeatType { Name = "Звичайний", MarkUpInPercentage = 0, Description = "Звичайне крісло" },
                    new SeatType { Name = "Комфорт", MarkUpInPercentage = 15, Description = "Крісло з відкидною спинкою" },
                    new SeatType { Name = "VIP", MarkUpInPercentage = 30, Description = "Шкіряне крісло-диван" }
                );
                await context.SaveChangesAsync();
            }

            // 4. ЖАНРИ (Genres)
            if (!context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Name = "Бойовик" },
                    new Genre { Name = "Фантастика" },
                    new Genre { Name = "Драма" },
                    new Genre { Name = "Комедія" },
                    new Genre { Name = "Жахи" }
                );
                await context.SaveChangesAsync();
            }

            var standardHallType = await context.HallTypes.FirstOrDefaultAsync(h => h.Name == "Звичайний");
            var imaxHallType = await context.HallTypes.FirstOrDefaultAsync(h => h.Name == "IMAX");

            var standardSeatType = await context.SeatTypes.FirstOrDefaultAsync(s => s.Name == "Звичайний");
            var vipSeatType = await context.SeatTypes.FirstOrDefaultAsync(s => s.Name == "VIP");


            if (standardHallType == null
                || imaxHallType == null
                || standardSeatType == null
                || vipSeatType == null)
            {
                return;
            }

            if (!context.Halls.Any())
            {
                context.Halls.AddRange(
                    new Hall
                    {
                        Name = "Червоний зал",
                        NumberOfRows = 5,
                        SeatsInRow = 8,
                        HallTypeId = standardHallType.Id
                    },
                    new Hall
                    {
                        Name = "Синій зал",
                        NumberOfRows = 6,
                        SeatsInRow = 10,
                        HallTypeId = standardHallType.Id
                    },
                    new Hall
                    {
                        Name = "IMAX зал",
                        NumberOfRows = 8,
                        SeatsInRow = 12,
                        HallTypeId = imaxHallType.Id
                    }
                );
                await context.SaveChangesAsync();
            }

            var halls = await context.Halls.Include(h => h.Seats).ToListAsync();

            foreach (var hall in halls)
            {
                if (hall.Seats == null || !hall.Seats.Any())
                {
                    var seats = new List<Seat>();

                    for (short row = 1; row <= hall.NumberOfRows; row++)
                    {
                        for (short number = 1; number <= hall.SeatsInRow; number++)
                        {
                            var currentSeatTypeId = (row == hall.NumberOfRows)
                                ? vipSeatType.Id
                                : standardSeatType.Id;

                            seats.Add(new Seat
                            {
                                HallId = hall.Id,
                                Row = row,
                                NumberInRow = number,
                                SeatTypeId = currentSeatTypeId
                            });
                        }
                    }
                    context.Seats.AddRange(seats);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureFilmsAndSessions(CinemaDbContext context)
        {
            if (context.Films.Any())
            {
                return;
            }

            var allGenres = await context.Genres.ToListAsync();
            var allHalls = await context.Halls.ToListAsync();

            if (!allGenres.Any() || !allHalls.Any())
            {
                return;
            }

            var films = new List<Film>
            {
                new Film {
                    Name = "Дюна: Частина друга",
                    Description = "Пол Атрід об'єднується з Чані та Фріменами, щоб помститися змовникам, які знищили його родину.",
                    ReleaseDate = DateTime.Now.AddDays(-10),
                    DurationMinutes = 166,
                    AllowedMinAge = 12,
                    PosterUrl = "https://image.tmdb.org/t/p/original/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=Way9Dexny3w"
                },
                new Film {
                    Name = "Оппенгеймер",
                    Description = "Історія життя американського фізика Роберта Оппенгеймера, який очолював перші розробки ядерної зброї.",
                    ReleaseDate = DateTime.Now.AddDays(-20),
                    DurationMinutes = 180,
                    AllowedMinAge = 16,
                    PosterUrl = "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=uYPbbksJxIg"
                },
                new Film {
                    Name = "Матриця",
                    Description = "Хакер Нео дізнається від таємничих повстанців правду про реальність: світ є імітацією, створеною машинами.",
                    ReleaseDate = new DateTime(1999, 3, 31),
                    DurationMinutes = 136,
                    AllowedMinAge = 16,
                    PosterUrl = "https://image.tmdb.org/t/p/original/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=m8e-FF8MsqU"
                },
                new Film {
                    Name = "Інтерстеллар",
                    Description = "Команда дослідників вирушає крізь червоточину у просторі, намагаючись знайти нову домівку для людства.",
                    ReleaseDate = new DateTime(2014, 11, 7),
                    DurationMinutes = 169,
                    AllowedMinAge = 12,
                    PosterUrl = "https://image.tmdb.org/t/p/original/gEU2QniL6C8z1BHu8sqQjsvl2ym.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=zSWdZVtXT7E"
                },
                new Film {
                    Name = "Початок",
                    Description = "Кобб — талановитий злодій, найкращий у небезпечному мистецтві вилучення: він краде цінні секрети з глибин підсвідомості.",
                    ReleaseDate = new DateTime(2010, 7, 16),
                    DurationMinutes = 148,
                    AllowedMinAge = 12,
                    PosterUrl = "https://image.tmdb.org/t/p/original/9gk7admal4zl248sKidtwi9x3bH.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=YoHD9XEInc0"
                },
                
                new Film {
                    Name = "Аватар 3",
                    Description = "Продовження епічної саги про народ На'ві та їх боротьбу за виживання на Пандорі.",
                    ReleaseDate = DateTime.Now.AddYears(1),
                    DurationMinutes = 190,
                    AllowedMinAge = 12,
                    PosterUrl = "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=d9MyqW1pTOc"
                },
                 new Film {
                    Name = "Джокер: Божевілля на двох",
                    Description = "Артур Флек знаходить кохання та спільницю в стінах лікарі Аркхем.",
                    ReleaseDate = DateTime.Now.AddMonths(5),
                    DurationMinutes = 130,
                    AllowedMinAge = 18,
                    PosterUrl = "https://image.tmdb.org/t/p/original/aciP8Km0waTLXEYf5ybXB57zbpZ.jpg",
                    TrailerUrl = ""
                },
                 new Film {
                    Name = "Дедпул 3",
                    Description = "Дедпул об'єднується з Росомахою, щоб змінити історію кіновсесвіту.",
                    ReleaseDate = DateTime.Now.AddMonths(3),
                    DurationMinutes = 120,
                    AllowedMinAge = 18,
                    PosterUrl = "https://image.tmdb.org/t/p/original/yF1eOkaYvwiORauRCPWznV9xVvi.jpg",
                    TrailerUrl = ""
                },
                 new Film {
                    Name = "Гладіатор 2",
                    Description = "Історія Луція, племінника Коммода, через роки після смерті Максимуса.",
                    ReleaseDate = DateTime.Now.AddMonths(8),
                    DurationMinutes = 150,
                    AllowedMinAge = 16,
                    PosterUrl = "https://image.tmdb.org/t/p/original/2cxhvwyEwRlysAmf4oo67BCZ00.jpg",
                    TrailerUrl = ""
                },
                 new Film {
                    Name = "Міккі 17",
                    Description = "Міккі — «відновлюваний» співробітник, якого відправляють на смертельно небезпечні місії з колонізації крижаного світу.",
                    ReleaseDate = DateTime.Now.AddMonths(2),
                    DurationMinutes = 139,
                    AllowedMinAge = 16,
                    PosterUrl = "https://image.tmdb.org/t/p/original/55sKjM6G2Fq1x2z1X3lFz1x2z1X.jpg",
                    TrailerUrl = ""
                }
            };

            context.Films.AddRange(films);
            await context.SaveChangesAsync();

            var sessions = new List<Session>();
            var random = new Random();

            for (int i = 0; i < films.Count; i++)
            {
                var film = films[i];

                var randomGenre = allGenres[random.Next(allGenres.Count)];
                context.FilmGenres.Add(new FilmGenre { FilmId = film.Id, GenreId = randomGenre.Id });

                if (i < 5)
                {
                    for (int day = 0; day < 3; day++)
                    {
                        var randomHall = allHalls[random.Next(allHalls.Count)];

                        sessions.Add(new Session
                        {
                            FilmId = film.Id,
                            HallId = randomHall.Id,
                            StartTime = DateTime.Today.AddDays(day).AddHours(10 + random.Next(0, 8)),
                            BasePrice = 150 + random.Next(0, 50)
                        });

                        sessions.Add(new Session
                        {
                            FilmId = film.Id,
                            HallId = randomHall.Id,
                            StartTime = DateTime.Today.AddDays(day).AddHours(19 + random.Next(0, 2)),
                            BasePrice = 250
                        });
                    }
                }
            }

            context.Sessions.AddRange(sessions);
            await context.SaveChangesAsync();
        }
    }
}
