using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class Session
    {
        public int Id { get; set; }

        public int FilmId { get; set; }
        public int HallId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }

        public Film? Film { get; set; }
        public Hall? Hall { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

}
