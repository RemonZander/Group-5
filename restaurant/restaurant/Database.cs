using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Database
    {
        public Database()
        {
            //tafels defineren, hier worden alle zetels toegewezen
            for (int i = 0; i < 100; i++)
            {
                Tafels[i, 0] = i;
                Tafels[i, 1] = 4;
            }
        }

        public Menukaart menukaart { get; set; }

        public List<Reserveringen> reserveringen { get; set; }

        public List<Klantgegevens> klantgegevens { get; set; }

        public restaurant.Uitgaven uitgaven { get; set; }

        public restaurant.Inkomsten inkomsten { get; set; }

        //positie 1 is ID en positie 2 is aantal zetels per tafel
        public int[,] Tafels = new int[100,2];
    }
}