using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Ingredienten : Naam_verzendkosten
    {
        public Ingredienten()
        {
            Type = "Ingredienten";
            is_uitgave = true;
        }

        public Afzender_gegevens Afzender_gegenvens
        {
            get => default;
            set
            {
            }
        }
    }
}