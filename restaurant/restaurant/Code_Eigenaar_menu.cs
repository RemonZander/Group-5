using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Eigenaar_menu
    {
        private Database database = new Database();
        Reserveringen[] remon = new Reserveringen[100];
        public void fillReservations()
        {
            for (int i = 0; i < 100; i++)
            {
                remon[i].datum = new DateTime(2000-i, 1, 14);
            }

            database.reserveringen = remon;
        }
        public string sayHello()
        {
            string s = "";
            foreach (var item in database.reserveringen)
            {
                s += item;
                s += Environment.NewLine;
            }
            return s;
        }
    }
}