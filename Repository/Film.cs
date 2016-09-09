using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class Film
    {
        public int FilmId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public string Country { get; set; }
        public virtual List<Producer> Producers { get; set; }
        public virtual List<Genre> Genres { get; set; }
        public string PosterUrl { get; set; }
    }
}
