using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Threading;

namespace restaurant
{
    //Voor het goed gebruiken van de testing class is het handig om de data op een bepaalde manier toe te voegen.
    //Voeg als eerste klantgegevens in door de funcite Fill_Userdata
    //Voeg daarna reserveringen toe met de functie Fill_reservations
    //Voeg daarna inkosten toe met de functie Inkomsten
    public class Testing_class
    {
        private Database database = new Database();
        private readonly IO io = new IO();
        private List<Reserveringen> reserveringen_list = new List<Reserveringen>();

        public Testing_class()
        {
            database = io.Getdatabase();
           
            
        }
        
        public void Debug()
        {
            Make_menu();
            Fill_Userdata(100);
            //Fill_reservations(1000, 9, 9, 9, 9);
            //Fill_reservations(10000, 1, 12, 1, 28);
            Fill_reservations_threading(24, 5000, 1, 3, 1, 9);
            Make_reviews();

            //Save_Sales();

            Inkomsten inkomsten = Sales(new DateTime(DateTime.Now.Year, 9, 9, 10, 0, 0), new DateTime(DateTime.Now.Year, 9, 9, 22, 59, 0));
            database.inkomsten = inkomsten;

            List<Tuple<DateTime, List<Tafels>>> test = Reservering_beschikbaarheid(Calc_totale_beschikbaarheid(9, 9, 9, 9), database.reserveringen);         
        }

        //In de region hierinder staat alle code voor het opslaan van Reserveringen
        #region Reserveringen

        private void Fill_reservations_threading(int threads,int amount, int start_month, int stop_month, int start_day, int stop_day)
        {
            Thread[] reservation_thread = new Thread[threads];
            database = io.Getdatabase();
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = Calc_totale_beschikbaarheid(start_month, stop_month, start_day, stop_day);
            for (int a = 0; a < threads; a++)
            {
                reservation_thread[a] = new Thread(() => Fill_reservations(threads, a, amount, new List<Tuple<DateTime, List<Tafels>>>(beschikbaar)));
            }

            for (int a = 0; a < threads; a++)
            {
                reservation_thread[a].Start();
            }


            for (int b = 0; b < threads; b++)
            {
                reservation_thread[b].Join();
            }

            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        //Deze functie is voor als je de database wilt vullen met random reserveringen
        //Als er al klantgegevens zijn in het systeem dan vult hij die gelijk aan in de reservering
        //Als er geen klantgegevens in het systeem zijn dan kunnen er ook geen gerechten gegeten zijn dus die zijn dan ook leeg in de reservering
        public void Fill_reservations(int amount, int start_month, int stop_month, int start_day, int stop_day)
        {
            database = io.Getdatabase();
            List<Reserveringen> reserveringen_list = new List<Reserveringen>();
            List<Tuple<DateTime, List<Tafels>>> totaal_beschikbaar = Calc_totale_beschikbaarheid(start_month, stop_month, start_day, stop_day);
            Random rnd = new Random();
            for (int a = 0; a < amount; a++)
            {
                make_reservation(a, totaal_beschikbaar);
            }

            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        public void Fill_reservations(int threads, int ofset, int amount, List<Tuple<DateTime, List<Tafels>>> totaal_beschikbaar)
        {           
            for (int a = ofset; a < amount; a += threads - 1)
            {
                make_reservation(a, totaal_beschikbaar);
            }
        }

        private void make_reservation(int a, List<Tuple<DateTime, List<Tafels>>> totaal_beschikbaar)
        {
            Random rnd = new Random();
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = Reservering_beschikbaarheid(totaal_beschikbaar, reserveringen_list);
        a:
            List<Tafels> tafels = new List<Tafels>();
            int pos = rnd.Next(0, beschikbaar.Count);

            if (beschikbaar.Count == 0)
            {
                database.reserveringen = reserveringen_list;
                return;
            }

            int tafel_count = rnd.Next(1, 4);
            if (beschikbaar[pos].Item2.Count >= tafel_count)
            {
                for (int b = 0; b < tafel_count; b++)
                {
                    tafels.Add(beschikbaar[pos].Item2[rnd.Next(0, beschikbaar[pos].Item2.Count)]);
                }
            }
            else
            {
                goto a;
            }

            if (database.login_gegevens.Count == 0)
            {
                reserveringen_list.Add(new Reserveringen
                {
                    datum = beschikbaar[pos].Item1,
                    ID = a,
                    tafels = tafels,
                });
            }
            else
            {
                List<int> klantnummers = new List<int>();
                switch (tafels.Count)
                {
                    case 1:
                        for (int c = 0; c < rnd.Next(1, 5); c++)
                        {
                            klantnummers.Add(database.login_gegevens[rnd.Next(database.login_gegevens.Count)].klantgegevens.klantnummer);
                        }
                        break;
                    case 2:
                        for (int c = 0; c < rnd.Next(5, 9); c++)
                        {
                            klantnummers.Add(database.login_gegevens[rnd.Next(database.login_gegevens.Count)].klantgegevens.klantnummer);
                        }
                        break;
                    case 3:
                        for (int c = 0; c < rnd.Next(9, 13); c++)
                        {
                            klantnummers.Add(database.login_gegevens[rnd.Next(database.login_gegevens.Count)].klantgegevens.klantnummer);
                        }
                        break;
                }

                List<Gerechten> gerechten = Make_dishes(klantnummers.Count * 3);
                List<int> gerechten_ID = gerechten.Select(g => g.ID).ToList();

                reserveringen_list.Add(new Reserveringen
                {
                    datum = beschikbaar[pos].Item1,
                    ID = a,
                    tafels = tafels,
                    klantnummers = klantnummers,
                    gerechten_ID = gerechten_ID
                });
            }
        }

        private  List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(List<Tuple<DateTime, List<Tafels>>> beschikbaar, List<Reserveringen> reserveringen)
        {
            if (reserveringen == null) return beschikbaar;
            if (beschikbaar.Count == 0) return beschikbaar;

            //verantwoordelijk voor het communiceren met de database
            for (int c = 0; c < reserveringen.Count; c++)
            {
                List<Tafels> tempTableList = new List<Tafels>();
                List<Tafels> removed_tables = new List<Tafels>();
                int location = 0;
                for (int d = 0; d < beschikbaar.Count; d++)
                {
                    if (beschikbaar[d].Item1 == reserveringen[c].datum)
                    {
                        tempTableList = new List<Tafels>(beschikbaar[d].Item2);
                        location = d;
                        break;
                    }
                }

                //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
                foreach (var tafel in reserveringen[c].tafels)
                {
                    tempTableList.Remove(tafel);
                    removed_tables.Add(tafel);
                }

                //als er geen tafels meer vrij zijn haalt hij de datum weg
                if (tempTableList.Count == 0)
                {
                    beschikbaar.RemoveAt(location);
                    break;
                }
                //maakt tuple met tafels die wel beschikbaar zijn
                else
                {
                    beschikbaar[location] = Tuple.Create(reserveringen[c].datum, tempTableList);
                    for (int b = 1; b <= 8; b++)
                    {
                        if ((location + b) >= beschikbaar.Count) break;
                        beschikbaar[location + b] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());
                        if (beschikbaar[location + b].Item2.Count == 0)
                        {
                            beschikbaar.RemoveAt(location + b);
                        }
                    }                  
                }  
            }

            return beschikbaar;
        }

        private List<Tuple<DateTime, List<Tafels>>> Calc_totale_beschikbaarheid(int start_maand, int eind_maand, int start_dag, int eind_dag)
        {
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(DateTime.Now.Year, start_maand, start_dag, 10, 0, 0);
            for (int maanden = start_maand; maanden <= eind_maand; maanden++)
            {
                for (int days = start_dag; days <= eind_dag; days++)
                {
                    //gaat naar de volgende dag met de openingsuren
                    possibleTime = new DateTime(DateTime.Now.Year, maanden, days, 10, 0, 0);
                    //45 kwaterieren van 1000 tot 2100
                    for (int i = 0; i < 45; i++)
                    {
                        beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                        possibleTime = possibleTime.AddMinutes(15);
                    }
                }
                possibleTime = new DateTime(DateTime.Now.Year, maanden, start_dag, 10, 0, 0);
            }

            return beschikbaar;
        }

        //Deze functie is voor als je de database wilt vullen met je eigen data.
        //Zorg wel dat iedere list even lang is als amount
        public void Fill_reservations(int amount, List<DateTime> datum, List<List<int>> gerechten_ID, List<List<Tafels>> tafels, List<List<int>> klantnummers)
        {
            database = io.Getdatabase();
            if (datum.Count != amount || gerechten_ID.Count != amount || tafels.Count != amount)
            {
                return;
            }

            List<Reserveringen> reserveringen_list = new List<Reserveringen>();
            for (int a = 0; a < amount; a++)
            {
                reserveringen_list.Add(new Reserveringen
                {
                    datum = datum[a],
                    ID = a,
                    gerechten_ID = gerechten_ID[a],
                    tafels = tafels[a],
                    klantnummers = klantnummers[a]
                });
            }

            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        #endregion

        //In de region hieronder staat alle code voor het maken van gerechten
        #region Gerechten

        //Deze functie maakt de menukaart aan en vult de gerechten aan
        public void Make_menu()
        {
            database = io.Getdatabase();
            Menukaart menukaart = new Menukaart
            {
                gerechten = Get_standard_dishes()
            };

            database.menukaart = menukaart;
            io.Savedatabase(database);
        }

        //Deze functie is voor als je simpel een lijst van gerechten wilt zonder voorkeur
        public  List<Gerechten> Make_dishes(int amount)
        {
            List<Gerechten> gerechten = new List<Gerechten>();
            Random rnd = new Random();

            for (int a = 0; a <= amount; a++)
            {
                switch (rnd.Next(6))
                {
                    case 0:
                        gerechten.Add(new Gerechten
                        {
                            ID = 0,
                            naam = "Pizza Salami",
                            is_populair = true,
                            is_gearchiveerd = false,
                            special = true,
                            prijs = 15.0
                        });
                        break;
                    case 1:
                        gerechten.Add(new Gerechten
                        {
                            ID = 1,
                            naam = "Vla",
                            is_populair = false,
                            is_gearchiveerd = false,
                            special = true,
                            prijs = 8.0
                        });
                        break;
                    case 2:
                        gerechten.Add(new Gerechten
                        {
                            ID = 2,
                            naam = "Hamburger",
                            is_populair = true,
                            is_gearchiveerd = false,
                            special = false,
                            prijs = 13.0
                        });
                        break;
                    case 3:
                        gerechten.Add(new Gerechten
                        {
                            ID = 3,
                            naam = "Yoghurt",
                            is_populair = false,
                            is_gearchiveerd = true,
                            special = false,
                            prijs = 6.0
                        });
                        break;
                    case 4:
                        gerechten.Add(new Gerechten
                        {
                            ID = 4,
                            naam = "IJs",
                            is_populair = false,
                            is_gearchiveerd = true,
                            special = false,
                            prijs = 9.5
                        });
                        break;
                    case 5:
                        gerechten.Add(new Gerechten
                        {
                            ID = 5,
                            naam = "Patat",
                            is_populair = true,
                            is_gearchiveerd = false,
                            special = false,
                            prijs = 11.5
                        });
                        break;
                }
            }

            return gerechten;
        }

        public  List<Gerechten> Get_standard_dishes()
        {
            List<Gerechten> gerechten = new List<Gerechten>();
            gerechten.Add(new Gerechten
            {
                ID = 0,
                naam = "Pizza Salami",
                is_populair = true,
                is_gearchiveerd = false,
                special = true,
                prijs = 15.0,
                allergenen = new List<string>()
            });
            gerechten.Add(new Gerechten
            {
                ID = 1,
                naam = "Vla",
                is_populair = false,
                is_gearchiveerd = false,
                special = true,
                prijs = 8.0,
                allergenen = new List<string>
                {
                    "lactose intolerantie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 2,
                naam = "Hamburger",
                is_populair = true,
                is_gearchiveerd = false,
                special = false,
                prijs = 13.0,
                allergenen = new List<string>()
            });
            gerechten.Add(new Gerechten
            {
                ID = 3,
                naam = "Yoghurt",
                is_populair = false,
                is_gearchiveerd = true,
                special = false,
                prijs = 6.0,
                allergenen = new List<string>
                {
                    "lactose intolerantie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 4,
                naam = "IJs",
                is_populair = false,
                is_gearchiveerd = true,
                special = false,
                prijs = 9.5,
                allergenen = new List<string>
                {
                    "lactose intolerantie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 5,
                naam = "Patat",
                is_populair = true,
                is_gearchiveerd = false,
                special = false,
                prijs = 11.5,
                allergenen = new List<string>()
            });

            return gerechten;
        }

        //Deze functie is voor als je een lijst van gerechten wilt met een voorkeur. Zorg wel dat iedere list die je doorgeeft dezelfde lengte heeft
        public  List<Gerechten> Make_dishes(List<string> names, List<bool> populair, List<bool> archived, List<bool> special, List<double> price)
        {
            if (populair.Count != names.Count || archived.Count != names.Count || special.Count != names.Count || price.Count != names.Count)
            {
                List<Gerechten> leeg = new List<Gerechten>();
                return leeg;
            }

            List<Gerechten> gerechten = new List<Gerechten>();
            for (int a = 0; a < names.Count; a++)
            {
                gerechten.Add(new Gerechten
                {
                    naam = names[a],
                    ID = a,
                    special = special[a],
                    is_gearchiveerd = archived[a],
                    is_populair = populair[a],
                    prijs = price[a],
                });
            }

            return gerechten;
        }

        #endregion

        //In de region hieronder staat alle code voor het maken van klantgegevens en login gegevens
        #region Klantgegevens

        /// <summary>
        /// This is a private function to make a list of names. This is used for making random clients
        /// </summary>
        /// <param name="amount">Fill here the amount of different users you want to add</param>
        public void Fill_Userdata(int amount)
        {
            database = io.Getdatabase();
            string[][] names = Make_Names();
            Random rnd = new Random();
            List<Login_gegevens> login_Gegevens = new List<Login_gegevens>();
            for (int a = 0; a < amount; a++)
            {
                string Surname;
                string Firstname;
                if (a % 2 == 0)
                {
                    Firstname = names[0][rnd.Next(0, 20)];
                }
                else
                {
                    Firstname = names[1][rnd.Next(0, 20)];
                }

                if (Firstname != "Rémon" && Firstname != "Moureen")
                {
                    Surname = names[2][rnd.Next(0, 40)];
                }
                else if (Firstname == "Rémon")
                {
                    Surname = "Zander";
                }
                else
                {
                    Surname = "Wittekoek";
                }

                login_Gegevens.Add(new Login_gegevens
                {
                    email = Firstname + "." + Surname + "@gmail.com",
                    type = "Gebruiker",
                    password = "000" + a.ToString(),
                    klantgegevens = new Klantgegevens
                    {
                        voornaam = Firstname,
                        achternaam = Surname,
                        geb_datum = new DateTime(rnd.Next(1929, 2006), rnd.Next(1, 13),rnd.Next(1, 29), 1, 0, 0),
                        klantnummer = a,
                    }
                }) ;
            }

            database.login_gegevens = login_Gegevens;
            io.Savedatabase(database);
        }

        /// <summary>
        /// This is a private function to make a list of names. This is used for making random clients
        /// </summary>
        /// <returns>This returns a jaggered string, array 1 is male names, array 2 is female names and array 3 is surnames</returns>
        private string[][] Make_Names()
        {
            string[][] names = new string[3][];
            names[0] = new string[]
            {
               "Wade",
                "Dave",
                "Seth",
                "Ivan",
                "Riley",
                "Gilbert",
                "Jorge",
                 "Dan",
                "Brian",
                "Roberto",
                "Rémon",
                "Miles",
                "Liam",
                "Nathaniel",
                "Ethan",
                "Lewis",
                "Milton",
                "Claude",
                "Joshua",
                "Glen",
                "Harvey",
                "Blake",
                "Antonio",
            };

            names[1] = new string[]
            {
                "Daisy",
                "Deborah",
                "Isabel",
                "Stella",
                "Debra",
                "Beverly",
                "Vera",
                "Angela",
                "Lucy",
                "Lauren",
                "Janet",
                "Loretta",
                "Tracey",
                "Beatrice",
                "Sabrina",
                "Moureen",
                "Chrysta",
                "Christina",
                "Vicki",
                "Molly",
            };

            names[2] = new string[]
            {
                "Williams",
                "Harris",
                "Thomas",
                "Robinson",
                "Walker",
                "Scott",
                "Nelson",
                "Mitchell",
                "Morgan",
                "Cooper",
                "Howard",
                "Davis",
                "Miller",
                "Martin",
                "Smith",
                "Anderson",
                "White",
                "Perry",
                "Clark",
                "Richards",
                "Wheeler",
                "Wittekoek",
                "Zander",
                "Holland",
                "Terry",
                "Shelton",
                "Miles",
                "Lucas",
                "Fletcher",
                "Parks",
                "Norris",
                "Guzman",
                "Daniel",
                "Newton",
                "Potter",
                "Francis",
                "Erickson",
                "Norman",
                "Moody",
                "Lindsey",
            };
            return names;
        }

        #endregion

        #region Sales

        /// <summary>
        /// This function retuns a list of Sales between begintime and endtime. If there are no reservations then this function returns new Inkomsten().
        /// </summary>
        /// <param name="begintime">This is the starting date</param>
        /// <param name="endtime">This is the end date, this can't be the the same or higher then the current date</param>
        public Inkomsten Sales(DateTime begintime, DateTime endtime)
        {
            database = io.Getdatabase();
            if (database.reserveringen.Count == 0) return new Inkomsten();

            Random rnd = new Random();

            Inkomsten inkomsten = database.inkomsten;
            List<Bestelling_reservering> bestelling_Reservering = new List<Bestelling_reservering>();
            for (int a = 0, b = 0; a < database.reserveringen.Count; a++)
            {
                if (database.reserveringen[a].datum >= begintime && database.reserveringen[a].datum <= endtime)
                {
                    double prijs = 0;
                    foreach (var gerecht_ID in database.reserveringen[a].gerechten_ID)
                    {
                        foreach (var gerecht in database.menukaart.gerechten)
                        {
                            if (gerecht.ID == gerecht_ID)
                            {
                                prijs += gerecht.prijs;
                                break;
                            }
                        }                        
                    }
                    bestelling_Reservering.Add(new Bestelling_reservering
                    {
                        ID = b,
                        reservering_ID = database.reserveringen[a].ID,
                        fooi = rnd.Next(11),
                        prijs = prijs,
                        BTW = prijs * 0.21,
                    });

                    b++;
                }
            }

            inkomsten.bestelling_reservering = bestelling_Reservering;

            return inkomsten;
        }

        /// <summary>
        /// This function retuns a list of all Sales. If there are no reservations then this function returns new Inkomsten().
        /// </summary>
        public void Save_Sales()
        {
            database = io.Getdatabase();
            if (database.reserveringen == null) return;

            Random rnd = new Random();

            Inkomsten inkomsten = database.inkomsten;
            List<Bestelling_reservering> bestelling_Reservering = new List<Bestelling_reservering>();
            for (int a = 0, b = 0; a < database.reserveringen.Count; a++)
            {
                double prijs = 0;
                if (database.reserveringen[a].datum < DateTime.Now)
                {
                    foreach (var gerecht_ID in database.reserveringen[a].gerechten_ID)
                    {
                        foreach (var gerecht in database.menukaart.gerechten)
                        {
                            if (gerecht.ID == gerecht_ID)
                            {
                                prijs += gerecht.prijs;
                                break;
                            }
                        }
                    }
                }
                bestelling_Reservering.Add(new Bestelling_reservering
                {
                    ID = b,
                    reservering_ID = database.reserveringen[a].ID,
                    fooi = rnd.Next(11),
                    prijs = prijs,
                    BTW = prijs * 0.21,
                });

                b++;
            }

            inkomsten.bestelling_reservering = bestelling_Reservering;
            database.inkomsten = inkomsten;
            io.Savedatabase(database);
        }

        #endregion

        #region Review and feedback

        public void Make_reviews()
        {
            database = io.Getdatabase();
            if (database.reserveringen == null) return;
            if (database.login_gegevens == null) return;

            List<Review> reviews = new List<Review>();
            Random rnd = new Random();

            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum < DateTime.Now)
                {
                    reviews.Add(new Review
                    {
                        ID = reviews.Count,
                        Klantnummer = reservering.klantnummers[0],
                        reservering_ID = reservering.ID,
                        Rating = rnd.Next(0, 6),
                        message = ""
                    });
                }
            }

            database.reviews = reviews;
            io.Savedatabase(database);
        }

        #endregion

        #region Medewerkers

        public void Maak_werknemer(int amount)
        {
            database = io.Getdatabase();

            string[][] names = Make_Names();

            List<Werknemer> werknemers = new List<Werknemer>();
            Random rnd = new Random();

            for (int a = 0; a < amount; a++)
            {
                werknemers.Add(new Werknemer
                {
                    salaris = 3000,
                    inkomstenbelasting = 0.371,
                    prestatiebeloning = rnd.NextDouble()
                });
            }
        }

        #endregion
    }












    public partial class Code_Gebruiker_menu
    {

    }
    
    public partial class IO
    {

    }
}
