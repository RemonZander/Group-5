using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public struct Inkomsten
    {
        public Bestelling_reservering[] Bestelling_reservering { get; set; }

        public int[,] Overig { get; set; }
    }
}