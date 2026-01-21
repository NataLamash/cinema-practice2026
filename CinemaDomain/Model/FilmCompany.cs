using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class FilmCompany
    {
        public int FilmId { get; set; }
        public Film Film { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }

}
