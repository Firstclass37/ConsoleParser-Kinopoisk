using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using RestSharp;
using Repository;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using RestSharp.Extensions.MonoHttp;

namespace ConsoleParser
{
    public class Parser
    {
        /// <summary>
        /// Указать папку для сохранения постеров. По умолчанию в текущем каталоге создает папку Posters и загружает туда
        /// </summary>
        public string PathPoster { get; set; }
        /// <summary>
        /// Клиент
        /// </summary>
        private RestClient client = new RestClient("https://www.kinopoisk.ru");
        /// <summary>
        /// Репозиторий
        /// </summary>
        private DbRepository repo = new DbRepository();
        public Parser()
        {
            PathPoster = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory("Posters").FullName;
        }
        /// <summary>
        /// Входная точка в программу
        /// </summary>
        public void Pars()
        {
            int i = 1;
            while (i < 5)
            {
                try
                {
                    var doc = GetPageHtml(i);
                    ParsePage(doc);
                    i++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
        /// <summary>
        /// Возвращает страницу с фильмами
        /// </summary>
        /// <param name="pageNum">Номер страницы</param>
        /// <returns></returns>
        public HtmlDocument GetPageHtml(int pageNum)
        {
            string requestUrl = string.Format("lists/ord/rating_kp/m_act%5Bcountry%5D/1/m_act%5Ball%5D/ok/page/{0}/perpage/200/", pageNum);
            client.Proxy = GetProxy(pageNum);
            var request = new RestRequest(requestUrl, Method.GET);
            IRestResponse response = client.Execute(request);
            var content = (Encoding.GetEncoding(1251)).GetString(response.RawBytes);
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(content);
            return doc;
        }
        /// <summary>
        /// Возвращает страничку с информацией о фильме
        /// </summary>
        /// <param name="url">url странички</param>
        /// <returns></returns>
        private HtmlDocument GetInfoHtml(string url)
        {
            string requestUrl = url.Remove(url.IndexOf("/"), 1);
            var request = new RestRequest(requestUrl, Method.GET);
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Accept-Language", "ru - RU, ru; q = 0.8,en - US; q = 0.5,en; q = 0.3");
            request.AddHeader("Accept-Charset", "Windows-1252,utf-8;q=0.7,*;q=0.7");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:48.0) Gecko/20100101 Firefox/48.0");
            IRestResponse response = client.Execute(request);
            var content = (Encoding.GetEncoding(1251)).GetString(response.RawBytes);
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(content);
            return doc;
        }
        /// <summary>
        /// Парсит заданную страницу и сохраняет результат в БД
        /// </summary>
        /// <param name="doc">Страница HTML</param>
        public void ParsePage(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//div[@class='item _NO_HIGHLIGHT_']//div[@class='info']//div[@class='name']//a");

            foreach (var node in nodes)
            {
                try
                {
                    string href = node.Attributes["href"].Value;
                    HtmlDocument cuFilm = GetInfoHtml(href);
                    int filmId = GetIdFromUrl(href);
                    Film currentFilfInfo = GetFilmInfo(cuFilm, filmId);
                    SaveInDb(currentFilfInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }
        /// <summary>
        /// Парсит информацию о фильме со страничкт и возвращает результат
        /// </summary>
        /// <param name="filmHtml">Страничка с информацией</param>
        /// <param name="filmId">ID фильма на сайте</param>
        /// <returns></returns>
        private Film GetFilmInfo(HtmlDocument filmHtml, int filmId)
        {
            Film film = new Film();
            film.IdOnSite = filmId;
            film.Name = GetName(filmHtml);
            film.PosterUrl = GetLocalUrl(filmHtml, filmId);
            film.Year = GetYear(filmHtml);
            film.Description = GetDescription(filmHtml);
            film.Country = GetCounrty(filmHtml);
            film.Genres = GetGenres(filmHtml);
            film.Producers = GetProducers(filmHtml);
            return film;
        }
        /// <summary>
        /// Парсит имя
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns></returns>
        private string GetName(HtmlDocument filmHtml)
        {
            string name = string.Empty;
            var descriptionNode = filmHtml.DocumentNode.SelectSingleNode("//div[@id = 'headerFilm']//h1");
            if (descriptionNode != null)
            {
                name = HttpUtility.HtmlDecode(descriptionNode.InnerText);
            }
            return !string.IsNullOrEmpty(name) ? name : "No name";
        }
        /// <summary>
        /// Парсит страну
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns>Название страны</returns>
        private string GetCounrty(HtmlDocument filmHtml)
        {
            string country = string.Empty;
            var countryNode = filmHtml.DocumentNode.SelectSingleNode("// div[@id = 'infoTable'] // table // tr [td = 'страна'] // a");
            if (countryNode != null)
            {
                country = countryNode.InnerText;
            }
            return !string.IsNullOrEmpty(country) ? country : "No coutry";
        }
        /// <summary>
        /// Парсит год
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns>год</returns>
        private int GetYear(HtmlDocument filmHtml)
        {
            int year = 0;
            var yearNode = filmHtml.DocumentNode.SelectSingleNode("// div[@id = 'infoTable'] //table// tr [td = 'год'] // a");
            if (yearNode != null)
            {
                int.TryParse(yearNode.InnerText, out year);
            }
            return year;
        }
        /// <summary>
        /// Парсит описание
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns>Описание</returns>
        private string GetDescription(HtmlDocument filmHtml)
        {
            string description = string.Empty;
            var descrNode = filmHtml.DocumentNode.SelectSingleNode("// div[@itemprop='description']");
            if (descrNode != null)
            {
                description = HttpUtility.HtmlDecode(descrNode.InnerText);
            }
            return !string.IsNullOrEmpty(description) ? description : "No description";
        }
        /// <summary>
        /// Парсит список продюсеров
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns>Список продюсеров</returns>
        private List<Producer> GetProducers(HtmlDocument filmHtml)
        {
            List<Producer> producers = new List<Producer>();
            var producersNodes = filmHtml.DocumentNode.SelectNodes("// div[@id = 'infoTable'] // table // tr [td = 'продюсер'] // td[@itemprop='producer'] //a");
            if (producersNodes != null)
            {
                foreach (var node in producersNodes)
                {
                    producers.Add(new Producer() { Name = node.InnerText });
                }
            }
            return producers;
        }
        /// <summary>
        /// Парсит список жанров
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <returns>Список жанров</returns>
        private List<Genre> GetGenres(HtmlDocument filmHtml)
        {
            List<Genre> genres = new List<Genre>();
            var genresNodes = filmHtml.DocumentNode.SelectNodes("// div[@id = 'infoTable'] // table // tr [td = 'жанр'] // *[@itemprop='genre'] //a");
            if (genresNodes != null)
            {
                foreach (var node in genresNodes)
                {
                    genres.Add(new Genre() { Name = node.InnerText });
                }
            }
            return genres;
        }
        /// <summary>
        /// Получает ID фильма из URL
        /// </summary>
        /// <param name="url">URL странички с фильмом</param>
        /// <returns>ID фильма</returns>
        private int GetIdFromUrl(string url)
        {
            int id;
            Regex reg = new Regex("[0-9]{2,}");
            string idString = reg.Match(url).Value;
            int.TryParse(idString, out id);
            return id;
        }
        /// <summary>
        /// Скачивает изображение, если оно есть, сохраняет на диск и возвращает путь к нему
        /// </summary>
        /// <param name="filmHtml">Страничка с фильмом</param>
        /// <param name="filmId">ID фильма на сайте</param>
        /// <returns>Путь к изображению</returns>
        private string GetLocalUrl(HtmlDocument filmHtml, int filmId)
        {
            string localAddress = string.Empty;
            string posterUrl = string.Empty;
            //xpath не находит тег нужный тег с картинкой, хотя он есть. Поэтому ниже вызывается метод VeloPars... который парсит сам 
            var temp = filmHtml.DocumentNode.SelectSingleNode("//div[@class='film-img-box']");
            if (temp != null)
            {
                string badHtml = temp.InnerHtml;
                posterUrl = VeloParsPosterUrl(badHtml);
                if (string.IsNullOrEmpty(posterUrl))
                {
                    ///А в этом случае все работает)
                    temp = filmHtml.DocumentNode.SelectSingleNode("//div[@class='film-img-box']//img");
                    if (temp != null)
                    {
                        posterUrl = temp.GetAttributeValue("src", "");
                    }                                   
                }
            }

            if (!string.IsNullOrEmpty(posterUrl))
            {
                localAddress = SavePosterAndGetAddress(posterUrl, filmId);
            }
            return localAddress;
        }
        /// <summary>
        /// Скачивает постер , сохраняет на диск и возвращает путь к нему
        /// </summary>
        /// <param name="url">Ссылка на изображение</param>
        /// <param name="id">ID фильма на сайте</param>
        /// <returns></returns>
        private string SavePosterAndGetAddress(string url, int id)
        {
            ///У многих картинок путь относительный, но у некоторых путь полный. Этим обусловлен if-else
            byte[] poster;
            if (url.Contains("https"))
            {
                var newClient = new RestClient(url);
                var newRequest = new RestRequest();
                poster = newClient.DownloadData(newRequest);
            }
            else
            {
                var request = new RestRequest(url);
                poster = client.DownloadData(request);
            }
            
            var targetPath = PathPoster + string.Format("\\{0}.jpg", id);
            File.WriteAllBytes(targetPath, poster);
            return targetPath;
        }
        /// <summary>
        /// Регулярным выражением парсит разметку, содержащую ссылку на изображение
        /// </summary>
        /// <param name="htmlString">Html код, содержащий ссылку</param>
        /// <returns></returns>
        private string VeloParsPosterUrl(string htmlString)
        {
            Regex reg = new Regex("['].*?[']");
            string urlString = reg.Match(htmlString).Value.Replace("'", "");
            return urlString;
        }
        /// <summary>
        /// Получает прокси (свой для каждой странички)
        /// </summary>
        /// <param name="page">Номер странички</param>
        /// <returns></returns>
        private WebProxy GetProxy(int page)
        {
            WebProxy proxy = new WebProxy();
            switch (page)
            {
                case 1:
                    proxy.Address = new Uri("http://217.61.0.107:3128");
                    break;
                case 2:
                    proxy.Address = new Uri("http://91.144.189.82:3128");
                    break;
                case 3:
                    proxy.Address = new Uri("http://86.62.71.41:3128");
                    break;
                case 4:
                    proxy.Address = new Uri("http://94.20.21.38:3128");
                    break;
            }
            return proxy;
        }
        /// <summary>
        /// Сохраняет иформацию о фильме в БД
        /// </summary>
        /// <param name="film">Экземпляр информации о фильме</param>
        private void SaveInDb(Film film)
        {
            repo.SaveInfo(film);
        }
    }
}
