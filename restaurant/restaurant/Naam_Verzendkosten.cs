using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public abstract class Naam_Verzendkosten : Type_BTW_Prijs
    {
        public string item_Naam { get; set; }

        public double verzendkosten { get; set; }

        public DateTime datum { get; set; }
    }
}