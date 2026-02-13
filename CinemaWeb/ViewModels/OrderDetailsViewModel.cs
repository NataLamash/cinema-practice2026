namespace CinemaWeb.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; } // Було CreatedDate
        public string Status { get; set; }
        public List<TicketViewModel> Tickets { get; set; } = new List<TicketViewModel>();
    }
}
