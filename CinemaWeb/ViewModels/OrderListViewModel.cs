namespace CinemaWeb.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } 
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public int TicketsCount { get; set; }
    }
}
