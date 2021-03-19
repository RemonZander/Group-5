using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Klantgegevens
    {
        public string achternaam { get; set; }

        public adres adres { get; set; }

        public int klantnummer { get; set; }

        public int leeftijd { get; set; }

        public Login_gegevens login_gegevens { get; set; }

        public long[] telefoonnummer { get; set; }

        public string voornaam { get; set; }
    }
}