using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Uitgaven
    {
        public List<Inboedel> inboedel { get; set; }

        public List<int> eigenaar_ID { get; set; }

        public List<Ingredienten> ingredienten { get; set; }

        public List<int> werknemer_ID { get; set; }

        public int[,] overig { get; set; }
    }
}