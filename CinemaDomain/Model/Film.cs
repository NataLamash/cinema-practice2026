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

        public int ProducerId { get; set; }
        public int CompanyId { get; set; }

        public DateTime ReleaseDate { get; set; }
        public short DurationMinutes { get; set; }
        public byte AllowedMinAge { get; set; }

        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }

        // Navigation
        public Producer Producer { get; set; }

        public ICollection<FilmCompany> FilmCompanies { get; set; }
        public ICollection<Session> Sessions { get; set; }
        public ICollection<FilmGenre> FilmGenres { get; set; }
        public ICollection<FilmActor> FilmActors { get; set; }
        public ICollection<FilmRating> Ratings { get; set; }
    }

}
