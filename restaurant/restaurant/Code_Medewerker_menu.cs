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


        public double Uitgaven(DateTime beginDatum, DateTime eindDatum) // Mogelijkheid een bepaald tijdsspan in te voeren en daarvan de uitgaven te kunnen zien.
        {
            double uitgaven = 0;

            /*public double GetUitgavenInboedel() // Inboedel
            {
                double uitgavenInboedel = 0;
                return uitgavenInboedel;
            }

            public double GetUitgavenEigenaar() // Eigenaar
            {
                double uitgavenEigenaar = 0;
                foreach (var uitgave in database.uitgaven.eigenaar_ID)
                {
                    uitgavenEigenaar += uitgave;
                }
                return uitgavenEigenaar;
            }

            public double GetUitgavenIngredienten() // Ingrediënten
            {
                double uitgavenIngredienten = 0;
                foreach (var uitgave in database.uitgaven.ingredienten_ID)
                {
                    uitgavenIngredienten += uitgave;
                }
                return uitgavenIngredienten;
            }

            public double GetUitgavenWerknemers()
            {
                double uitgavenWerknemers = 0;
                foreach (var uitgave in database.uitgaven.werknemer_ID)
                {
                    uitgavenWerknemers += uitgave;
                }
                return uitgavenWerknemers;
            }

            public double GetUitgavenOverig()
            {
                double uitgavenOverig = 0;
                return uitgavenOverig;
            }*/

            return uitgaven;
        }

        public double Inkomsten(DateTime beginDatum, DateTime eindDatum) // Mogelijkheid een bepaald tijdsspan in te voeren en daarvan de inkomsten te kunnen zien.
        {
            double inkomsten = 0;
            foreach (var bestelling in database.inkomsten.bestelling_reservering)
            {
                foreach (var reservering in database.reserveringen)
                {
                    if (reservering.ID == bestelling.reservering_ID && beginDatum <= reservering.datum && reservering.datum <= eindDatum)
                    {
                        inkomsten += bestelling.prijs;
                        break;
                    }
                }
            }
            return inkomsten;
        }
        
        
        #endregion
    } 
}