using CinemaDomain.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
    public class FilmFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Film title is required")]
        [Display(Name = "Title")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 1000, ErrorMessage = "Duration must be greater than 0")]
        [Display(Name = "Duration (min)")]
        public short DurationMinutes { get; set; }

        [Range(0, 21, ErrorMessage = "Age must be between 0 and 21")]
        [Display(Name = "Min Age")]
        public byte? AllowedMinAge { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [Display(Name = "Poster URL")]
        public string? PosterUrl { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [Display(Name = "Trailer URL")]
        public string? TrailerUrl { get; set; }

        // --- Dropdowns (Nullable, so they are optional) ---

        [Display(Name = "Producer")]
        public int? ProducerId { get; set; }

        [Display(Name = "Companies")]
        public List<int> SelectedCompanyIds { get; set; } = new List<int>();

        [Display(Name = "Genres")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        [Display(Name = "Actors")]
        public List<int> SelectedActorIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? ProducersList { get; set; }
        public IEnumerable<SelectListItem>? CompaniesList { get; set; }
        public IEnumerable<SelectListItem>? GenresList { get; set; }
        public IEnumerable<SelectListItem>? ActorsList { get; set; }
    }
}
