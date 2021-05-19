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
using System.Diagnostics;
using System.ComponentModel.Design;

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
           //database = io.GetDatabase();
        }

        public void Debug()
        {
            Make_menu();
            Fill_Userdata(10);
            Fill_reservations_threading(24, 500, 1, 3, 1, 9);
            Maak_werknemer(10);
            //Save_expenses();
            Make_reviews();
            //Make_feedback();
            //Save_Sales();

            //Inkomsten inkomsten = Sales(new DateTime(DateTime.Now.Year, 9, 9, 10, 0, 0), new DateTime(DateTime.Now.Year, 9, 9, 22, 59, 0));
            //database.inkomsten = inkomsten;

            //List<Tuple<DateTime, List<Tafels>>> test = Reservering_beschikbaarheid(Calc_totale_beschikbaarheid(9, 9, 9, 9), database.reserveringen);
        }

        #region Reserveringen

        public void Fill_reservations_threading(int threads, int amount, int start_month, int stop_month, int start_day, int stop_day)
        {
            Thread[] reservation_thread = new Thread[threads];
            database = io.GetDatabase();
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
                int aantal = 0;
                switch (tafels.Count)
                {
                    case 1:
                        for (int c = 0; c < rnd.Next(1, 5); c++)
                        {
                            aantal++;
                        }
                        break;
                    case 2:
                        for (int c = 0; c < rnd.Next(5, 9); c++)
                        {
                            aantal++;
                        }
                        break;
                    case 3:
                        for (int c = 0; c < rnd.Next(9, 13); c++)
                        {
                            aantal++;
                        }
                        break;
                }

                List<Gerechten> gerechten = Make_dishes(aantal * 3, beschikbaar[pos].Item1, ingredient_temp);
                List<int> gerechten_ID = gerechten.Select(g => g.ID).ToList();

                reserveringen_list.Add(new Reserveringen
                {
                    datum = beschikbaar[pos].Item1,
                    ID = reserveringen_list.Count,
                    tafels = tafels,
                    klantnummer = database.login_gegevens[rnd.Next(database.login_gegevens.Count)].klantgegevens.klantnummer,
                    gerechten_ID = gerechten_ID,
                    aantal = aantal,
                });
            }
        }

        private List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(List<Tuple<DateTime, List<Tafels>>> beschikbaar, List<Reserveringen> reserveringen)
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
                        if ((location + b) >= beschikbaar.Count)
                        {
                            for (int e = 0; e < 8-b; e++)
                            {
                                beschikbaar[location - (b+e)] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * (-b-e)), beschikbaar[location - (b+e)].Item2.Except(removed_tables).ToList());
                                if (beschikbaar[location - (b+e)].Item2.Count == 0)
                                {
                                    beschikbaar.RemoveAt(location - (b+e));
                                }
                            }
                            break;
                        }
                        
                        beschikbaar[location + b] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());

                        if (location - b >= 0)
                        {
                            beschikbaar[location - b] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * -b), beschikbaar[location - b].Item2.Except(removed_tables).ToList());
                            if (beschikbaar[location - b].Item2.Count == 0)
                            {
                                beschikbaar.RemoveAt(location - b);
                            }
                        }
                        
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
            database = io.GetDatabase();
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
        public List<Gerechten> Make_dishes(int amount, DateTime bestel_Datum, BlockingCollection<Ingredient> ingredient_temp)
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

        public List<Gerechten> Get_standard_dishes()
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
        public List<Gerechten> Make_dishes(List<string> names, List<bool> populair, List<bool> archived, List<bool> special, List<double> price)
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
            database = io.GetDatabase();
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
                        geb_datum = new DateTime(rnd.Next(1929, 2006), rnd.Next(1, 13), rnd.Next(1, 29), 1, 0, 0),
                        klantnummer = a,
                    }
                });
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
            database = io.GetDatabase();
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
            database = io.GetDatabase();
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
            database = io.GetDatabase();
            if (database.reserveringen == null || database.werknemers == null || database.ingredienten == null) return;

            Uitgaven uitgaven = database.uitgaven;
            uitgaven.ingredienten = new List<Tuple<int, DateTime>>();
            uitgaven.werknemer = new List<Tuple<int, DateTime>>();
            foreach (var werknemer in database.werknemers)
            {
                uitgaven.werknemer.Add(Tuple.Create(werknemer.ID, DateTime.Now));
            }

            foreach (var ingredient in database.ingredienten)
            {
                uitgaven.ingredienten.Add(Tuple.Create(ingredient.ID, DateTime.Now));
            }

            uitgaven.inboedel = new List<Inboedel>();
            for (int a = 0; a < 100; a++)
            {
                uitgaven.inboedel.Add(new Inboedel
                {
                    ID = a * 5,
                    item_Naam = "Tafel nummer: " + a,
                    prijs = 50,
                    verzendkosten = 5,
                    datum = DateTime.Now
                });

                for (int b = 1; b < 5; b++)
                {
                    uitgaven.inboedel.Add(new Inboedel
                    {
                        ID = a * 5 + b,
                        item_Naam = "Stoel",
                        prijs = 20,
                        verzendkosten = 3,
                        datum = DateTime.Now
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
            database = io.GetDatabase();
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
                        Klantnummer = reservering.klantnummer,
                        reservering_ID = reservering.ID,
                        Rating = rnd.Next(1, 6),
                        datum = reservering.datum.AddDays(rnd.Next(0, 50))
                    });

                    if (rnd.Next(5) == 3)
                    {
                        reviews[reviews.Count - 1].annomeme = true;
                        reviews[reviews.Count - 1].Klantnummer = -1;
                        reviews[reviews.Count - 1].reservering_ID = -1;
                        reviews[reviews.Count - 1].datum = new DateTime();
                    }

                    switch (reviews[reviews.Count - 1].Rating)
                    {
                        case 1:
                            reviews[reviews.Count - 1].message = "Verschikkelijk restaurant, hier kom ik nooit meer! Wie die sushipizza heeft uitgevonden mag branden in hell!";
                            break;
                        case 2:
                            reviews[reviews.Count - 1].message = "De service was wel goed, maar het eten wat niet zo goed. Ik denk dat ik hier niet meer terug wil komen. Geen aanrader voor vrienden!";
                            break;
                        case 3:
                            reviews[reviews.Count - 1].message = "Niet goed, niet slecht. Eten is te doen. Service was prima, ik kom nog wel terug.";
                            break;
                        case 4:
                            reviews[reviews.Count - 1].message = "gewoon goed! niet meer te zeggen.";
                            break;
                        case 5:
                            reviews[reviews.Count - 1].message = "OMG, die sushipiza was amazing!!! Dit is het beste restaurant ever, nog nooit zo'n hipster restaurant gezien in mijn leven. Ik kom hier zeker terug!!!";
                            break;
                    }
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
            database = io.GetDatabase();
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
                        Klantnummer = reservering.klantnummer,
                        reservering_ID = reservering.ID,
                        message = "",
                        recipient = database.werknemers[rnd.Next(0, database.werknemers.Count)].ID,
                        datum = reservering.datum.AddDays(rnd.Next(0, 50))
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
            database = io.GetDatabase();

            string[][] names = Make_Names();

            List<Werknemer> werknemers = new List<Werknemer>();
            Random rnd = new Random();

            for (int a = 0; a < amount; a++)
            {
                werknemers.Add(new Werknemer
                {
                    salaris = 3000,
                    inkomstenbelasting = 0.371,
                    prestatiebeloning = rnd.Next(0, 30) / 100,
                    ID = werknemers.Count,
                    login_gegevens = new Login_gegevens
                    {
                        email = names[rnd.Next(0, 2)][rnd.Next(0, 20)] + "," + names[2][rnd.Next(0, 40)] + "@gmail.com",
                        password = "0000",
                        klantgegevens = new Klantgegevens
                        {
                            voornaam = names[rnd.Next(0, 2)][rnd.Next(0, 20)],
                            achternaam = names[2][rnd.Next(0, 40)],
                        },
                    },
                });
            }

            database.eigenaar = new Eigenaar {
                ID = 0,
                salaris = 5000,
                inkomstenbelasting = 0.371,
                prestatiebeloning = rnd.Next(0, 30) / 100,
                login_gegevens = new Login_gegevens
                {
                    email = "Natnael.Tefera@gmail.com",
                    password = "0000",
                    klantgegevens = new Klantgegevens
                    {
                        voornaam = "Natnael",
                        achternaam = "Tefera",
                    },
                },
            };

            database.werknemers = werknemers;
            io.Savedatabase(database);
        }

        #endregion
    }





    public class ViewReviewScreen : Screen
    {
        public override int DoWork()
        {
            List<Review> reviews = new List<Review>();
            reviews = io.GetReviews(ingelogd.klantgegevens).OrderBy(s => s.datum).ToList();
            if (reviews.Count == 0)
            {
                Console.WriteLine("U heeft nog geen reviews");
                Console.WriteLine("druk op een knop om terug te gaan");
                Console.ReadKey();
                return 5;
            }

            Console.WriteLine(GetGFLogo(5));
            Console.WriteLine("Hier kunt u uw eigen reviews zien en bewerken:");
            Console.WriteLine("[1] Laat al uw reviews zien");
            Console.WriteLine("[2] Laat al uw reviews zien vanaf een datum (genoteerd als 1-1-2000)");
            Console.WriteLine("[3] Laat al uw reviews zien op rating");
            Console.WriteLine("[4] Ga terug naar klant menu scherm");

            ConsoleKeyInfo key = Console.ReadKey();
            Console.ReadKey();
            if (IsKeyPressed(key, ESCAPE_KEY))
            {
                return 5;
            }
            else if (IsKeyPressed(key, "D1"))
            {
                int page = 0;
                double pos = 0;
                List<string> pages = new List<string>();
                do
                {
                    pages = new List<string>();
                    List<List<string>> reviewstring = ReviewsToString(reviews);
                    List<string> boxes = new List<string>();
                    for (int a = 0; a < reviewstring.Count; a++)
                    {
                        if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (pos % 2 == 0 || pos == 0)
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 48),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 48),
                                    new string(' ', 50) + "##" + new string(' ', 50) }));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 48 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 48 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##" + new string(' ', 50)}));
                            }
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true));
                        }
                    }

                    pages = MakePages(boxes, 3);
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(3));
                    Console.WriteLine($"Dit zijn uw reviews op pagina {page} van de {pages.Count - 1}:");
                    Console.WriteLine(pages[page] + new string('#', 108));

                    var result = Nextpage(page, pages.Count - 1, pos, boxes.Count - 1, 10);
                    pos = result.Item3;
                    if (result.Item2 != -1 && result.Item2 != -2)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        return EditReview(MakeReviewBox(reviews[Convert.ToInt32(pos)]), reviews[Convert.ToInt32(pos)]);
                    }
                    else if (result.Item1 == -2 && result.Item2 == -2)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(1));
                        Console.WriteLine(MakeReviewBox(reviews[Convert.ToInt32(pos)]) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                        (string, int) input = AskForInput(10);
                        Console.ReadKey();
                        if (input.Item2 != -1)
                        {
                            return input.Item2;
                        }
                        else if (input.Item1 == "1")
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (input.Item1 == "ja")
                        {
                            code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                            Console.WriteLine("\n Review is verwijdert");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 10;

                        }
                        else if (input.Item1 == "nee")
                        {
                            return 10;
                        }
                        else
                        {
                            Console.WriteLine("\n U moet wel een jusite keuze maken");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 10;
                        }
                    }
                    page = result.Item1;
                } while (true);
            }
            else if (IsKeyPressed(key, "D2"))
            {
                Console.WriteLine("\n Vul hieronder de datum in vanaf wanneer u uw reviews wilt zien");
                (string, int) choice = AskForInput(10);
                Console.ReadKey();
                if (choice.Item2 != -1)
                {
                    return choice.Item2;
                }
                int page = 0;
                try
                {

                    DateTime date = Convert.ToDateTime(choice.Item1);
                    if (date >= DateTime.Now)
                    {
                        Console.WriteLine("U moet wel een datum in het verleden invoeren.");
                        Console.WriteLine("Druk op en knop om verder te gaan.");
                        Console.ReadKey();
                        return 10;
                    }
                    double pos = 0;
                    List<string> pages = new List<string>();
                    do
                    {
                        pages = new List<string>();
                        List<Review> reviewsfiler = reviews.Where(d => d.datum >= date).ToList();
                        List<List<string>> reviewstring = ReviewsToString(reviewsfiler);
                        List<string> boxes = new List<string>();
                        for (int a = 0; a < reviewstring.Count; a++)
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (pos % 2 == 0 || pos == 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 48),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 48),
                                    new string(' ', 50) + "##" + new string(' ', 50) }));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 48 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 48 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##" + new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true));
                            }
                        }

                        pages = MakePages(boxes, 3);
                        if (pages.Count == 0)
                        {
                            Console.WriteLine("Vanaf deze datum heeft u nog geen reviews geschreven.");
                            Console.WriteLine("Druk op en knop om verder te gaan.");
                            Console.ReadKey();
                            return 10;
                        }
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(3));
                        Console.WriteLine($"Dit zijn uw reviews op pagina {page} van de {pages.Count - 1}:");
                        Console.WriteLine(pages[page] + new string('#', 108));
                        var result = Nextpage(page, pages.Count - 1, pos, boxes.Count - 1, 10);
                        pos = result.Item3;
                        if (result.Item2 != -1)
                        {
                            return result.Item2;
                        }
                        else if (result.Item1 == -1 && result.Item2 == -1)
                        {
                            return EditReview(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]), reviewsfiler[Convert.ToInt32(pos)]);
                        }
                        else if (result.Item1 == -2 && result.Item2 == -2)
                        {
                            Console.Clear();
                            Console.WriteLine(GetGFLogo(1));
                            Console.WriteLine(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                            (string, int) input = AskForInput(10);
                            Console.ReadKey();
                            if (input.Item2 != -1)
                            {
                                return input.Item2;
                            }
                            else if (input.Item1 == "1")
                            {
                                logoutUpdate = true;
                                Logout();
                                return 0;
                            }
                            else if (input.Item1 == "ja")
                            {
                                code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                                Console.WriteLine("\n Review is verwijdert");
                                Console.WriteLine("Druk op een knop om verder te gaan...");
                                Console.ReadKey();
                                return 10;

                            }
                            else if (input.Item1 == "nee")
                            {
                                return 10;
                            }
                            else
                            {
                                Console.WriteLine("\n U moet wel een jusite keuze maken");
                                Console.WriteLine("Druk op een knop om verder te gaan...");
                                Console.ReadKey();
                                return 10;
                            }
                        }
                        page = result.Item1;
                    } while (true);
                }
                catch
                {
                    Console.WriteLine("U moet wel een geldige datum invullen op deze manier: 1-1-2000");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return 10;
                }
            }
            else if (IsKeyPressed(key, "D3"))
            {
                Console.WriteLine("\n Vul hieronder de rating in warop u uw reviews wilt filteren.");
                (string, int) choice = AskForInput(10);
                Console.ReadKey();
                if (choice.Item2 != -1)
                {
                    return choice.Item2;
                }
                int page = 0;
                if (choice.Item1 != "1" && choice.Item1 != "2" && choice.Item1 != "3" && choice.Item1 != "4" && choice.Item1 != "5")
                {
                    Console.WriteLine("U moet wel een geldige rating invullen tussen de 1 en de 5.");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return 10;
                }
                double pos = 0;
                List<string> pages = new List<string>();
                do
                {
                    pages = new List<string>();
                    List<Review> reviewsfiler = reviews.Where(r => r.Rating == Convert.ToInt32(choice.Item1)).ToList();
                    List<List<string>> reviewstring = ReviewsToString(reviewsfiler);
                    List<string> boxes = new List<string>();

                    for (int a = 0; a < reviewstring.Count; a++)
                    {
                        if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                        {
                            if (pos % 2 == 0 || pos == 0)
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 48),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 48),
                                    new string(' ', 50) + "##" + new string(' ', 50) }));
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 48 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 48 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##" + new string(' ', 50)}));
                            }
                        }
                        else
                        {
                            boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 102, true));
                        }
                    }

                    pages = MakePages(boxes, 3);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(3));
                    Console.WriteLine($"Dit zijn uw reviews op pagina {page} van de {pages.Count - 1}:");
                    Console.WriteLine(pages[page] + new string('#', 108));
                    var result = Nextpage(page, pages.Count - 1, pos, boxes.Count - 1, 10);
                    pos = result.Item3;
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        return EditReview(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]), reviewsfiler[Convert.ToInt32(pos)]);
                    }
                    else if (result.Item1 == -2 && result.Item2 == -2)
                    {
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(1));
                        Console.WriteLine(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                        (string, int) input = AskForInput(10);
                        Console.ReadKey();
                        if (input.Item2 != -1)
                        {
                            return input.Item2;
                        }
                        else if (input.Item1 == "1")
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (input.Item1 == "ja")
                        {
                            code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                            Console.WriteLine("\n Review is verwijdert");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 10;

                        }
                        else if (input.Item1 == "nee")
                        {
                            return 10;
                        }
                        else
                        {
                            Console.WriteLine("\n U moet wel een jusite keuze maken");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 10;
                        }
                    }
                    page = result.Item1;
                } while (true);
            }
            else if (IsKeyPressed(key, "D4"))
            {
                return 5;
            }
            else if (IsKeyPressed(key, "D5"))
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("U moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 10;
            }
        }

        private int EditReview(string reviewstr, Review review)
        {
            Review newreview = new Review();
            Console.Clear();
            Console.WriteLine(GetGFLogo(6));
            Console.WriteLine("Hier kunt u een review bewerken:");
            Console.WriteLine(reviewstr + "\n");

            Console.WriteLine("Wilt u deze review anoniem maken? ja | nee");
            (string, int) input = AskForInput(10);
            Console.ReadKey();
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "ja")
            {
                newreview.annomeme = true;
                newreview.Klantnummer = -1;
                newreview.ID = review.ID;
                newreview.reservering_ID = -1;
                newreview.datum = new DateTime();
            }
            else if (input.Item1 == "nee")
            {
                newreview.annomeme = false;
                newreview.Klantnummer = review.Klantnummer;
                newreview.ID = review.ID;
                newreview.reservering_ID = review.reservering_ID;
                newreview.datum = review.datum;
            }
            else if (input.Item1 == "6")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }


            Console.WriteLine("\n Typ hier uw bericht: ");
            input = AskForInput(10);
            Console.ReadKey();
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1.Length > 160)
            {
                Console.WriteLine("\n Uw bericht mag niet langer zijn dan 160 tekens");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }
            else if (input.Item1.Length == 0)
            {
                Console.WriteLine("\n u moet wel een bericht achterlaten");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }
            else if (input.Item1 == "6")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            newreview.message = input.Item1;

            a:
            Console.WriteLine("\n Typ hier uw rating: ");
            input = AskForInput(10);
            Console.ReadKey();
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (!new List<string>{ "1", "2", "3", "4", "5"}.Contains(input.Item1))
            {
                Console.WriteLine($"\n {input.Item1} is geen geldige rating. U kunt 1 t/m 5 invullen.");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                goto a;
            }
            else if (input.Item1 == "6")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            newreview.Rating = Convert.ToInt32(input.Item1);

            b:
            Console.Clear();
            Console.WriteLine(GetGFLogo(6));
            Console.WriteLine("Bewerkte review:");
            if (!newreview.annomeme)
            {
                Console.WriteLine("Voornaam: " + ingelogd.klantgegevens.voornaam);
                Console.WriteLine("Achternaam: " + ingelogd.klantgegevens.achternaam);
            }
            Console.WriteLine("Bericht: " + newreview.message);
            Console.WriteLine("Rating: " + newreview.Rating + "\n");

            Console.WriteLine("Wilt u dit review bewerken en opslaan? ja | nee");
            input = AskForInput(10);
            Console.ReadKey();
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "ja")
            {
                if (!newreview.annomeme)
                {
                    code_gebruiker.OverwriteReview(newreview.ID, newreview.Rating, ingelogd.klantgegevens, newreview.message);
                }
                else
                {
                    code_gebruiker.OverwriteReview(newreview.ID, newreview.Rating, newreview.message);
                }
                Console.WriteLine("\n Review is bijgewerkt");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                return 10;
            }
            else if (input.Item1 == "nee")
            {
                return 10;
            }
            else if (input.Item1 == "6")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een knop om verder te gaan...");
                Console.ReadKey();
                goto b;
            }
        }

        private string MakeReviewBox(Review review)
        {
            string output = "";
            output += new string('#', 56) + "\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + "Voornaam: " + ingelogd.klantgegevens.voornaam + new string(' ', 50 - ("Voornaam: " + ingelogd.klantgegevens.voornaam).Length) + "  #\n";
            output += "#  " + "Achternaam: " + ingelogd.klantgegevens.achternaam + new string(' ', 50 - ("Achternaam: " + ingelogd.klantgegevens.achternaam).Length) + "  #\n";
            
            List<string> msgparts1 = new List<string>();
            string message = review.message;

            if (message.Length > 50 - "Review: ".Length)
            {
                msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Review: ").Length).LastIndexOf(' ')));
                message = message.Remove(0, msgparts1[0].Length + 1);
                int count = 1;
                while (message.Length > 50)
                {
                    msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                    message = message.Remove(0, msgparts1[count].Length + 1);
                    count++;
                }
                msgparts1.Add(message);

                output += "#  " + "Review: " + msgparts1[0] + new string(' ', 50 - ("Review: " + msgparts1[0]).Length) + "  #\n";
                for (int a = 1; a < msgparts1.Count; a++)
                {
                    output += "#  " + msgparts1[a] + new string(' ', 50 - msgparts1[a].Length) + "  #\n";
                }
            }
            else
            {
                output += "#  " + "Review: " + message + new string(' ', 50 - ("Review: " + message).Length) + "  #\n";
            }

            output += "#  " + "Rating: " + review.Rating + new string(' ', 50 - ("Rating: " + review.Rating).Length) + "  #\n";
            output += "#  " + "Datum: " + review.datum + new string(' ', 50 - ("Datum: " + review.datum).Length) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += "#  " + new string(' ', 50) + "  #\n";
            output += new string('#', 56);

            return output;
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class MakeReviewScreen : Screen
    {
        public override int DoWork()
        {
            //checkt of je een review mag maken of niet
            //pak alle oude reserveringen
            List<Reserveringen> reserveringen = new List<Reserveringen>(code_gebruiker.GetCustomerReservation(ingelogd.klantgegevens, false));
            //lijst met alle reserveringen die al een review hebben
            List<Reserveringen> reviewdReservervations = new List<Reserveringen>();
            //lijst met alle reviews van een klant
            List<Review> klantReviews = new List<Review>(io.GetReviews(ingelogd.klantgegevens));


            //slaat alle reserveringen die al een review hebben op en except deze uit de lijst zodat je alleen niet reviewde reserveringen overhoudt
            for (int i = 0; i < reserveringen.Count; i++)
            {
                for (int j = 0; j < klantReviews.Count; j++)
                {
                    if (reserveringen[i].ID == klantReviews[j].reservering_ID)
                    {
                        reviewdReservervations.Add(reserveringen[i]);
                        break;
                    }
                }
            }
            reserveringen = reserveringen.Except(reviewdReservervations).ToList();

            //main logo
            Console.WriteLine(GFLogo);


            //als er geen open reserveringen meer zijn voor review
            if (reserveringen.Count == 0)
            {
                Console.WriteLine("Er zijn momenteel geen oude reserveringen waarover u een review kunt schrijven.");
                Console.WriteLine("Druk op een toets om terug te gaan.");
                Console.ReadKey();
                //de return key
                return 5;
            }
            //checkt of de review die je hebt gekozen bestaat
            else
            {
                Console.WriteLine("Hier kunt u een review maken.");
                Console.WriteLine("U kunt een review schrijven per reservering.");
                Console.WriteLine("U kunt kiezen uit een van de volgende reserveringen:");

                //list met alle IDs van reserveringen die nog geen review hebben
                for (int i = 0; i < reserveringen.Count; i++)
                {
                    Console.WriteLine(reserveringen[i].ID + new string(' ', 8 - reserveringen[i].ID.ToString().Length) + "| " + reserveringen[i].aantal + new string(' ', 5 - reserveringen[i].aantal.ToString().Length) + "| " + reserveringen[i].datum);
                }

                Console.WriteLine("\"ID | aantal mensen | datum\"");
                Console.WriteLine("Is het formaat van deze weergave.\n");
                Console.WriteLine("Met het ID kunt u selecteren over welk bezoek u een review wilt schrijven.");
                Console.WriteLine("Het ID van u reservering:");

                (string, int) choice = ("", -1);
                bool succes = false;
                do
                {                   
                    choice = AskForInput(7);
                    if (choice.Item2 != -1)
                    {
                        return choice.Item2;
                    }
                    //als de input niet een van de getallen is in de lijst met IDs, invalid input
                    if (!reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(choice.Item1)))
                    {
                        Console.WriteLine("U moet wel een juist ID selecteren");
                        Console.WriteLine("Druk op een knop om verder te gaan...");
                        Console.ReadKey();
                    }
                    else
                    {
                        succes = true;
                    }
                } while (!succes);

                // checkt input: 1 voor anoniem, 2 voor normaal, 3 voor terug
                //de reservering ophalen waarover een review word geschreven, word later gebruikt
                Reserveringen chosenReservation = new Reserveringen();
                for (int i = 0; i < reserveringen.Count; i++)
                {
                    if (reserveringen[i].ID == Convert.ToInt32(choice.Item1))
                    {
                        chosenReservation = reserveringen[i];
                        break;
                    }
                }


                //begin met een een clear screen hier
                Console.Clear();
                Console.WriteLine(GetGFLogo(6));
                Console.WriteLine("Er is een mogelijkheid om anoniem een review te geven, anoniem houdt in:");
                Console.WriteLine("-> U naam word niet opgeslagen met de review.");
                Console.WriteLine("-> De review word niet gekoppeld aan deze reservering.");
                Console.WriteLine("-> U kunt deze review niet aanpassen of verwijderen.");
                //Console.WriteLine("Kies [1] voor het maken voor een anonieme review, kies [2] voor het maken van een normale review.");
                Console.WriteLine("[1] Anoniem");
                Console.WriteLine("[2] Normaal");
                Console.WriteLine("[3] Terug");
                choice = AskForInput(7);
                if (choice.Item2 != -1)
                {
                    return choice.Item2;
                }

                //list met mogelijke inputs
                List<string> possibleInput = new List<string> { "1", "2", "3", "6"};

                if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out int test))
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om terug te gaan.");
                    Console.ReadKey();
                    //naar welk scherm gereturned moet worden als de input incorrect is
                    return 7;
                }

                //clear screen
                Console.Clear();
                Console.WriteLine(GetGFLogo(6));


                if (choice.Item1 == "1")
                {
                    //anoniem Review, alleen rating en message
                    Console.WriteLine("De beoordeling die u dit restaurant wilt geven van 1 tot 5 waarin,");
                    Console.WriteLine("1 is het slechtste en 5 is het beste");

                    choice = AskForInput(7);
                    if (choice.Item2 != -1)
                    {
                        return choice.Item2;
                    }

                    possibleInput = new List<string> { "1", "2", "3", "4", "5", "6" };

                    //check voor de rating
                    if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out test))
                    {
                        Console.WriteLine("U heeft een incorrecte beoordeling ingevoerd.");
                        Console.WriteLine("Druk op een knop om terug te gaan.");
                        Console.ReadKey();

                        //naar welk scherm gereturned moet worden als de input incorrect is
                        return 7;
                    }
 
                    //zet rating naar een int, word later gebruikt
                    int rating = Convert.ToInt32(choice.Item1);

                    Console.WriteLine("Als laatste, de inhoud van u review.");
                    Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                    Console.WriteLine("Als u klaar bent met het typen van u review, type dan \"klaar\" op een nieuwe regel.");
                    Console.WriteLine("De inhoud van de review:");

                    string message = "";
                    string Line = Console.ReadLine();
                    message = Line;
                    while (Line != "klaar")
                    {
                        Line = Console.ReadLine();
                        if (Line == "klaar")
                        {
                            break;
                        }
                        else
                        {
                            message += " " + Line;
                        }
                    }
                    code_gebruiker.MakeReview(rating, message);
                    Console.WriteLine("Succesvol een review gemaakt.");
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    Console.ReadKey();

                    //return naar het vorige scherm pls
                    return 5;
                    
                }

                else if (choice.Item1 == "2")
                {
                    //rating, message en klantgegevens/naam klant
                    Console.WriteLine("De beoordeling die u dit restaurant wilt geven van 1 tot 5 waarin:");
                    Console.WriteLine("1 het slechtste is en 5 het beste.");

                    choice = AskForInput(7);

                    possibleInput = new List<string> { "1", "2", "3", "4", "5", "6" };

                    //check voor de rating
                    if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out test))
                    {
                        Console.WriteLine("U heeft een incorrecte beoordeling ingevoerd.");
                        Console.WriteLine("Druk op een knop om terug te gaan.");
                        Console.ReadKey();

                        //naar welk scherm gereturned moet worden als de input incorrect is
                        return 7;
                    }

                    //zet rating naar een int, word later gebruikt
                    int rating = Convert.ToInt32(choice.Item1);

                    Console.WriteLine("Als laatste, de inhoud van u review.");
                    Console.WriteLine("Met Enter gaat u verder op een nieuwe regel.");
                    Console.WriteLine("Als u klaar bent met het typen van u review, type dan \"klaar\" op een nieuwe regel.");
                    Console.WriteLine("De inhoud van de review:");

                    string message = "";
                    string Line = Console.ReadLine();
                    message = Line;
                    while (Line != "klaar")
                    {
                        Line = Console.ReadLine();
                        if (Line == "klaar")
                        {
                            break;
                        }
                        else
                        {
                            message += " " + Line;
                        }
                    }
                    code_gebruiker.MakeReview(rating, ingelogd.klantgegevens, message, chosenReservation);
                    Console.WriteLine("Succesvol een review gemaakt.");
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    //return naar het vorige scherm pls
                    return 5;
                    
                }
                else if (choice.Item1 == "3")
                {
                    //return naar vorig scherm
                    return 5;
                }
                else if (choice.Item1 == "6")
                {
                    logoutUpdate = true;
                    Logout();
                    return 0;
                }
                
                return 5;
            }
        }
        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }



    public partial class Code_Gebruiker_menu
    {

    }
    
    public partial class IO
    {

    }
}
