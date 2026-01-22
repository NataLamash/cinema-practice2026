using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CinemaDomain.Model
{
    public class Film
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ProducerId { get; set; }

        public DateTime? ReleaseDate { get; set; }
        public short DurationMinutes { get; set; }
        public byte? AllowedMinAge { get; set; }

        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }

        // Navigation
        public Producer? Producer { get; set; }

        public ICollection<FilmCompany> FilmCompanies { get; set; } = new List<FilmCompany>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<FilmGenre> FilmGenres { get; set; } = new List<FilmGenre>();
        public ICollection<FilmActor> FilmActors { get; set; } = new List<FilmActor>();
        public ICollection<FilmRating> Ratings { get; set; } = new List<FilmRating>();
    }

}
