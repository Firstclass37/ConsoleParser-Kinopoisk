using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HtmlAgilityPack;
using RestSharp;
using Repository;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using RestSharp.Deserializers;


namespace ConsoleParser
{
    public class Parser
    {
        /// <summary>
        /// Репозиторий
        /// </summary>
        private DbRepository repo = new DbRepository();
        /// <summary>
        /// Входная точка в программу
        /// </summary>
        public void Pars()
        {
            int i = 1;
            while (i < 5)
            {
                var doc = GetPageHtml(i);
                ParsePage(doc);
                i++;
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
                var client = new RestClient("https://www.kinopoisk.ru");
                var request = new RestRequest(requestUrl, Method.GET);
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);
                return doc;
        }
        /// <summary>
        /// Принимает ссылку на фильм и возвращает результат (десериализованный объект Json)
        /// </summary>
        /// <param name="filmUrl">ссылка на страницу с фильмом</param>
        /// <returns></returns>
        private Dictionary<string, Object> GetFilmInfoDictionary(string filmUrl)
        {
            int id = GetIdFromUrl(filmUrl);
            var requestUrl = "getFilm?filmID=" + id.ToString();
            var client = new RestClient("http://api.kinopoisk.cf/");
            var request = new RestRequest(requestUrl, Method.GET);
            IRestResponse response = client.Execute(request);
            JsonDeserializer temp = new JsonDeserializer();
            var result = temp.Deserialize<Dictionary<string,Object>>(response);
            return result;
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
                string href = node.Attributes["href"].Value;
                Film currentFilfInfo = GetFilmInfo(href);
                SaveInDb(currentFilfInfo);
            }
        }
        /// <summary>
        /// Получает все имеющуюся информацию о фильме и возвращает результат
        /// </summary>
        /// <param name="href">Ссылка на страницу</param>
        /// <returns></returns>
        private Film GetFilmInfo(string href)
        {
            Film info = new Film();
            var dictionaryInfo = GetFilmInfoDictionary(href);
            foreach (var item in dictionaryInfo)
            {
                switch (item.Key)
                {
                    case "nameRU":
                        info.Name = item.Value.ToString().Trim();
                        break;
                    case "country":
                        info.Country = item.Value.ToString().Trim();
                        break;
                    case "genre":
                        info.Genres = item.Value.ToString().Split(',').Select(g=>new Genre() {Name = g.Trim()}).ToList();
                        break;
                    case "year":                       
                        info.Year = GetYear(item.Value.ToString());
                        break;
                    case "description":
                        info.Description =  item.Value.ToString().Trim();
                        break;
                    case "creators":
                        info.Producers = GetProducers(item.Value);
                        break;
                    case "posterURL":
                        info.PosterUrl = GetPosterUrl(item.Value.ToString());
                        break;
                    default:
                        continue;                       
                }
            }
            return info;
        }
        /// <summary>
        /// Возвращает корректный URL к постеру фильма
        /// </summary>
        /// <param name="filmId">ID фильма</param>
        /// <returns></returns>
        private string GetPosterUrl(string filmId)
        {
            return "https://st.kp.yandex.net/images/film_iphone/iphone360" + "_" + GetIdFromUrl(filmId) + ".jpg";
        }
        /// <summary>
        /// Возвращает список продюссеров к текущему фильму
        /// </summary>
        /// <param name="creaters">Словарь с создателями</param>
        /// <returns></returns>
        private List<Producer> GetProducers(object creaters)
        {
            List<Producer> producersList = new List<Producer>();
            var creatorsList = (creaters as List<object>);
            if (creatorsList != null)
            {
                foreach (var creat in creatorsList)
                {                    
                    var producers = creat as IList<object>;
                    if (producers != null)
                    {
                        foreach (var prod in producers)
                        {
                            var producer = prod as IDictionary<string, object>;
                            if (producer != null)
                            {
                                if (producer["professionText"].ToString() == "Режиссеры")
                                {
                                    producersList.Add(new Producer() {Name = producer["nameRU"].ToString().Trim() });
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }

            }
            return producersList;

        }
        /// <summary>
        /// Возвращает ID фильма из url
        /// </summary>
        /// <param name="url">URL фильма</param>
        /// <returns></returns>
        private int GetIdFromUrl(string url)
        {
            int id;
            Regex reg = new Regex("[0-9]{2,}");
            string idString = reg.Match(url).Value;
            int.TryParse(idString, out id);
            return id;
        }
        /// <summary>
        /// Возвращает первое значение года из входящей строки
        /// </summary>
        /// <param name="yearLine">Строка содержащая значение года</param>
        /// <returns></returns>
        private int GetYear(string yearLine)
        {
            int year;
            Regex reg = new Regex("[0-9]{4}");
            string yearString = reg.Match(yearLine).Value;
            int.TryParse(yearString, out year);
            return year;
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
