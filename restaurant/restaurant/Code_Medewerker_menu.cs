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

        #region Reververingen

        public List<Reserveringen> getReserveringen() // Medewerker kan de reserveringen van de huidige dag zien
        {
            var reserveringenVandaag = new List<Reserveringen>();
            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum == DateTime.Now)
                {
                    reserveringenVandaag.Add(reservering);
                }
            }
            return reserveringenVandaag;
        }

        #endregion


        #region Tafels

        public List<Tuple<DateTime, List<Tafels>>> getBeschikbareTafels() // Medewerker kan zien welke tafels beschikbaar zijn op de huidige dag
        {
            var beschikbareTafels = io.Reservering_beschikbaarheid(DateTime.Now);
            return beschikbareTafels;
        }
        
        public List<Reserveringen> getReserveringenZonderTafel() // Returns de reserveringen die nog niet zijn gekoppeld aan een tafel
        {
            var reserveringen = getReserveringen();
            var reserveringenZonderTafel = new List<Reserveringen>();

            foreach (var reservering in reserveringen)
            {
                if (reservering.tafels == null)
                {
                    reserveringenZonderTafel.Add(reservering);
                }
            }

            return reserveringenZonderTafel;
        }

        public void tafelKoppelen() // Medewerker moet de reserveringen kunnen koppelen aan een tafel
        {
            var reserveringenZonderTafel = getReserveringenZonderTafel();
            var beschikbareTafels = getBeschikbareTafels();
        }

        #endregion
    }


    public partial class Code_Eigenaar_menu
    {
        #region Inkomsten_en_Uitgaven

        public double Uitgaven(DateTime beginDatum, DateTime eindDatum) // Mogelijkheid een bepaald tijdsspan in te voeren en daarvan de uitgaven te kunnen zien.
        {
            double uitgaven = 0;
            uitgaven += GetUitgavenEigenaar(beginDatum, eindDatum);
            uitgaven += GetUitgavenIngredienten(beginDatum, eindDatum);
            uitgaven += GetUitgavenWerknemers(beginDatum, eindDatum);
            return uitgaven;
        }

        /*public double GetUitgavenInboedel(DateTime beginDatum, DateTime eindDatum) // Inboedel
        {
            double uitgavenInboedel = 0;
            return uitgavenInboedel;
        }*/

        public double GetUitgavenEigenaar(DateTime beginDatum, DateTime eindDatum) // Eigenaar
        {
            double uitgavenEigenaar = 0;
            foreach (var uitgave in database.uitgaven.eigenaar)
            {
                if (beginDatum <= uitgave.Item2 && uitgave.Item2 <= eindDatum)
                {
                    uitgavenEigenaar += uitgave.Item1;
                    break;
                }
            }
            return uitgavenEigenaar;
        }

        public double GetUitgavenIngredienten(DateTime beginDatum, DateTime eindDatum) // Ingrediënten
        {
            double uitgavenIngredienten = 0;
            foreach (var uitgave in database.uitgaven.ingredienten)
            {
                if (beginDatum <= uitgave.Item2 && uitgave.Item2 <= eindDatum)
                {
                    uitgavenIngredienten += uitgave.Item1;
                    break;
                }
            }
            return uitgavenIngredienten;
        }

        public double GetUitgavenWerknemers(DateTime beginDatum, DateTime eindDatum) // Werknemers
        {
            double uitgavenWerknemers = 0;
            foreach (var uitgave in database.uitgaven.werknemer)
            {
                if (beginDatum <= uitgave.Item2 && uitgave.Item2 <= eindDatum)
                {
                    uitgavenWerknemers += uitgave.Item1;
                    break;
                }
            }
            return uitgavenWerknemers;
        }

        /*public double GetUitgavenOverig(DateTime beginDatum, DateTime eindDatum)
        {
            double uitgavenOverig = 0;
            return uitgavenOverig;
        }*/

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

        #region Winst_of_Verlies

        public double Winst_of_Verlies(DateTime beginDatum, DateTime eindDatum)
        {
            double inkomsten = Inkomsten(beginDatum, eindDatum);
            double uitgaven = Uitgaven(beginDatum, eindDatum);

            return inkomsten - uitgaven;
        }

        #endregion
    }
}