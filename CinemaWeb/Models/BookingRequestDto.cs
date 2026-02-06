namespace CinemaWeb.Models
{
    public class BookingRequestDto
    {
        public int SessionId { get; set; }
        public List<int> SeatIds { get; set; } = new List<int>();
    }
}
