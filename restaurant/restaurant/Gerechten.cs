using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Gerechten
    {
        public string naam { get; set; }

        public int ID { get; set; }

        public List<string> ingredienten { get; set; }

        public bool is_populair { get; set; }

        public double prijs { get; set; }

        public bool special { get; set; }

        public bool is_gearchiveerd { get; set; }
    }
}