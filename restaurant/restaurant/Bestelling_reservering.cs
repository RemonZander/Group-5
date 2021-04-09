using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Bestelling_reservering : Type_BTW_Prijs
    {
        public double fooi { get; set; }

        public int korting { get; set; }

        public int reservering_ID { get; set; }
    }
}