using CinemaDomain.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
    public class FilmFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Це обов'язкове поле.")]
        [Display(Name = "Назва")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 50 символів.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Опис має містити від 2 до 500 символів.")]
        public string? Description { get; set; }

        [Display(Name = "Дата виходу")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Required(ErrorMessage = "Це обов'язкове поле.")]
        [Range(1, 1000, ErrorMessage = "Тривалість має перевищувати 0.")]
        [Display(Name = "Тривалість (хв)")]
        public short DurationMinutes { get; set; }

        [Range(0, 21, ErrorMessage = "Вік має бути від 0 до 21.")]
        [Display(Name = "Вікове обмеження")]
        public byte? AllowedMinAge { get; set; }

        [Url(ErrorMessage = "Хибний формат URL")]
        [Display(Name = "URL постера")]
        public string? PosterUrl { get; set; }

        [Display(Name = "Завантажити постер")]
        public IFormFile? PosterFile { get; set; }

        [Url(ErrorMessage = "Хибний формат URL")]
        [Display(Name = "URL трейлера")]
        public string? TrailerUrl { get; set; }


        [Display(Name = "Продюсер")]
        public int? ProducerId { get; set; }

        [Display(Name = "Компанії")]
        public List<int>? SelectedCompanyIds { get; set; } = new List<int>();

        [Display(Name = "Жанри")]
        public List<int>? SelectedGenreIds { get; set; } = new List<int>();

        [Display(Name = "Актори")]
        public List<int>? SelectedActorIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? ProducersList { get; set; }
        public IEnumerable<SelectListItem>? CompaniesList { get; set; }
        public IEnumerable<SelectListItem>? GenresList { get; set; }
        public IEnumerable<SelectListItem>? ActorsList { get; set; }
    }
}
