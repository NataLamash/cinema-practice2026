using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaDomain.Model;

namespace CinemaInfrastructure
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Hall> Halls { get; set; }
        public DbSet<HallType> HallTypes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }

        public DbSet<Film> Films { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<FilmCompany> FilmCompanies { get; set; }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<FilmGenre> FilmGenres { get; set; }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<FilmActor> FilmActors { get; set; }

        public DbSet<FilmRating> FilmRatings { get; set; }
        public DbSet<Session> Sessions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNIQUE INDEXES

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.AzureIdentityId)
                .IsUnique();

            modelBuilder.Entity<OrderStatus>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.HallId, s.Row, s.NumberInRow })
                .IsUnique();

            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.SessionId, t.SeatId })
                .IsUnique();

            modelBuilder.Entity<FilmRating>()
                .HasIndex(r => new { r.UserId, r.FilmId })
                .IsUnique();

            // COMPOSITE KEYS

            modelBuilder.Entity<FilmCompany>()
                .HasKey(fc => new { fc.FilmId, fc.CompanyId });

            modelBuilder.Entity<FilmGenre>()
                .HasKey(fg => new { fg.FilmId, fg.GenreId });

            modelBuilder.Entity<FilmActor>()
                .HasKey(fa => new { fa.FilmId, fa.ActorId });

            //  RELATIONSHIPS

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrderId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Session)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SessionId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.SeatId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Film)
                .WithMany(f => f.Sessions)
                .HasForeignKey(s => s.FilmId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HallId);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Hall)
                .WithMany(h => h.Seats)
                .HasForeignKey(s => s.HallId);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.SeatType)
                .WithMany(st => st.Seats)
                .HasForeignKey(s => s.SeatTypeId);

            modelBuilder.Entity<Hall>()
                .HasOne(h => h.HallType)
                .WithMany(ht => ht.Halls)
                .HasForeignKey(h => h.HallTypeId);

            modelBuilder.Entity<Film>()
                .HasOne(f => f.Producer)
                .WithMany(p => p.Films)
                .HasForeignKey(f => f.ProducerId);

            modelBuilder.Entity<FilmCompany>()
                .HasOne(fc => fc.Film)
                .WithMany(f => f.FilmCompanies)
                .HasForeignKey(fc => fc.FilmId);

            modelBuilder.Entity<FilmCompany>()
                .HasOne(fc => fc.Company)
                .WithMany(c => c.FilmCompanies)
                .HasForeignKey(fc => fc.CompanyId);

            modelBuilder.Entity<FilmGenre>()
                .HasOne(fg => fg.Film)
                .WithMany(f => f.FilmGenres)
                .HasForeignKey(fg => fg.FilmId);

            modelBuilder.Entity<FilmGenre>()
                .HasOne(fg => fg.Film)
                .WithMany(f => f.FilmGenres)
                .HasForeignKey(fg => fg.GenreId);

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Film)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(fg => fg.FilmId);

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Actor)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(fg => fg.ActorId);

            modelBuilder.Entity<FilmRating>()
                .HasOne(fr => fr.User)
                .WithMany(u => u.FilmRatings)
                .HasForeignKey(fr => fr.UserId);

            modelBuilder.Entity<FilmRating>()
                .HasOne(fr => fr.Film)
                .WithMany(f => f.Ratings)
                .HasForeignKey(fr => fr.FilmId);

            //  Length Constraints and other configurations

            modelBuilder.Entity<User>()
                .Property(u => u.AzureIdentityId)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.RegistrationDate)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .IsRequired();

            modelBuilder.Entity<OrderStatus>()
                .Property(os => os.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Hall>()
                .Property(h => h.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Hall>()
                .Property(h => h.NumberOfRows)
                .IsRequired();

            modelBuilder.Entity<Hall>()
                .Property(h => h.SeatsInRow)
                .IsRequired();

            modelBuilder.Entity<HallType>()
                .Property(h => h.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.HallId)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.Row)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.NumberInRow)
                .IsRequired();

            modelBuilder.Entity<SeatType>()
                .Property(st => st.Name)
                .HasMaxLength(50)
                .IsRequired();


            modelBuilder.Entity<SeatType>()
                .Property(st => st.MarkUpInPercentage)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Film>()
                .Property(f => f.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Film>()
                .Property(f => f.DurationMinutes)
                .IsRequired();

            modelBuilder.Entity<Film>()
                .Property(f => f.PosterUrl)
                .HasMaxLength(512);

            modelBuilder.Entity<Film>()
                .Property(f => f.TrailerUrl)
                .HasMaxLength(512);

            modelBuilder.Entity<Producer>()
                .Property(p => p.Name)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .Property(c => c.Name)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Genre>()
                .Property(g => g.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Actor>()
                .Property(a => a.Name)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Actor>()
                .Property(a => a.PhotoUrl)
                .HasMaxLength(512);

            modelBuilder.Entity<Session>()
                .Property(s => s.BasePrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Session>()
                .Property(s => s.StartTime)
                .IsRequired();

            modelBuilder.Entity<Session>()
                .Property(s => s.EndTime)
                .IsRequired();

            //  SEED DATA

            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, Name = "Reserved" },
                new OrderStatus { Id = 2, Name = "Paid" },
                new OrderStatus { Id = 3, Name = "Cancelled" }
            );

            modelBuilder.Entity<HallType>().HasData(
                new HallType { Id = 1, Name = "Standard", Description = "Regular cinema hall" },
                new HallType { Id = 2, Name = "IMAX", Description = "IMAX large format hall" },
                new HallType { Id = 3, Name = "VIP", Description = "VIP hall with premium seats" }
            );

            modelBuilder.Entity<SeatType>().HasData(
                new SeatType { Id = 1, Name = "Standard", MarkUpInPercentage = 0, Description = "Regular seat" },
                new SeatType { Id = 2, Name = "Comfort", MarkUpInPercentage = 15, Description = "More comfortable seat" },
                new SeatType { Id = 3, Name = "VIP", MarkUpInPercentage = 30, Description = "Premium seat" }
            );
        }
    }
}
