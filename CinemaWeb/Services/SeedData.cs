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

            // 1. СТВОРЕННЯ АДМІНА (Залишаємо як було)
            await EnsureAdminCreated(serviceProvider, configuration, context);

            // 2. СТВОРЕННЯ ДОВІДНИКІВ (Тільки якщо база порожня)
            await EnsureReferenceData(context);

            // 3. СТВОРЕННЯ ФІЛЬМІВ (Українською)
            //await EnsureFilmsAndSessions(context);
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
    }
}
