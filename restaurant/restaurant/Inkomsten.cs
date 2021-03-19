using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Inkomsten
    {
        public int[,] overig { get; set; }

        public Bestelling_reservering[] bestelling_reservering { get; set; }
    }
}