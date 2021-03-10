using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Bestelling_reservering : type_BTW_prijs
    {
        public Bestelling_reservering(string Type)
        {
            this.Type = Type;
            is_uitgave = false;
        }

        public Klantgegevens Klantgegevens
        {
            get => default;
            set
            {
            }
        }

        public double Fooi
        {
            get => default;
            set
            {
            }
        }

        public int Korting
        {
            get => default;
            set
            {
            }
        }
    }
}