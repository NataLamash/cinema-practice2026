namespace CinemaWeb.ViewModels.Booking
{
    public class SelectSeatsViewModel
    {
        public int SessionId { get; set; }
        public string? FilmTitle { get; set; }
        public DateTime StartTime { get; set; }
        public int HallId { get; set; }
        public string? HallName { get; set; }
        public decimal BasePrice { get; set; }
        public List<SeatRowVm> Rows { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class SeatRowVm
    {
        public short RowNumber { get; set; }
        public List<SeatVm> Seats { get; set; } = new();
    }

    public class SeatVm
    {
        public int SeatId { get; set; }
        public short Row { get; set; }
        public short Number { get; set; }
        public int SeatTypeId { get; set; }
        public string SeatTypeName { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsTaken { get; set; }
    }
}
