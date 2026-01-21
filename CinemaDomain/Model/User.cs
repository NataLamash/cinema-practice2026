using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDomain.Model
{
    public class User
    {
        public int Id { get; set; }
        public string AzureIdentityId { get; set; }
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Navigation
        public ICollection<Order> Orders { get; set; }
        public ICollection<FilmRating> FilmRatings { get; set; }
    }

}
