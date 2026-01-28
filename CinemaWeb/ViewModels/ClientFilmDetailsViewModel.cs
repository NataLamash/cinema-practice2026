using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaWeb.ViewModels // Або твій namespace
{
    public class ClientFilmDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public byte? AllowedMinAge { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ProducerName { get; set; }

        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Actors { get; set; } = new List<string>();


        //parse trailer url to embed url for the youtube
        public string TrailerEmbedUrl
        {
            get
            {
                if (string.IsNullOrEmpty(TrailerUrl)) return null;

                var uri = new Uri(TrailerUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];

                if (string.IsNullOrEmpty(videoId) && uri.Host.Contains("youtu.be"))
                {
                    videoId = uri.Segments.Last();
                }

                return !string.IsNullOrEmpty(videoId)
                    ? $"https://www.youtube.com/embed/{videoId}"
                    : null;
            }
        }
    }
}
