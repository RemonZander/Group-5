using Microsoft.VisualBasic;
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

        public List<Reserveringen> tijdBewerken(Reserveringen reservering, DateTime Datum) // Medewerker kan tijd van reservering aanpassen als er geen beschikbare tafels zijn
        {
            database = io.GetDatabase();
            for (int a = 0; a < database.reserveringen.Count; a++)
            {
                if (database.reserveringen[a].ID == reservering.ID)
                {
                    database.reserveringen[a] = new Reserveringen
                    {
                        datum = Datum,
                        ID = database.reserveringen[a].ID,
                        aantal = database.reserveringen[a].aantal,
                        klantnummer = database.reserveringen[a].klantnummer,
                        tafel_bij_raam = database.reserveringen[a].tafel_bij_raam,
                        tafels = database.reserveringen[a].tafels
                    };
                    break;
                }
            }

            io.Savedatabase(database);

            return database.reserveringen;
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
        private bool fromMedewerker;
        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(false));
            Console.WriteLine("Welkom in het medewerkersmenu.");
            Console.WriteLine("[1] Menukaart");
            Console.WriteLine("[2] Reviews bekijken");
            Console.WriteLine("[3] Feedback bekijken");
            Console.WriteLine("[4] Reserveringen bekijken");
            Console.WriteLine("[5] Tafels koppelen");

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
                return 1;
            }
            else if (antwoord.Item1 == "2")
            {
                return 2;
            }
            else if (antwoord.Item1 == "3")
            {
                fromMedewerker = true;
                return 20;
            }
            else if (antwoord.Item1 == "4")
            {
                return 17;
            }
            else if (antwoord.Item1 == "5")
            {
                return 18;
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
            screens[20].fromMedewerker = true;
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
            List<Feedback> feedbackList = io.GetFeedback();
            List<Feedback> employeeFeedbackList = new List<Feedback>();

            for (int i = 0; i < feedbackList.Count; i++)
            {
                if (feedbackList[i].recipient == ingelogd.klantgegevens.klantnummer)
                {
                    employeeFeedbackList.Add(feedbackList[i]);
                }
            }

            if (employeeFeedbackList.Count == 0)
            {
                Console.WriteLine("U heeft nog geen feedback.");
                Console.WriteLine("Druk op een toets om terug te keren naar het medewerkersmenu.");
                Console.ReadKey();
                return 16;
            }

            int page = 0;
            int uneven = 0;
            var employeeFeedbackString = Makedubbelboxes(FeedbackToString(employeeFeedbackList));
            List<string> pages = MakePages(BoxAroundText(employeeFeedbackString, "#", 2, 0, 104, true), 3);
            do
            {
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Dit is alle feedback gericht aan u.");
                Console.WriteLine($"Dit is de feedback op pagina {page + 1} van de {pages.Count}:");
                if (employeeFeedbackString[employeeFeedbackString.Count - 1][1].Length < 70 && page == pages.Count - 1)
                {
                    Console.WriteLine(pages[page] + new string('#', 56));
                    uneven = 1;
                }
                else
                {
                    Console.WriteLine(pages[page] + new string('#', 110));
                }

                (int, int) result = (0, 0);
                if (fromMedewerker)
                {
                    result = Nextpage(page, pages.Count * 2 - (1 + uneven), 16);
                }
                else
                {
                    result = Nextpage(page, pages.Count * 2 - (1 + uneven), 11);
                }
                

                if (result.Item2 != -1)
                {
                    return result.Item2;
                }
                page = result.Item1;

            } while (true);
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            screens[20].fromMedewerker = false;
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    } // Schermnummer: 20

    public class AddTableToReservationScreen : Screen
    {
        private Reserveringen reservering;
        private bool vanGetReservationsScreen = false;

        public AddTableToReservationScreen(Reserveringen reservering)
        {
            this.reservering = reservering;
            vanGetReservationsScreen = true;
        }

        public AddTableToReservationScreen()
        {
        }
        
        public override int DoWork()
        {
            string datum = DateTime.Now.ToShortDateString();
            Dictionary<string, List<Reserveringen>> reserveringenZonderTafel = new Dictionary<string, List<Reserveringen>>();
            List<Tuple<DateTime, List<Tafels>>> beschikbareTafels = code_medewerker.getBeschikbareTafels(DateTime.Parse(datum));

            if (!vanGetReservationsScreen) //Als je niet vanaf het reserveringsscherm komt
            {
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
                bool wantToKoppelReservering;
                do
                {
                    noReserveringen:
                    wantToKoppelReservering = false;

                    (int, int, double) nextPageFunc = (0, 0, 0.0);
                    pages = new List<string>();
                    List<List<string>> reserveringString = new List<List<string>>();

                    if (reserveringenZonderTafel.ContainsKey(datum))
                    {
                        reserveringenZonderTafel[datum] = reserveringenZonderTafel[datum].OrderBy(o => o.datum).ToList();
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
                                Tuple.Create((-3, -3, pos), "D3"),
                                Tuple.Create((-3, -3, pos), "NumPad3"),
                                Tuple.Create((-4, -4, pos), "D4"),
                                Tuple.Create((-4, -4, pos), "NumPad4"),
                                Tuple.Create((-5, -5, pos), "D5"),
                                Tuple.Create((-5, -5, pos), "NumPad5"),
                            },
                            new List<string> { "[3] Vorige dag             [4] Volgende dag             [5] Naar vandaag" });
                        if (nextPageFunc.Item2 > -1)
                        {
                            return nextPageFunc.Item2;
                        }
                        switch (nextPageFunc.Item1)
                        {
                            case -3: //Vorige dag
                                datum = DateTime.Parse(datum).AddDays(-1).ToShortDateString();
                                break;
                            case -4: //Volgende dag
                                datum = DateTime.Parse(datum).AddDays(1).ToShortDateString();
                                break;
                            case -5: //Vandaag
                                datum = DateTime.Now.ToShortDateString();
                                break;
                        }
                        goto noReserveringen;
                    }

                    List<string> vakjes = new List<string>();
                    for (int i = 0; i < reserveringString.Count; i++)
                    {
                        if (i == reserveringString.Count - 1 && reserveringString[i][1].Length < 70) //Checkt of het aantal vakjes oneven is & huidige locatie is laatste vakje
                        {
                            if (i == Convert.ToInt32(Math.Floor(pos/2))) //Checkt of positie in de list reserveringString (omgezet naar int) gelijk is aan i
                            {
                                if (i != 0 && i % 6 != 0) //Rechterkant van de grid
                                {
                                    vakjes.Add(BoxAroundText(reserveringString[i], "#", 2, 0, maxLength, true, new List<string>{
                                        "[6] Tafels koppelen" + new string(' ', 50 - "[6] Tafels koppelen".Length),
                                        new string(' ', 50)}));
                                }
                                else //Linkerkant van de grid
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
                    int uneven = 0;
                    pages = MakePages(vakjes, 3);
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));

                    Console.WriteLine();
                    Console.WriteLine("Reserveringen van: " + datum + (datum == DateTime.Now.ToShortDateString() ? " (vandaag)" : ""));
                    Console.WriteLine("Pagina " + (pagNum + 1) + " van de " + (pages.Count) + ":");
                    if (reserveringString[reserveringString.Count - 1][1].Length < 70 && pagNum == pages.Count - 1)
                    {
                        Console.WriteLine(pages[pagNum] + new string('#', 56));
                        uneven = 1;
                    }
                    else
                    {
                        Console.WriteLine(pages[pagNum] + new string('#', 110));
                    }

                    Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reserveringen.\nDe reservering met de tekst 'Tafels koppelen' is de huidig geselecteerde reservering.");

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

                    nextPageFuncTuples.Add(Tuple.Create((-3, -3, pos), "D3"));
                    nextPageFuncTuples.Add(Tuple.Create((-3, -3, pos), "NumPad3"));
                    nextPageFuncTuples.Add(Tuple.Create((-4, -4, pos), "D4"));
                    nextPageFuncTuples.Add(Tuple.Create((-4, -4, pos), "NumPad4"));
                    nextPageFuncTuples.Add(Tuple.Create((-5, -5, pos), "D5"));
                    nextPageFuncTuples.Add(Tuple.Create((-5, -5, pos), "NumPad5"));
                    nextPageFuncTuples.Add(Tuple.Create((-6, -6, pos), "D6"));
                    nextPageFuncTuples.Add(Tuple.Create((-6, -6, pos), "NumPad6"));
                    tekst.Add("[3] Vorige dag             [4] Volgende dag             [5] Naar vandaag");

                    nextPageFunc = Nextpage(pagNum, pos, vakjes.Count * 2 - (1 + uneven), 16, nextPageFuncTuples, tekst);
                    if (nextPageFunc.Item2 > -1)
                    {
                        return nextPageFunc.Item2;
                    }
                    string newDatum = DateTime.Now.ToShortDateString();

                    switch (nextPageFunc.Item1)
                    {
                        case -3: //Vorige dag
                            datum = DateTime.Parse(datum).AddDays(-1).ToShortDateString();
                            break;
                        case -4: //Volgende dag
                            datum = DateTime.Parse(datum).AddDays(1).ToShortDateString();
                            break;
                        case -5: //Vandaag
                            datum = DateTime.Now.ToShortDateString();
                            break;
                        case -6: //Tafels koppelen
                            reservering = reserveringenZonderTafel[datum][Convert.ToInt32(pos)];
                            wantToKoppelReservering = true;
                            break;
                    }
                    pagNum = 0;
                    pos = 0;
                    if (nextPageFunc.Item1 >= 0)
                    {
                        pos = nextPageFunc.Item3;
                        pagNum = nextPageFunc.Item1;
                    }
                } while (!wantToKoppelReservering);
            }

            tafelkoppelen:
            List<Tafels> tafels = new List<Tafels>();
            string tafelID = "";
            List<Tuple<DateTime, List<Tafels>>> alleBeschikbareTafels = code_medewerker.getBeschikbareTafels(reservering.datum);
            List<Tafels> beschikbareTafelsOpTijdstip = new List<Tafels>();

            for (int i = 0; i < alleBeschikbareTafels.Count; i++)
            {
                if (alleBeschikbareTafels[i].Item1 == reservering.datum)
                {
                    for (int j = 0; j < alleBeschikbareTafels[i].Item2.Count; j++)
                    {
                        beschikbareTafelsOpTijdstip.Add(alleBeschikbareTafels[i].Item2[j]);
                    }
                }
            }

            do
            {
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Kies een beschikbare tafel voor reservering " + reservering.ID + "       (Datum en tijd: " + reservering.datum + ")");
                Console.WriteLine("Aantal benodigde tafels: " + Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(reservering.aantal / 4.0))) + "       (Aantal personen: " + reservering.aantal + ")");
                if (reservering.tafel_bij_raam)
                {
                    Console.WriteLine("Graag tafel(s) aan raam: Ja");
                }
                else
                {
                    Console.WriteLine("Graag tafel(s) aan raam: Nee");
                }
                Console.WriteLine("Gekozen tafels: " + tafelID);

                // Laat alle beschikbare tafels zien => ID | tijd | is aan raam
                Console.WriteLine("\n----------------------------------------------");
                Console.WriteLine("| ID  |         Tijd         |  Is aan raam  |");
                Console.WriteLine("----------------------------------------------");

                (string, int) ans = ("", 0);
                if (beschikbareTafelsOpTijdstip.Count == 0 || beschikbareTafelsOpTijdstip.Count < Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(reservering.aantal / 4.0))))
                {
                    Console.WriteLine("Sorry, er zijn geen beschikbare tafels op het tijdstip van deze reservering.");
                    DateTime newDatum = new DateTime();
                    if (reservering.datum.Hour >= 21)
                    {
                        Console.WriteLine("[1] Een kwartier eerder");
                        ans = AskForInput(18);
                        if (ans.Item2 != -1) //Escape
                        {
                            return ans.Item2;
                        }
                        if (ans.Item1 == "0") //Log uit
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        if (ans.Item1 == "1") //Een kwartier eerder
                        {
                            newDatum = reservering.datum.AddMinutes(-15);
                        }
                    }
                    else if (reservering.datum.Hour < 10 || reservering.datum.Hour == 10 && reservering.datum.Minute == 0)
                    {
                        Console.WriteLine("                                         [2] Een kwartier later");
                        ans = AskForInput(18);
                        if (ans.Item2 != -1) //Escape
                        {
                            return ans.Item2;
                        }
                        if (ans.Item1 == "0") //Log uit
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        if (ans.Item1 == "2") //Een kwartier later
                        {
                            newDatum = reservering.datum.AddMinutes(15);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[1] Een kwartier eerder                  [2] Een kwartier later");
                        ans = AskForInput(18);
                        if (ans.Item2 != -1) //Escape
                        {
                            return ans.Item2;
                        }
                        else if (ans.Item1 == "0") //Log uit
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (ans.Item1 == "1") //Een kwartier eerder
                        {
                            newDatum = reservering.datum.AddMinutes(-15);
                        }
                        else if (ans.Item1 == "2") //Een kwartier later
                        {
                            newDatum = reservering.datum.AddMinutes(15);
                        }
                        else
                        {
                            Console.WriteLine("U heeft een ongeldig antwoord gegeven.");
                            Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                            Console.ReadKey();
                            goto tafelkoppelen;
                        }
                    }
                    // verandert datum van reservering & update de reservering
                    List<Reserveringen> reserveringen = code_medewerker.tijdBewerken(reservering, newDatum);
                    for (int i = 0; i < reserveringen.Count; i++)
                    {
                        if (reserveringen[i].ID == reservering.ID)
                        {
                            reservering = reserveringen[i];
                        }
                    }

                    goto tafelkoppelen;
                }

                for (int i = 0; i < beschikbareTafelsOpTijdstip.Count; i++)
                {
                    string isAanRaam;
                    if (beschikbareTafelsOpTijdstip[i].isRaam)
                    {
                        isAanRaam = "Ja   ";
                    }
                    else
                    {
                        isAanRaam = "Nee  ";
                    }

                    if ($"{beschikbareTafelsOpTijdstip[i].ID}".Length == 1)
                    {
                        Console.WriteLine($"| {beschikbareTafelsOpTijdstip[i].ID}   |  {reservering.datum}  |      {isAanRaam}     |");
                    }
                    else //|  Is aan raam  |
                    {
                        Console.WriteLine($"| {beschikbareTafelsOpTijdstip[i].ID}  |  {reservering.datum}  |      {isAanRaam}     |");
                    }
                }

                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("\nTyp hier het ID van de tafel die u wilt koppelen: ");
                ans = AskForInput(18);
                if (ans.Item2 != -1)
                {
                    return ans.Item2;
                }
                else if (ans.Item1 == "0")
                {
                    logoutUpdate = true;
                    Logout();
                    return 0;
                }

                for (int i = 0; i < beschikbareTafelsOpTijdstip.Count; i++)
                {
                    if (Convert.ToString(beschikbareTafelsOpTijdstip[i].ID) == ans.Item1)
                    {
                        tafelID += beschikbareTafelsOpTijdstip[i].ID + "  ";
                        tafels.Add(beschikbareTafelsOpTijdstip[i]);
                        beschikbareTafelsOpTijdstip.Remove(beschikbareTafelsOpTijdstip[i]);
                        Console.WriteLine("Tafel succesvol gekozen, klik op een toets om verder te gaan.");
                        Console.ReadKey();
                        break;
                    }
                }
            } while (tafels.Count != Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(reservering.aantal / 4.0))));

            bool succes = false;
            do
            {
                Console.Clear();
                Console.WriteLine(GFLogo);
                Console.WriteLine("\nU heeft de volgende tafels gekoppeld aan reservering " + reservering.ID + ":  " + tafelID);
                Console.WriteLine("Wilt u uw keuze bevestigen?");
                Console.WriteLine("[1] Ja\n[2] Nee");
                (string, int) ans = AskForInput(18);
                if (ans.Item2 != -1)
                {
                    return ans.Item2;
                }
                else if (ans.Item1 == "0")
                {
                    logoutUpdate = true;
                    Logout();
                    return 0;
                }
                else if (ans.Item1 == "1")
                {
                    code_medewerker.tafelKoppelen(reservering, tafels);
                    Console.WriteLine("Uw keuze is succesvol opgeslagen in het systeem");
                    succes = true;
                }
                else if (ans.Item1 == "2")
                {
                    Console.WriteLine("Druk op een toets om opniew tafels te koppelen aan reservering " + reservering.ID);
                    Console.ReadKey();
                    goto tafelkoppelen;
                }
                else
                {
                    Console.WriteLine("Sorry, het lijkt erop dat u een onjuiste keuze heeft gemaakt.");
                    Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                    Console.ReadKey();
                }
            } while (!succes);

            Console.WriteLine("\nDruk op een toets om terug te keren naar het medewerkersmenu.");
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
                (string, int) date = AskForInput(5);
                if (date.Item2 != -1)
                {
                    return date.Item2;
                }
                Console.WriteLine("\nHet tijdstip waarop u wilt reserveren (genoteerd als '18:30'): ");
                tijd = AskForInput(5);
                if (tijd.Item2 != -1)
                {
                    return tijd.Item2;
                }
                try
                {
                    string dag = date.Item1.Substring(0, date.Item1.IndexOf('-'));
                    date.Item1 = date.Item1.Remove(0, date.Item1.IndexOf('-') + 1);
                    string maand = date.Item1.Substring(0, date.Item1.IndexOf('-'));
                    date.Item1 = date.Item1.Remove(0, date.Item1.IndexOf('-') + 1);
                    string jaar = date.Item1;
                    dagtijd = Convert.ToDateTime(dag + " " + maand + " " + jaar + " " + tijd.Item1);
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
                Console.WriteLine("Druk op een toets om terug te keren naar het klantenmenu.");
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

        public List<Ingredient> GetUitgavenIngredienten() // Ingrediënten
        {
            database = io.GetDatabase();
            List<Ingredient> ingredients = new List<Ingredient>();
            foreach (var ingredient in database.ingredienten)
            {
                ingredients.Add(ingredient);
            }
            return ingredients;
        }

        public double GetUitgavenIngredienten(DateTime begindate, DateTime enddate) // Ingrediënten
        {
            database = io.GetDatabase();
            double price = 0;
            foreach (var ingredient in database.ingredienten)
            {
                if (ingredient.bestel_datum >= begindate && ingredient.bestel_datum <= enddate)
                {
                    price += ingredient.prijs;
                }
            }
            return price;
        }

        public List<(Werknemer, DateTime)> GetUitgavenWerknemers() // Werknemers
        {
            database = io.GetDatabase();
            List<(Werknemer, DateTime)> kosten = new List<(Werknemer, DateTime)>();
            foreach (var uitgave in database.uitgaven.werknemer)
            {
                Werknemer werknemer = database.werknemers.Where(i => i.ID == uitgave.Item1).FirstOrDefault();
                kosten.Add((werknemer, uitgave.Item2));
            }
            return kosten;
        }

        public double GetUitgavenWerknemers(DateTime begindate, DateTime enddate) // Werknemers
        {
            database = io.GetDatabase();
            double price = 0;
            foreach (var uitgave in database.uitgaven.werknemer)
            {
                if (uitgave.Item2 >= begindate && uitgave.Item2 <= enddate)
                {
                    Werknemer werknemer = database.werknemers.Where(i => i.ID == uitgave.Item1).FirstOrDefault();
                    price += (werknemer.salaris + werknemer.salaris * werknemer.prestatiebeloning + werknemer.lease_auto);
                }
            }
            return price;
        }

        public List<(Inboedel, DateTime)> GetInboedel()
        {
            database = io.GetDatabase();
            List<(Inboedel, DateTime)> output = new List<(Inboedel, DateTime)>();
            if (database.uitgaven.Equals(null) || database.uitgaven.inboedel.Count == 0)
            {
                return new List<(Inboedel, DateTime)>();
            }

            for (int a = 0; a < database.uitgaven.inboedel.Count; a++)
            {
                output.Add((database.uitgaven.inboedel[a], database.uitgaven.inboedel[a].datum));
            }
            return output;
        }

        public double GetInboedel(DateTime begindate, DateTime enddate)
        {
            database = io.GetDatabase();
            double output = 0;
            if (database.uitgaven.Equals(null) || database.uitgaven.inboedel.Count == 0)
            {
                return 0;
            }

            for (int a = 0; a < database.uitgaven.inboedel.Count; a++)
            {
                if (database.uitgaven.inboedel[a].datum >= begindate && database.uitgaven.inboedel[a].datum <= enddate)
                {
                    output += database.uitgaven.inboedel[a].prijs;
                }
            }
            return output;
        }

        public double Inkomsten(DateTime beginDatum, DateTime eindDatum) // Mogelijkheid een bepaald tijdsspan in te voeren en daarvan de inkomsten te kunnen zien.
        {
            database = io.GetDatabase();
            if (database.reserveringen == null || database.inkomsten.bestelling_reservering == null) return 0;
            List<int> id = database.reserveringen.Where(d => d.datum >= beginDatum && d.datum <= eindDatum).Select(i => i.ID).ToList();
            double inkomsten = database.inkomsten.bestelling_reservering.Where(d => id.Contains(d.ID)).Select(p => p.prijs).ToList().Sum();

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
    }
}