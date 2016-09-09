using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;

namespace ConsoleParser
{
    class Program
    {
        static void Main(string[] args)
        {
            DbRepository repo = new DbRepository();
            repo.ClearDb();
            Parser par = new Parser();
            par.Pars();
        }
    }
}
