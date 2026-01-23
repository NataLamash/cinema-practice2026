using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class FilmActor
    {
        public int FilmId { get; set; }
        public int ActorId { get; set; }

        public string? CharacterName { get; set; }

        public Film? Film { get; set; }
        public Actor? Actor { get; set; }
    }

}
