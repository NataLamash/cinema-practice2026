namespace CinemaWeb.ViewModels
{
    public class TicketViewModel
    {
        public string FilmTitle { get; set; }
        public DateTime SessionStart { get; set; }
        public string HallName { get; set; }
        public int Row { get; set; }
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
    }
}
