using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Repository;

namespace SimpleAspApp.Controllers
{
    public class MainController : Controller
    {
        DbRepository repo = new DbRepository();

        [HttpGet]
        public ActionResult Index(string country = "", int year = 0, string producer = "", string genre = "")
        {
            ViewBag.Tittle = "Films Info";            
            var filmsInfos = repo.SearchBy(country, year, producer, genre);
            return View(filmsInfos);
        }

        [HttpGet]
        public ActionResult About(int id)
        {
            var film = repo.SearchById(id);
            ViewBag.Tittle = film.Name;
            return View(film);
        }

        
    }
}