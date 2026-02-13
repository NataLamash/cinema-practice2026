namespace CinemaWeb.ViewModels.Booking
{
        public class ConfirmBookingViewModel
        {
            public int SessionId { get; set; }
            public List<ConfirmSeatVm> Seats { get; set; } = new();
            public decimal TotalPrice { get; set; }
        }

        public class ConfirmSeatVm
        {
            public int SeatId { get; set; }
            public short Row { get; set; }
            public short Number { get; set; }
            public string SeatTypeName { get; set; } = "";
            public decimal Price { get; set; }
        }
    }
