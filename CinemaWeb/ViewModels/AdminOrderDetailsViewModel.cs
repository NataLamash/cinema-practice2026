using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaWeb.ViewModels;

public class AdminOrderDetailsViewModel // more like Order->Tickets details 
{
    public int TicketId { get; set; }
    public int SessionId { get; set; }

    public int Row { get; set; }
    public int SeatNumber { get; set; }

    public decimal Price { get; set; }
}
