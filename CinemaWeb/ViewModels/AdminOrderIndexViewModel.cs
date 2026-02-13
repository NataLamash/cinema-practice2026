using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaWeb.ViewModels;

public class AdminOrderIndexViewModel
{
    public int Id { get; set; }
    public string UserEmail { get; set; } 
    public string StatusName { get; set; } 
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int TicketCount { get; set; }
}
