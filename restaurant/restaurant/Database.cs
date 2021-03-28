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
            List<Tafels> temp = new List<Tafels>();
            for (int i = 0; i < 100; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            tafels = temp;
        }

        public Menukaart menukaart { get; set; }

        public List<Reserveringen> reserveringen { get; set; }

        public List<Klantgegevens> klantgegevens { get; set; }

        public restaurant.Uitgaven uitgaven { get; set; }

        public restaurant.Inkomsten inkomsten { get; set; }

        public List<Tafels> tafels { get; set; }

        public List<Review> reviews { get; set; }

        public List<Feedback> feedback { get; set; }
    }
}