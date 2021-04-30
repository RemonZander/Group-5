using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using System.Collections.Concurrent;

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
            Fill_reservations_threading(24, 5000, 1, 3, 1, 9);
            Maak_werknemer(10);
            Save_expenses();
            Make_reviews();
            Make_feedback();
            Save_Sales();

            //Inkomsten inkomsten = Sales(new DateTime(DateTime.Now.Year, 9, 9, 10, 0, 0), new DateTime(DateTime.Now.Year, 9, 9, 22, 59, 0));
            //database.inkomsten = inkomsten;

            //List<Tuple<DateTime, List<Tafels>>> test = Reservering_beschikbaarheid(Calc_totale_beschikbaarheid(9, 9, 9, 9), database.reserveringen);
        }

        #region Reserveringen

        public void Fill_reservations_threading(int threads,int amount, int start_month, int stop_month, int start_day, int stop_day)
        {
            Thread[] reservation_thread = new Thread[threads];
            database = io.Getdatabase();
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = Calc_totale_beschikbaarheid(start_month, stop_month, start_day, stop_day);
            BlockingCollection<Ingredient> ingredient_temp = new BlockingCollection<Ingredient>();
            for (int a = 0; a < threads; a++)
            {
                int b = a;
                reservation_thread[a] = new Thread(() => Fill_reservations(threads, b, amount, new List<Tuple<DateTime, List<Tafels>>>(beschikbaar), ingredient_temp));
            }

            for (int a = 0; a < threads; a++)
            {
                reservation_thread[a].Start();
                Thread.Sleep(300);
            }


            for (int b = 0; b < threads; b++)
            {
                reservation_thread[b].Join();
            }

            database.ingredienten.AddRange(ingredient_temp);
            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        private void Fill_reservations(int threads, int ofset, int amount, List<Tuple<DateTime, List<Tafels>>> totaal_beschikbaar, BlockingCollection<Ingredient> ingredient_temp)
        {           
            for (int a = ofset; a < amount; a += threads)
            {
                make_reservation(a, totaal_beschikbaar, ingredient_temp);
            }
        }

        private void make_reservation(int a, List<Tuple<DateTime, List<Tafels>>> totaal_beschikbaar, BlockingCollection<Ingredient> ingredient_temp)
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

                List<Gerechten> gerechten = Make_dishes(klantnummers.Count * 3, beschikbaar[pos].Item1, ingredient_temp);
                List<int> gerechten_ID = gerechten.Select(g => g.ID).ToList();

                reserveringen_list.Add(new Reserveringen
                {
                    datum = beschikbaar[pos].Item1,
                    ID = a,
                    tafels = tafels,
                    klantnummers = klantnummers,
                    gerechten_ID = gerechten_ID,
                    aantal = klantnummers.Count
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

        #endregion

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

        private BlockingCollection<int> Maak_gerechten(string gerecht_naam, DateTime bestel_Datum, BlockingCollection<Ingredient> ingredient_temp)
        {
            BlockingCollection<int> ingredienten_id = new BlockingCollection<int>();
            if (database.ingredienten == null) database.ingredienten = new List<Ingredient>();

            if (gerecht_naam == "Pizza Salami")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Deeg",
                    prijs = 1,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Salami",
                    prijs = 0.80,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Tomaten saus",
                    prijs = 0.60,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

            }
            else if (gerecht_naam == "Vla")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vanille vla",
                    prijs = 1.5,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

            }
            else if (gerecht_naam == "Hamburger")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Broodjes",
                    prijs = 0.10,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vlees",
                    prijs = 0.85,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Sla",
                    prijs = 0.05,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

            }
            else if (gerecht_naam == "Yoghurt")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Yoghurt",
                    prijs = 1.8,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

            }
            else if (gerecht_naam == "IJs")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vanille ijs",
                    prijs = 1.85,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

            }
            else if (gerecht_naam == "Patat")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Frituur vet",
                    prijs = 0.10,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Aardappelen",
                    prijs = 0.15,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Friet saus",
                    prijs = 0.30,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_id.Add(ingredient.ID);
            }

            return ingredienten_id;
        }

        //Deze functie is voor als je simpel een lijst van gerechten wilt zonder voorkeur
        public  List<Gerechten> Make_dishes(int amount, DateTime bestel_Datum, BlockingCollection<Ingredient> ingredient_temp)
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
                            prijs = 15.0,
                            ingredienten = Maak_gerechten("Pizza Salami", bestel_Datum, ingredient_temp).ToList()
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
                            prijs = 8.0,
                            ingredienten = Maak_gerechten("Vla", bestel_Datum, ingredient_temp).ToList()
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
                            prijs = 13.0,
                            ingredienten = Maak_gerechten("Hamburger", bestel_Datum, ingredient_temp).ToList()
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
                            prijs = 6.0,
                            ingredienten = Maak_gerechten("Yoghurt", bestel_Datum, ingredient_temp).ToList()
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
                            prijs = 9.5,
                            ingredienten = Maak_gerechten("IJs", bestel_Datum, ingredient_temp).ToList()
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
                            prijs = 11.5,
                            ingredienten = Maak_gerechten("Patat", bestel_Datum, ingredient_temp).ToList()
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

        #region Klantgegevens

        /// <summary>
        /// This function makes userdata and saves it in the database
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
        /// This is a private function to make a list of names. This is used for making random clients and employee's
        /// </summary>
        /// <returns>This returns a jaggered string array, array 0 is male names, array 1 is female names and array 2 is surnames</returns>
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
        /// <returns>This returns inkomsten where inkomsten.bestelling_reservering is a list of all sales between begintime and endtime</returns>
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
        /// This function Saves all Sales. If there are no reservations then this function returns new Inkomsten().
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

        /// <summary>
        /// This function saves all expenses based on ingredients and tables, chairs and employee's.
        /// If database.reserveringen == null || database.werknemers == null || database.ingredienten == null then this functions aborts.
        /// </summary>
        public void Save_expenses()
        {
            database = io.Getdatabase();
            if (database.reserveringen == null || database.werknemers == null || database.ingredienten == null) return;

            Uitgaven uitgaven = database.uitgaven;
            uitgaven.ingredienten_ID = new List<int>();
            uitgaven.werknemer_ID = new List<int>();
            foreach (var werknemer in database.werknemers)
            {
                uitgaven.werknemer_ID.Add(werknemer.ID);
            }

            foreach (var ingredient in database.ingredienten)
            {
                uitgaven.ingredienten_ID.Add(ingredient.ID);
            }

            uitgaven.inboedel = new List<Inboedel>();
            for (int a = 0; a < 100; a++)
            {
                uitgaven.inboedel.Add(new Inboedel
                {
                    ID = a * 5,
                    item_Naam = "Tafel nummer: " + a,
                    prijs = 50,
                    verzendkosten = 5
                });

                for (int b = 1; b < 5; b++)
                {
                    uitgaven.inboedel.Add(new Inboedel
                    {
                        ID = a * 5 + b,
                        item_Naam = "Stoel",
                        prijs = 20,
                        verzendkosten = 3
                    });
                }
            }

            database.uitgaven = uitgaven;
            io.Savedatabase(database);
        }

        #endregion

        #region Review and feedback

        /// <summary>
        /// This function that saves reviews of all reservations.
        /// If reservations and login_gegevens are empty then this function aborts.
        /// </summary>
        public void Make_reviews()
        {
            database = io.Getdatabase();
            if (database.reserveringen == null || database.login_gegevens == null) return;

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

        /// <summary>
        /// This function that saves feedback of all reservations.
        /// If reservations and login_gegevens and werknemers are empty then this function aborts.
        /// </summary>
        public void Make_feedback()
        {
            database = io.Getdatabase();
            if (database.reserveringen == null || database.login_gegevens == null || database.werknemers == null) return;

            List<Feedback> feedback = new List<Feedback>();
            Random rnd = new Random();

            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum < DateTime.Now)
                {
                    feedback.Add(new Feedback
                    {
                        ID = feedback.Count,
                        Klantnummer = reservering.klantnummers[0],
                        reservering_ID = reservering.ID,
                        message = "",
                        recipient = database.werknemers[rnd.Next(0, database.werknemers.Count)].ID
                    });
                }
            }

            database.feedback = feedback;
            io.Savedatabase(database);
        }

        #endregion

        #region Medewerkers

        /// <summary>
        /// This function makes employee's and saves them in the database
        /// </summary>
        /// <param name="amount">This is the amount of employee's you want to make</param>
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
                    prestatiebeloning = rnd.Next(0 , 30) / 100,
                    ID = werknemers.Count,
                    Klantgegevens = new Klantgegevens
                    {
                        voornaam = names[rnd.Next(0, 2)][rnd.Next(0, 20)],
                        achternaam = names[2][rnd.Next(0, 40)],
                    },
                });
            }

            database.werknemers = werknemers;
            io.Savedatabase(database);
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
