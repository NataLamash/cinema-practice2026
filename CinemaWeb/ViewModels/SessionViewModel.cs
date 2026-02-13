using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
	public class SessionViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Будь ласка, оберіть фільм із списку")]
		[Display(Name = "Фільм")]
		public int? FilmId { get; set; }

		[Required(ErrorMessage = "Будь ласка, оберіть зал")]
		[Display(Name = "Кінозал")]
		public int? HallId { get; set; }

		[Required(ErrorMessage = "Вкажіть дату та час початку сеансу")]
		[Display(Name = "Початок сеансу")]
		[DataType(DataType.DateTime)]
		public DateTime StartTime { get; set; }

		[Required(ErrorMessage = "Вкажіть дату та час закінчення сеансу")]
		[Display(Name = "Завершення сеансу")]
		[DataType(DataType.DateTime)]
		public DateTime EndTime { get; set; }

		[Required(ErrorMessage = "Вкажіть базову вартість квитка")]
		[Range(50, 10000, ErrorMessage = "Ціна повинна бути в межах від 50 до 10 000 грн")]
		[Display(Name = "Базова ціна (грн)")]
		public decimal BasePrice { get; set; }

		public IEnumerable<SelectListItem>? Films { get; set; }
		public IEnumerable<SelectListItem>? Halls { get; set; }
	}
}
