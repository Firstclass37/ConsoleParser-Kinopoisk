using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;
using System.IO;

namespace ConsoleParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser par = new Parser();
            ///Указать путь к ~/Content/Poster асп сайта, иначе фоток на сайте не будет:)
            par.PathPoster = "D:\\IT\\Projects\\ConsoleParser\\SimpleAspApp\\Content\\Posters";
            par.Pars();
        }

    }
}
