using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Uitgaven
    {
        public List<Inboedel> inboedel { get; set; }

        public List<Tuple<double, DateTime>> eigenaar { get; set; }

        public List<Tuple<int, DateTime>> werknemer { get; set; }
    }
}