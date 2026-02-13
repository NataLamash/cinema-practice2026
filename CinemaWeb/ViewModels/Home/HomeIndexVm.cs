using System;
using System.Collections.Generic;

namespace CinemaWeb.ViewModels.Home
{
    public class HomeIndexVm
    {
        public List<FilmCardVm> ActualFilms { get; set; } = new(); // "Актуальні" / Нові
        public List<FilmCardVm> TodayFilms { get; set; } = new();  // "Сьогодні в кіно"
    }

    public class FilmCardVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? PosterUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
