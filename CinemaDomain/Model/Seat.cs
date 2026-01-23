using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class Seat
    {
        public int Id { get; set; }

        public int HallId { get; set; }
        public int SeatTypeId { get; set; }

        public short Row { get; set; }
        public short NumberInRow { get; set; }

        // Navigation
        public Hall? Hall { get; set; }
        public SeatType? SeatType { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

}
