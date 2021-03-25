using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Database
    {
        public Menukaart menukaart { get; set; }

        public List<Reserveringen> reserveringen { get; set; }

        public List<Klantgegevens> klantgegevens { get; set; }

        public restaurant.Uitgaven uitgaven { get; set; }

        public restaurant.Inkomsten inkomsten { get; set; }
    }
}