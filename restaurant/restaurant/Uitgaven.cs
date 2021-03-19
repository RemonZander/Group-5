using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Uitgaven
    {
        public restaurant.Inboedel[] inboedel { get; set; }

        public restaurant.Eigenaar[] eigenaar { get; set; }

        public restaurant.Ingredienten[] ingredienten { get; set; }

        public restaurant.Werknemer[] werknemer { get; set; }

        public int[,] overig { get; set; }
    }
}