using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Inboedel : Naam_verzendkosten
    {
        public Inboedel()
        {
            Type = "Inboedel";
            is_uitgave = true;
        }

        public Afzender_gegevens Afzender_gegevens
        {
            get => default;
            set
            {
            }
        }
    }
}