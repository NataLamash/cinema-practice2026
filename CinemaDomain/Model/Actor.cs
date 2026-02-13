using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class Actor
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? PhotoUrl { get; set; }

        public ICollection<FilmActor> FilmActors { get; set; } = new List<FilmActor>();
    }

}
