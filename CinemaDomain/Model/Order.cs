using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int StatusId { get; set; }

        public DateTime OrderDate { get; set; }

        // Navigation
        public User User { get; set; }
        public OrderStatus Status { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }

}
