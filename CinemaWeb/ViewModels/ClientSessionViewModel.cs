using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels;

public class ClientSessionViewModel
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public decimal BasePrice { get; set; }

    public int FilmId { get; set; }
    public string FilmName { get; set; } = string.Empty;

    public string HallName { get; set; } = string.Empty;

}
