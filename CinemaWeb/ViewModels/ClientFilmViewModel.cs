using CinemaDomain.Model;

namespace CinemaWeb.ViewModels
{
    public class ClientFilmViewModel
    {
        public List<Film> ActualFilms { get; set; } = new List<Film>();
        public List<Film> ExpectedFilms { get; set; } = new List<Film>();

        public string SearchTerm { get; set; }
    }
}
