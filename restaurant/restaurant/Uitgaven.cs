using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Uitgaven
    {
        public List<Inboedel> inboedel { get; set; }

        public List<Tuple<int, DateTime>> eigenaar { get; set; }

        public List<Tuple<int, DateTime>> ingredienten { get; set; }

        public List<Tuple<int, DateTime>> werknemer { get; set; }

        public int[,] overig { get; set; }
    }
}