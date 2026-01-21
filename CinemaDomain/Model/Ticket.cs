using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CinemaDomain.Model
{
    public class Ticket
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public int SessionId { get; set; }
        public int SeatId { get; set; }

        public Order Order { get; set; }
        public Session Session { get; set; }
        public Seat Seat { get; set; }
    }

}
