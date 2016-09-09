using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class Producer
    {
        public int ProducerId { get; set; }
        public string Name { get; set; }

        public virtual List<Film> Films { get; set; }
    }
}
