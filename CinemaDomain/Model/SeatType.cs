using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class SeatType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal MarkUpInPercentage { get; set; }
        public string Description { get; set; }

        public ICollection<Seat> Seats { get; set; }
    }

}
