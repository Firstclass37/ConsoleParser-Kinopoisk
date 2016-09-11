using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repository
{
    public class Film
    {
        [Key]
        public int FilmId { get; set; }

        public int IdOnSite { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public string Country { get; set; }
        public virtual List<Producer> Producers { get; set; }
        public virtual List<Genre> Genres { get; set; }
        public string PosterUrl { get; set; }
    }
}
