using System;
using System.Collections.Generic;


namespace Repository
{
    public class Producer
    {
        public int ProducerId { get; set; }
        public string Name { get; set; }

        public virtual List<Film> Films { get; set; }
    }
}
