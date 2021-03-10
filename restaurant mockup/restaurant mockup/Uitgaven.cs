using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public struct Uitgaven
    {
        public Eigenaar[] Eigenaar { get; set; }

        public Medewerker[] Medewerker { get; set; }

        public Inboedel[] Inboedel { get; set; }

        public Ingredienten[] Ingredienten { get; set; }

        public int[,] Overig { get; set; }
    }
}