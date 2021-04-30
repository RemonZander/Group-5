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
        public void getGereserveerdeTafels()
        {

        }

        // zie Reservering_beschikbaarheid() in IO.cs
        

        // Medewerkers moeten een reservering kunnen koppelen aan een tafel
        public void tafelKoppelen()
        {

        }
    }

    public partial class Code_Eigenaar_menu
    {
        #region Inkomsten_en_Uitgaven
        public double Winst_Verlies() // Mogelijkheid een bepaald tijdspan te selecteren om van een bepaalde tijd de winst of verlies te kunnen zien.
        {
            double WinstVerlies;
            return WinstVerlies;
        }


        public double Uitgaven() // Mogelijkheid een bepaald tijdspan te selecteren om van een bepaalde tijd de uitgaven te kunnen zien.
        {
            double uitgaven;
            return uitgaven;
        }
        

        public double Inkomsten() // Mogelijkheid een bepaald tijdspan te selecteren om van een bepaalde tijd de inkomsten te kunnen zien.
        {
            double inkomsten;
            foreach (var bestelling in database.inkomsten.bestelling_Reservering) 
	        {
                inkomsten += bestelling.prijs;
	        }
            return inkomsten;
        }
        #endregion
    }
}