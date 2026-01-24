using CinemaDomain.Model;
using CinemaInfrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
    public class FilmFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва фільму обов'язкова")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Display(Name = "Дата виходу")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Тривалість має бути більше 0")]
        [Display(Name = "Тривалість (хв)")]
        public short DurationMinutes { get; set; }

        [Range(0, 21, ErrorMessage = "Вік має бути від 0 до 21")]
        [Display(Name = "Мін. вік")]
        public byte? AllowedMinAge { get; set; }

        [Url(ErrorMessage = "Некоректне посилання")]
        [Display(Name = "URL Постера")]
        public string? PosterUrl { get; set; }

        [Url(ErrorMessage = "Некоректне посилання")]
        [Display(Name = "URL Трейлера")]
        public string? TrailerUrl { get; set; }

        // --- Зв'язки (Dropdowns) ---

        [Display(Name = "Продюсер")]
        public int? ProducerId { get; set; }

        [Display(Name = "Компанії")]
        public List<int> SelectedCompanyIds { get; set; } = new List<int>();

        [Display(Name = "Жанри")]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        [Display(Name = "Актори")]
        public List<int> SelectedActorIds { get; set; } = new List<int>();

        // --- Дані для заповнення списків у View ---
        public IEnumerable<SelectListItem>? ProducersList { get; set; }
        public IEnumerable<SelectListItem>? CompaniesList { get; set; }
        public IEnumerable<SelectListItem>? GenresList { get; set; }
        public IEnumerable<SelectListItem>? ActorsList { get; set; }
    }
}
