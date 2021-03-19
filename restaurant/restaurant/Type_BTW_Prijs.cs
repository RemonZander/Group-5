using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public abstract class Type_BTW_Prijs
    {
        public double BTW { get; set; }

        public int ID { get; set; }

        public double prijs { get; set; }

        public string type { get; set; }
    }
}