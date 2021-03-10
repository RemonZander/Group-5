using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public abstract class Naam_verzendkosten : type_BTW_prijs
    {
        public string Item_naam
        {
            get => default;
            set
            {
            }
        }

        public double Verzendkosten
        {
            get => default;
            set
            {
            }
        }
    }
}