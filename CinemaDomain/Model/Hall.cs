using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CinemaDomain.Model
{
    public class Hall
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public short NumberOfRows { get; set; }
        public short SeatsInRow { get; set; }

        public int HallTypeId { get; set; }

        // Navigation
        public HallType? HallType { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }

}
