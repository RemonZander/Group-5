using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Ingredient
    {
        public int ID { get; set; }

        public string name { get; set; }

        public DateTime bestel_datum { get; set; }

        public DateTime houdbaarheids_datum { get; set; }

    }
}