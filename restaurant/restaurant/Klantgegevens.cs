using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Klantgegevens
    {
        public string voornaam { get; set; }

        public string achternaam { get; set; }

        public adres adres { get; set; }

        public int klantnummer { get; set; }

        public DateTime geb_datum { get; set; }

        public Login_gegevens login_gegevens { get; set; }

        public List<long> telefoonnummer { get; set; }
    }
}