using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Gerechten
    {
        public string naam { get; set; }

        public int ID { get; set; }

        [Obsolete("Gebruik de List<string> aub")]
        public List<int> ingredienten { get; set; }

        public List<string> Ingredienten { get; set; }

        public bool is_populair { get; set; }

        public double prijs { get; set; }

        public bool special { get; set; }

        public bool is_gearchiveerd { get; set; }

        public List<string> allergenen { get; set; }

        public bool diner { get; set; }

        public bool lunch { get; set; }

        public bool ontbijt { get; set; }

        public bool dessert { get; set; }
    }
}