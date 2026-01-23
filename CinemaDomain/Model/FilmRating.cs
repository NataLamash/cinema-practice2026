using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class FilmRating
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int FilmId { get; set; }

        public byte RatingValue { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; } 
        public Film? Film { get; set; }
    }

}
