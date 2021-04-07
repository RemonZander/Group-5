using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Medewerker_menu
    {
        private IO io = new IO();

        private Database database = new Database();

        public Code_Medewerker_menu()
        {
            database = io.Getdatabase();
        }

        public void Debug()
        {

        }

        // Medewerker moet huidige reserveringen kunnen zien
        public void getReserveringen()
        {

        }

        // Medewerkers moeten kunnen zien welke tafels al gereserveerd zijn (en welke niet)
        // zie Reservering_beschikbaarheid() in Code_Gebruiker_menu.cs

        // Medewerkers moeten een reservering kunnen koppelen aan een tafel
        public void tafelKoppelen()
        {

        }
    }
}