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

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.AzureIdentityId).IsUnique();

                entity.Property(u => u.AzureIdentityId).HasMaxLength(450).IsRequired(false);
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.RegistrationDate).IsRequired();
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique();

                entity.Property(os => os.Name).HasMaxLength(50).IsRequired();

                entity.HasData(
                    new OrderStatus { Id = 1, Name = "Reserved" },
                    new OrderStatus { Id = 2, Name = "Paid" },
                    new OrderStatus { Id = 3, Name = "Cancelled" }
                );
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Status)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(o => o.StatusId);

                entity.Property(o => o.OrderDate).IsRequired();
            });

            modelBuilder.Entity<HallType>(entity =>
            {
                entity.Property(h => h.Name).HasMaxLength(50).IsRequired();

                entity.HasData(
                    new HallType { Id = 1, Name = "Standard", Description = "Regular cinema hall" },
                    new HallType { Id = 2, Name = "IMAX", Description = "IMAX large format hall" },
                    new HallType { Id = 3, Name = "VIP", Description = "VIP hall with premium seats" }
                );
            });

            modelBuilder.Entity<Hall>(entity =>
            {
                entity.HasOne(h => h.HallType)
                    .WithMany(ht => ht.Halls)
                    .HasForeignKey(h => h.HallTypeId);

                entity.Property(h => h.Name).HasMaxLength(50).IsRequired();
                entity.Property(h => h.NumberOfRows).IsRequired();
                entity.Property(h => h.SeatsInRow).IsRequired();
            });

            modelBuilder.Entity<SeatType>(entity =>
            {
                entity.Property(st => st.Name).HasMaxLength(50).IsRequired();
                entity.Property(st => st.MarkUpInPercentage).HasPrecision(10, 2);

                entity.HasData(
                    new SeatType { Id = 1, Name = "Standard", MarkUpInPercentage = 0, Description = "Regular seat" },
                    new SeatType { Id = 2, Name = "Comfort", MarkUpInPercentage = 15, Description = "More comfortable seat" },
                    new SeatType { Id = 3, Name = "VIP", MarkUpInPercentage = 30, Description = "Premium seat" }
                );
            });

            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasIndex(s => new { s.HallId, s.Row, s.NumberInRow }).IsUnique();

                entity.HasOne(s => s.Hall)
                    .WithMany(h => h.Seats)
                    .HasForeignKey(s => s.HallId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.SeatType)
                    .WithMany(st => st.Seats)
                    .HasForeignKey(s => s.SeatTypeId);

                entity.Property(s => s.HallId).IsRequired();
                entity.Property(s => s.Row).IsRequired();
                entity.Property(s => s.NumberInRow).IsRequired();
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.Property(g => g.Name).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Producer>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            });

            modelBuilder.Entity<Actor>(entity =>
            {
                entity.Property(a => a.Name).HasMaxLength(150).IsRequired();
                entity.Property(a => a.PhotoUrl).HasMaxLength(512);
            });

            modelBuilder.Entity<Film>(entity =>
            {
                entity.HasOne(f => f.Producer)
                    .WithMany(p => p.Films)
                    .HasForeignKey(f => f.ProducerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(f => f.Name).HasMaxLength(255).IsRequired();
                entity.Property(f => f.DurationMinutes).IsRequired();
                entity.Property(f => f.PosterUrl).HasMaxLength(512);
                entity.Property(f => f.TrailerUrl).HasMaxLength(512);
            });

            modelBuilder.Entity<FilmCompany>(entity =>
            {
                entity.HasKey(fc => new { fc.FilmId, fc.CompanyId });

                entity.HasOne(fc => fc.Film)
                    .WithMany(f => f.FilmCompanies)
                    .HasForeignKey(fc => fc.FilmId);

                entity.HasOne(fc => fc.Company)
                    .WithMany(c => c.FilmCompanies)
                    .HasForeignKey(fc => fc.CompanyId);
            });

            modelBuilder.Entity<FilmGenre>(entity =>
            {
                entity.HasKey(fg => new { fg.FilmId, fg.GenreId });

                entity.HasOne(fg => fg.Film)
                    .WithMany(f => f.FilmGenres)
                    .HasForeignKey(fg => fg.FilmId);

                entity.HasOne(fg => fg.Genre)
                    .WithMany(f => f.FilmGenres)
                    .HasForeignKey(fg => fg.GenreId);
            });

            modelBuilder.Entity<FilmActor>(entity =>
            {
                entity.HasKey(fa => new { fa.FilmId, fa.ActorId });

                entity.HasOne(fa => fa.Film)
                    .WithMany(f => f.FilmActors)
                    .HasForeignKey(fa => fa.FilmId);

                entity.HasOne(fa => fa.Actor)
                    .WithMany(a => a.FilmActors)
                    .HasForeignKey(fa => fa.ActorId);

                entity.Property(fa => fa.CharacterName).HasMaxLength(100);
            });

            modelBuilder.Entity<FilmRating>(entity =>
            {
                entity.HasIndex(r => new { r.UserId, r.FilmId }).IsUnique();

                entity.HasOne(fr => fr.User)
                    .WithMany(u => u.FilmRatings)
                    .HasForeignKey(fr => fr.UserId);

                entity.HasOne(fr => fr.Film)
                    .WithMany(f => f.Ratings)
                    .HasForeignKey(fr => fr.FilmId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(fr => fr.RatingValue).IsRequired();
                entity.Property(fr => fr.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(s => new { s.HallId, s.StartTime });
                entity.HasIndex(s => s.StartTime);

                entity.HasOne(s => s.Film)
                    .WithMany(f => f.Sessions)
                    .HasForeignKey(s => s.FilmId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Hall)
                    .WithMany(h => h.Sessions)
                    .HasForeignKey(s => s.HallId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(s => s.BasePrice).HasPrecision(10, 2).IsRequired();
                entity.Property(s => s.StartTime).IsRequired();
                entity.Property(s => s.EndTime).IsRequired();
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasIndex(t => new { t.SessionId, t.SeatId }).IsUnique();

                entity.HasOne(t => t.Order)
                    .WithMany(o => o.Tickets)
                    .HasForeignKey(t => t.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Session)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.SessionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Seat)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.SeatId);
            });
        }
    }
}
