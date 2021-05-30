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
                if (reservering.datum.ToShortDateString() == datum.ToShortDateString())
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
                if (reservering.tafels.Count == 0)
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
            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine("Welkom in het medewerkersmenu.");
            Console.WriteLine("[1] Reserveringen bekijken");
            Console.WriteLine("[2] Tafels koppelen");
            Console.WriteLine("[3] Feedback bekijken");
            Console.WriteLine("[4] Menukaart");
            Console.WriteLine("[5] Reviews bekijken");

            (string, int) antwoord = AskForInput(16);
            if (antwoord.Item2 != -1)
            {
                return antwoord.Item2;
            }
            else if (antwoord.Item1 == "0")
            {
                LogoutWithMessage();
                return 0;
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
                return 20;
            }
            else if (antwoord.Item1 == "4")
            {
                return 1;
            }
            else if (antwoord.Item1 == "5")
            {
                return 2;
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
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    } // Schermnummer: 16

    public class EmployeeFeedbackScreen : Screen
    {
        public override int DoWork()
        {
            var database = io.GetDatabase();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Work in progress!");
            Console.ReadKey();
            return 16;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    } // Schermnummer: 20

    public class AddTableToReservationScreen : Screen
    {
        private Reserveringen reservering;
        private bool vanGetReservationsScreen;

        public AddTableToReservationScreen(Reserveringen reservering)
        {
            this.reservering = reservering;
            vanGetReservationsScreen = true;
        }

        public AddTableToReservationScreen()
        {
            vanGetReservationsScreen = false;
        }
        
        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(true));
            string datum = DateTime.Now.ToShortDateString();
            Dictionary<string, List<Reserveringen>> reserveringenZonderTafel = new Dictionary<string, List<Reserveringen>>();
            List<Tuple<DateTime, List<Tafels>>> beschikbareTafels = code_medewerker.getBeschikbareTafels(DateTime.Parse(datum));

            if (!vanGetReservationsScreen) {
                List<Reserveringen> reserveringLijst = io.GetReservations();
                List<string> datums = reserveringLijst.Select(reservering => reservering.datum.ToShortDateString()).Distinct().ToList(); //Pakt datum v reservering, zet t om naar string datum, Distinct = geen dubbele waarden, ToList zet m om naar n list
                
                for(int i = 0; i < datums.Count; i++)
                {
                    reserveringenZonderTafel.Add(datums[i], reserveringLijst.Where(R => R.tafels.Count == 0 && R.datum.ToShortDateString() == datums[i]).ToList()); //Kijkt of aantal tafels bij reservering == 0, en datum == datum van de reservering
                }

                int maxLength = 104; //Max aantal karakters per lijn
                double pos = 0; //Positie in de grid aan reserveringen
                List<string> pages = new List<string>();
                int pagNum = 0; //Huidig paginanummer
                do
                {
                    noReserveringen:
                    (int, int, double) nextPageFunc = (0, 0, 0.0);
                    pages = new List<string>();
                    List<List<string>> reserveringString = new List<List<string>>();

                    if (reserveringenZonderTafel.ContainsKey(datum))
                    {
                        reserveringString = Makedubbelboxes(code_eigenaar.ReserveringenToString(reserveringenZonderTafel[datum]));
                    }

                    if (reserveringString.Count == 0)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine("Geen ongekoppelde reserveringen gevonden op " + datum);
                        nextPageFunc = Nextpage(pagNum, pos, pagNum, 16,
                            new List<Tuple<(int, int, double), string>>
                            {
                                Tuple.Create((-2, -2, pos), "D3"),
                                Tuple.Create((-3, -3, pos), "D4"),
                                Tuple.Create((-4, -4, pos), "D5"),
                            },
                            new List<string> { "[3] Volgende dag             [4] Vorige dag             [5] Naar vandaag" });
                        if (nextPageFunc.Item2 > -1)
                        {
                            return nextPageFunc.Item2;
                        }
                        switch (nextPageFunc.Item1)
                        {
                            case -2: //Volgende dag
                                datum = DateTime.Parse(datum).AddDays(1).ToShortDateString();
                                break;
                            case -3: //Vorige dag
                                datum = DateTime.Parse(datum).AddDays(-1).ToShortDateString();
                                break;
                            case -4: //Vandaag
                                datum = DateTime.Now.ToShortDateString();
                                break;
                        }
                        goto noReserveringen;
                    }

                    List<string> vakjes = new List<string>();
                    for (int i = 0; i < reserveringString.Count; i++)
                    {
                        if (i == reserveringString.Count - 1 && reserveringString[i][1].Length < 70) //Checkt of er maar 1 alleeeeeeenig vakje is
                        {
                            if (i == Convert.ToInt32(Math.Floor(pos/2))) //Checkt of positie in de grid (omgezet naar int) gelijk is aan i
                            {
                                if (i != 0 && i % 6 != 0)
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true, new List<string>{
                                        "[6] Tafels koppelen" + new string(' ', 50 - "[6] Tafels koppelen".Length),
                                        new string(' ', 50)}));
                                }
                                else
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, 50, true, new List<string>{
                                        "[6] Tafels koppelen" + new string(' ', 50 - "[6] Tafels koppelen".Length),
                                        new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                if (i != 0 && i % 6 != 0)
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true));
                                }
                                else
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true));
                                }
                            }
                        }
                        else
                        {
                            if (i == Convert.ToInt32(Math.Floor(pos/2))) //Checkt of positie in de grid (omgezet naar int) gelijk is aan i
                            {
                                if (pos % 2 == 0 || pos == 0)
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true, new List<string>{
                                        "[6] Tafels koppelen" + new string(' ', 50 - "[6] Tafels koppelen".Length) + "##  " + new string(' ', 50),
                                        new string(' ', 50) + "##  " + new string(' ', 50)}));
                                }
                                else
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true, new List<string>{
                                        new string(' ', 50) + "##  " + "[6] Tafels koppelen" + new string(' ', 50 - "[6] Tafels koppelen".Length),
                                        new string(' ', 50) + "##  " + new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true));
                            }
                        }
                    }
                    pages = MakePages(vakjes, 3);
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));

                    Console.WriteLine();
                    Console.WriteLine("Reserveringen van: " + datum + (datum == DateTime.Now.ToShortDateString() ? " (vandaag)" : ""));
                    Console.WriteLine("Pagina " + (pagNum + 1) + " van de " + (pages.Count) + ":");
                    Console.WriteLine(pages[pagNum] + new string('#', maxLength + 6));

                    List<Tuple<(int, int, double), string>> nextPageFuncTuples = new List<Tuple<(int, int, double), string>>();
                    List<string> tekst = new List<string>();
                    if (pages.Count > 0 && pagNum > 0 && pagNum < pages.Count - 1) //Checkt of er een volgende en vorige pagina is
                    {
                        tekst.Add("[1] Volgende pagina                                     [2] Vorige pagina");
                        nextPageFuncTuples.Add(Tuple.Create((pagNum + 1, -1, (pagNum + 1) * 6.0), "D1"));
                        nextPageFuncTuples.Add(Tuple.Create((pagNum - 1, -1, (pagNum - 1) * 6.0), "D2"));
                    }
                    else if (pages.Count - 1 > 0 && pagNum < pages.Count - 1) //Checkt over er alleen een volgende pagina is
                    {
                        tekst.Add("[1] Volgende pagina");
                        nextPageFuncTuples.Add(Tuple.Create((pagNum + 1, -1, (pagNum + 1) * 6.0), "D1"));
                    }
                    else if (pages.Count - 1 > 0 && pagNum >= pages.Count - 1) //Checkt of er alleen een vorige pagina is
                    {
                        tekst.Add("                                                        [2] Vorige pagina");
                        nextPageFuncTuples.Add(Tuple.Create((pagNum - 1, -1, (pagNum - 1) * 6.0), "D2"));
                    }

                    nextPageFuncTuples.Add(Tuple.Create((-2, -2, pos), "D3"));
                    nextPageFuncTuples.Add(Tuple.Create((-3, -3, pos), "D4"));
                    nextPageFuncTuples.Add(Tuple.Create((-6, -6, pos), "D6"));
                    tekst.Add("[3] Volgende dag             [4] Vorige dag             [5] Naar vandaag");

                    nextPageFunc = Nextpage(pagNum, pos, vakjes.Count * 2 - 1, 16, nextPageFuncTuples, tekst);
                    if (nextPageFunc.Item2 > -1)
                    {
                        return nextPageFunc.Item2;
                    }
                    string newDatum = DateTime.Now.ToShortDateString();

                    switch (nextPageFunc.Item1)
                    {
                        case -2: //Volgende dag
                            datum = DateTime.Parse(datum).AddDays(1).ToShortDateString();
                            break;
                        case -3: //Vorige dag
                            datum = DateTime.Parse(datum).AddDays(-1).ToShortDateString();
                            break;
                        case -4:
                            datum = DateTime.Now.ToShortDateString();
                            break;
                        case -6:
                            return 16;
                    }
                    pagNum = 0;
                    pos = 0;
                    if (nextPageFunc.Item1 >= 0)
                    {
                        pos = nextPageFunc.Item3;
                        pagNum = nextPageFunc.Item1;
                    }
                } while (true);
            }

            /*
             * Okee ff kneitergrote comment hier want gotta heb n beetje het idee watk aant doen ben haha
             * 0. Programma haalt alle niet gekoppelde reserveringen op -> getReserveringenZonderTafel(datum)
             * 0.1 Programma haalt alle beschikbare tafels op -> getBeschikbareTafels(datum) 
             * 1. "Kies (met de pijltjestoetsen) een reservering om te koppelen aan een tafel"
             * 2. Reserveringen staan in boxes (net zoals bij reviews enz), je kan door de reserveringen navigeren met pijltjestoetsen
             * 3. Medewerker kiest een reservering uit, programma ziet hoeveel tafels er nodig zijn voor deze reservering
             * 4. Medewerker kiest een tafel(s) uit om te koppelen aan de reservering (idk hoek tafels moet vormgeven tho) -> tafelKoppelen(reservering, tafels)
             * 4.1 Als er een onjuist aantal tafels wordt meegegeven, geeft het programma een foutmelding en moet de medewerker opnieuw tafels kiezen
             * 5. Als de reservering aan de tafel is gekoppeld wordt de reservering weggehaald uitde lijst met ongekoppelde reserveringen
             * 5.1 De tafel wordt weggehaald uitde lijst met beschikbare tafels
            */

            Console.WriteLine("Work in progress!");
            Console.ReadKey();
            return 16;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    } // Schermnummer: 18

    public class MakeReservationScreen : Screen
    {
        public override int DoWork()
        {
            var database = io.GetDatabase();
            List<Reserveringen> reserveringen = database.reserveringen;
            var account = ingelogd.klantgegevens;

            Console.WriteLine(GetGFLogo(true));
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
                (string, int) dag = AskForInput(5);
                if (dag.Item2 != -1)
                {
                    return dag.Item2;
                }
                Console.WriteLine("\nHet tijdstip waarop u wilt reserveren (genoteerd als '18:30'): ");
                tijd = AskForInput(5);
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

            if (dagtijd < DateTime.Now)
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
                        aantalPersonen = AskForInput(5);
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
                        (string, int) antwoord = AskForInput(5);
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
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    } // Schermnummer: 6

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
            database = io.GetDatabase();
            if (database.reserveringen == null || database.inkomsten.bestelling_reservering == null) return 0;
            List<int> id = database.reserveringen.Where(d => d.datum >= beginDatum && d.datum <= eindDatum).Select(i => i.ID).ToList();
            double inkomsten = database.inkomsten.bestelling_reservering.Where(d => id.Contains(d.ID)).Select(p => p.prijs).ToList().Sum();
            //double test = database.inkomsten.bestelling_reservering.Select(p => p.prijs).ToList().Sum();


            return inkomsten;
        }

        public List<(DateTime, string, double)> InkomstenPerItem(DateTime beginDatum, DateTime eindDatum)
        {
            database = io.GetDatabase();
            if (database.reserveringen == null || database.inkomsten.bestelling_reservering == null) return new List<(DateTime, string, double)>();
            List<Reserveringen> reserveringen = database.reserveringen.Where(d => d.datum >= beginDatum && d.datum <= eindDatum).ToList();
            List<int> IDs = reserveringen.Select(i => i.ID).ToList();
            List<(DateTime, string, double)> output = new List<(DateTime, string, double)>();
            for (int a = 0; a < database.inkomsten.bestelling_reservering.Count; a++)
            {
                if (IDs.Contains(database.inkomsten.bestelling_reservering[a].reservering_ID))
                {
                    output.Add((reserveringen[IDs.IndexOf(database.inkomsten.bestelling_reservering[a].reservering_ID)].datum,
                        "Reservering van: " + database.login_gegevens[database.login_gegevens.IndexOf(database.login_gegevens.Where(k => k.klantgegevens.klantnummer == reserveringen[IDs.IndexOf(database.inkomsten.bestelling_reservering[a].reservering_ID)].klantnummer).FirstOrDefault())].klantgegevens.voornaam + " " +
                        database.login_gegevens[database.login_gegevens.IndexOf(database.login_gegevens.Where(k => k.klantgegevens.klantnummer == reserveringen[IDs.IndexOf(database.inkomsten.bestelling_reservering[a].reservering_ID)].klantnummer).FirstOrDefault())].klantgegevens.achternaam,
                        database.inkomsten.bestelling_reservering[a].prijs));
                }
            }
            output = output.OrderBy(d => d.Item1).ToList();
            return output;
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