using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;


namespace Repository
{
    public class DbRepository
    {
        /// <summary>
        /// Контекст
        /// </summary>
        private readonly EfDbContext context = new EfDbContext();
        /// <summary>
        /// Список всех фильмов
        /// </summary>
        public IEnumerable<Film> FilmsInfos
        {
            get
            {
                return context.FilmsInfos.ToList();
            }
        }
        /// <summary>
        /// Список всех жанров
        /// </summary>
        public IEnumerable<Genre> Genres
        {
            get
            {
                return context.Genres.ToList();
            }
        }
        /// <summary>
        /// Список всех продюссеров
        /// </summary>
        public IEnumerable<Producer> Producers
        {
            get
            {
                return context.Producers.ToList();
            }
        }
        /// <summary>
        /// Сохраняет информацию о фильме в БД
        /// </summary>
        /// <param name="info">Информация о фильме</param>
        public void SaveInfo(Film info)
        {
            if (info != null)
            {
                info.Genres = GetCheckedGenres(info.Genres);
                info.Producers = GetCheckedProducers(info.Producers);

                context.FilmsInfos.Add(info);
                context.SaveChanges();
            }
        }
        /// <summary>
        /// Проверяет, существует ли такой жанр в БД. Усли да, возвращает ссылку на него
        /// </summary>
        /// <param name="genre">Жанр для проверки</param>
        /// <returns></returns>
        private Genre CheckGenre(Genre genre)
        {
            var dbgenre = context.Genres.Where(g => g.Name == genre.Name).FirstOrDefault();

            return dbgenre != null ? dbgenre : genre;
        }
        /// <summary>
        /// Проверяет список жанров
        /// </summary>
        /// <param name="genres">Список жанров</param>
        /// <returns></returns>
        private List<Genre> GetCheckedGenres(List<Genre> genres)
        {
            List<Genre> newGenres = new List<Genre>();
            foreach (var item in genres)
            {
                var temp = CheckGenre(item);
                newGenres.Add(temp);
            }
            return newGenres;

        }
        /// <summary>
        /// Проверяет, существует ли такой продюссер в Бд. Если да, то возвращает ссылку на него
        /// </summary>
        /// <param name="producer"></param>
        /// <returns></returns>
        private Producer CheckProducer(Producer producer)
        {
            var dbProducer = context.Producers.Where(p => p.Name == producer.Name).FirstOrDefault();
            return dbProducer != null ? dbProducer : producer;
        }
        /// <summary>
        /// Проверяет список продюссеров
        /// </summary>
        /// <param name="producers">Список продюссеров</param>
        /// <returns></returns>
        private List<Producer> GetCheckedProducers(List<Producer> producers)
        {
            List<Producer> checkedProducers = new List<Producer>();

            foreach (var producer in producers)
            {
                var temp = CheckProducer(producer);
                checkedProducers.Add(temp);
            }
            return checkedProducers;
        }
        /// <summary>
        /// Ищет информацию по заданным фильтрам. Возвращает результат в виде списка.Если параметр не указан, он не учитывается
        /// </summary>
        /// <param name="country">Страна</param>
        /// <param name="year">Год</param>
        /// <param name="producer">Продюссер</param>
        /// <param name="genre">Жанр</param>
        /// <returns></returns>
        public IEnumerable<Film> SearchBy(string country = "", int year = 0, string producer = "", string genre = "")
        {
            IQueryable<Film> films = context.FilmsInfos.Include("Producers").Include("Genres");

            if (country != "")
            {
                films = films.Where(r=>r.Country == country).Include("Producers").Include("Genres");
            }
            if (year != 0)
            {
                films = films.Where(r=>r.Year == year).Include("Producers").Include("Genres");
            }
            if (producer != "")
            {
                var targetProd = context.Producers.FirstOrDefault(p => p.Name == producer);
                if (targetProd != null)
                {
                    films = films
                        .Where(f => f.Producers.Select(p=>p.ProducerId).Contains(targetProd.ProducerId))
                        .Include("Producers")
                        .Include("Genres");
                }
            }
            if (genre != "")
            {
                var targetGenre = context.Genres.FirstOrDefault(g => g.Name == genre);                  
                if (targetGenre != null)
                {
                    films = films.Where(f=>f.Genres.Select(g=>g.GenreId).Contains(targetGenre.GenreId))
                        .Include("Producers")
                        .Include("Genres");
                }
            }
            return films.ToList();
        }
        /// <summary>
        /// Возвращает фильм с заданным ID
        /// </summary>
        /// <param name="filmId">ID фильма</param>
        /// <returns></returns>
        public Film SearchById(int filmId)
        {
            var film = context.FilmsInfos.Where(f => f.FilmId == filmId).Include("Genres").Include("Producers").FirstOrDefault();
            return film;           
        }        
    }
}
