using System;
using System.Data.Entity;

namespace Repository
{
    internal class EfDbContext:DbContext
    {
        public DbSet<Film> FilmsInfos { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Producer> Producers { get; set; }
    }
}
