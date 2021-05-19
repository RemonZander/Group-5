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
            
        }

        public void Debug()
        {

        }

        #region Feedback

        public List<Feedback> GetFeedbackMedewerker(Werknemer werknemer)
        {
            database = io.GetDatabase();
            var feedbackMedewerker= new List<Feedback>();
            foreach (var feedback in database.feedback)
            {
                if (feedback.recipient == werknemer.ID)
                {
                    feedbackMedewerker.Add(feedback);
                }
            }
            return feedbackMedewerker;
        }

        #endregion


        #region Reververingen

        public List<Reserveringen> getReserveringen(DateTime datum) // Medewerker kan de reserveringen van de huidige dag zien
        {
            database = io.GetDatabase();
            var reserveringenVandaag = new List<Reserveringen>();
            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum == datum)
                {
                    reserveringenVandaag.Add(reservering);
                }
            }
            return reserveringenVandaag;
        }

        #endregion


        #region Tafels

        public List<Tuple<DateTime, List<Tafels>>> getBeschikbareTafels(DateTime datum) // Medewerker kan zien welke tafels beschikbaar zijn op de huidige dag
        {
            return io.ReserveringBeschikbaarheid(datum);
        }
        
        public List<Reserveringen> getReserveringenZonderTafel(DateTime datum) // Returns de reserveringen die nog niet zijn gekoppeld aan een tafel
        {
            var reserveringen = getReserveringen(datum);
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

        public List<Reserveringen> tafelKoppelen(Reserveringen reservering, List<Tafels> tafels) // Medewerker moet de reserveringen kunnen koppelen aan een tafel
        {
            database = io.GetDatabase();
            for (int a = 0; a < database.reserveringen.Count; a++)
            {
                if (database.reserveringen[a].ID == reservering.ID)
                {
                    database.reserveringen[a] = new Reserveringen {
                        datum = database.reserveringen[a].datum,
                        ID = database.reserveringen[a].ID,
                        aantal = database.reserveringen[a].aantal,
                        klantnummer = database.reserveringen[a].klantnummer,
                        tafel_bij_raam = database.reserveringen[a].tafel_bij_raam,
                        tafels = tafels
                    };
                    break;
                }
            }

            io.Savedatabase(database);

            return database.reserveringen;            
        }

        #endregion
    }


    #region Screens

    public class EmployeeMenuScreen : Screen
    {
        public override int DoWork()
        {
            var database = io.GetDatabase();
            Console.WriteLine(GetGFLogo(4));
            Console.WriteLine("Welkom in het medewerkersmenu.");
            Console.WriteLine("\nKies een optie:");
            Console.WriteLine("[1] Reserveringen");
            Console.WriteLine("[2] Tafels koppelen");
            Console.WriteLine("[3] Ga terug");

            (string, int) antwoord = AskForInput(16);
            if (antwoord.Item2 != -1)
            {
                return antwoord.Item2;
            }

            else if (antwoord.Item1 == "1")
            {
                return 17;
            }
            else if (antwoord.Item1 == "2")
            {
                return 18;
            }
            else if (antwoord.Item1 == "3")
            {
                return 0;
            }
            else
            {
                Console.WriteLine("\nU moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op een knop om het opnieuw te proberen.");
                Console.ReadKey();
                return 16;
            }

        }

        public override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    public class MakeReservationScreen : Screen
    {
        public override int DoWork()
        {
            var database = io.GetDatabase();
            List<Reserveringen> reserveringen = database.reserveringen;
            var account = ingelogd.klantgegevens;

            Console.WriteLine(GetGFLogo());
            Console.WriteLine("Hier kunt u een nieuwe reservering plaatsen.");

            DateTime dagtijd = new DateTime();
            (string, int) tijd = ("", -1);
            var starttijd = Convert.ToDateTime("10:00");
            var eindtijd = Convert.ToDateTime("21:00");
            (string, int) aantalPersonen = ("", -1);
            bool tafelBijRaam = false;

            bool succes = false;
            do
            {
                Console.WriteLine("\nDe dag waarop u wilt reserveren (genoteerd als 'dag-maand-jaar'): ");
                (string, int) dag = AskForInput(6);
                if (dag.Item2 != -1)
                {
                    return dag.Item2;
                }
                Console.WriteLine("\nHet tijdstip waarop u wilt reserveren (genoteerd als '18:30'): ");
                tijd = AskForInput(6);
                if (tijd.Item2 != -1)
                {
                    return tijd.Item2;
                }
                try
                {
                    dagtijd = Convert.ToDateTime(dag.Item1 + " " + tijd.Item1);
                    succes = true;
                }
                catch
                {
                    Console.WriteLine("\nU heeft een ongeldig antwoord gegeven voor de datum en/of tijd.");
                    Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                    Console.ReadKey();
                    return 6;
                }
            } while (!succes);

            if (dagtijd < DateTime.Today)
            {
                Console.WriteLine("\nSorry, het lijkt erop dat uw gekozen datum en tijdstip al is verlopen.");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                return 6;
            }
            else if (Convert.ToDateTime(tijd.Item1) < starttijd || Convert.ToDateTime(tijd.Item1) > eindtijd)
            {
                Console.WriteLine("\nSorry, het lijkt erop dat uw gekozen tijdstip buiten onze openingstijden valt.");
                Console.WriteLine("Wij zijn dagelijks van 10:00 tot en met 22:00 geopend.");
                Console.WriteLine("U kunt van 10:00 tot en met 21:00 een reservering plaatsen.");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                return 6;
            }
            else
            {
                succes = false;
                do
                {
                    Console.WriteLine("\nVoor hoeveel personen wilt u reserveren?");
                    try
                    {
                        aantalPersonen = AskForInput(6);
                        if (aantalPersonen.Item2 != -1)
                        {
                            return aantalPersonen.Item2;
                        }
                        int.Parse(aantalPersonen.Item1);
                        if (int.Parse(aantalPersonen.Item1) > 0 && int.Parse(aantalPersonen.Item1) <= 400)
                        {
                            succes = true;
                        }
                        else
                        {
                            Console.WriteLine("\nU heeft een ongeldig antwoord gegeven voor het aantal personen waarvoor u wilt reserveren.");
                            Console.WriteLine("Het aantal personen moet worden aangegeven door middel van een cijfer (1 t/m 400)");
                            Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                            Console.ReadKey();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("\nU heeft een ongeldig antwoord gegeven voor het aantal personen waarvoor u wilt reserveren.");
                        Console.WriteLine("Het aantal personen moet worden aangegeven door middel van een cijfer (1 t/m 400)");
                        Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                        Console.ReadKey();
                    }
                } while (!succes);

                //check of de gekozen tijd en datum beschikbaar zijn
                var minimaalAantalTafels = (int)(int.Parse(aantalPersonen.Item1) / 4.0);
                var beschikbareTafels = io.ReserveringBeschikbaarheid(dagtijd);
                if (beschikbareTafels == null || beschikbareTafels[0].Item2.Count < minimaalAantalTafels)
                {
                    Console.WriteLine("\nSorry, het lijkt erop dat er geen tafels beschikbaar zijn op uw gekozen datum en tijdstip.");
                    Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                    Console.ReadKey();
                    return 6;
                }
                else
                {
                    succes = false;
                    do
                    {
                        Console.WriteLine("\nHeeft u nog voorkeur voor een tafel aan het raam?\n[1] Ja\n[2] Nee");
                        (string, int) antwoord = AskForInput(6);
                        if (antwoord.Item2 != -1)
                        {
                            return antwoord.Item2;
                        }
                        if (antwoord.Item1 == "1")
                        {
                            tafelBijRaam = true;
                            succes = true;
                        }
                        else if (antwoord.Item1 == "2")
                        {
                            tafelBijRaam = false;
                            succes = true;
                        }
                        else
                        {
                            Console.WriteLine("\nU moet wel een juiste keuze maken...");
                            Console.WriteLine("Druk op een knop om verder te gaan.");
                            Console.ReadKey();
                        }
                    } while (!succes);
                }

                code_gebruiker.MakeCustomerReservation(dagtijd, account.klantnummer, int.Parse(aantalPersonen.Item1), tafelBijRaam);

                Console.WriteLine("\nUw reservering wordt verwerkt.");
                Console.WriteLine("Druk op een knop om terug te keren naar het klantenscherm.");
                Console.ReadKey();
                return 5;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    #endregion

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