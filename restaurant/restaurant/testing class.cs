using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public void Debug()
        {
            Make_menu();
            Fill_Userdata(10);
            Fill_reservations_threading(24, 500, 5, 7, 1, 30);
            Maak_werknemer(10);
            Make_reviews();
            Make_feedback();
            Save_Sales();
            Save_expenses();
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

            if (ingredient_temp.Count != 0)
            {
                database.ingredienten.AddRange(ingredient_temp);
            }
            else
            {
                database.ingredienten = new List<Ingredient>();
            }
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

                List<int> gerechten_ID = new List<int>();
                List<Gerechten> gerechten = new List<Gerechten>();
                if (beschikbaar[pos].Item1.Date < DateTime.Now.Date.Date)
                {
                    gerechten = Make_dishes(aantal * 3, beschikbaar[pos].Item1);
                    gerechten_ID = gerechten.Select(g => g.ID).ToList();
                }
                else
                {
                    tafels = new List<Tafels>();
                }

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
                            for (int e = 0; e < 8 - b; e++)
                            {
                                beschikbaar[location - (b + e)] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * (-b - e)), beschikbaar[location - (b + e)].Item2.Except(removed_tables).ToList());
                                if (beschikbaar[location - (b + e)].Item2.Count == 0)
                                {
                                    beschikbaar.RemoveAt(location - (b + e));
                                }
                            }
                            break;
                        }

                        if (reserveringen[c].datum.AddMinutes(15 * b).Hour < 22)
                        {
                            beschikbaar[location + b] = Tuple.Create(reserveringen[c].datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());
                        }

                        if (location - b >= 0 && reserveringen[c].datum.AddMinutes(15 * -b).Hour > 9)
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

            database.ingredientenNamen = MakeIngredients();
            Menukaart menukaart = new Menukaart
            {
                gerechten = Get_standard_dishes(),
                dranken = GetDranken()
            };

            database.menukaart = menukaart;
            io.Savedatabase(database);
        }

        private List<IngredientType> MakeIngredients()
        {
            List<IngredientType> ingredients = new List<IngredientType>();

            ingredients.AddRange(new List<IngredientType>
            {
                new IngredientType
                {
                name = "tomatensaus",
                prijs = 1.2,
                dagenHoudbaar = 30,
                },
                new IngredientType
                {
                name = "mozarella",
                prijs = 1.6,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "basilicum",
                prijs = 0.3,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "tonijn",
                prijs = 2.6,
                dagenHoudbaar = 20,
                },
                new IngredientType
                {
                name = "salami",
                prijs = 2.2,
                dagenHoudbaar = 35,
                },
                new IngredientType
                {
                name = "gorgonzola",
                prijs = 1.7,
                dagenHoudbaar = 55,
                },
                new IngredientType
                {
                name = "provolone",
                prijs = 2.8,
                dagenHoudbaar = 60,
                },
                new IngredientType
                {
                name = "parmezaanse kaas",
                prijs = 2.9,
                dagenHoudbaar = 80,
                },
                new IngredientType
                {
                name = "pittige salami",
                prijs = 2.8,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "rode peper",
                prijs = 0.5,
                dagenHoudbaar = 90,
                },
                new IngredientType
                {
                name = "cherrytomaatjes",
                prijs = 1.8,
                dagenHoudbaar = 30,
                },
                new IngredientType
                {
                name = "rode ui",
                prijs = 0.3,
                dagenHoudbaar = 100,
                },
                new IngredientType
                {
                name = "roomsaus",
                prijs = 1.95,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "champignons",
                prijs = 3.0,
                dagenHoudbaar = 60,
                },
                new IngredientType
                {
                name = "peterselie",
                prijs = 0.6,
                dagenHoudbaar = 80,
                },
                new IngredientType
                {
                name = "knoflook",
                prijs = 0.2,
                dagenHoudbaar = 120,
                },
                new IngredientType
                {
                name = "olijfolie",
                prijs = 0.8,
                dagenHoudbaar = 300,
                },
                new IngredientType
                {
                name = "rucola",
                prijs = 1,
                dagenHoudbaar = 60,
                },
                new IngredientType
                {
                name = "zongedroogde tomaat",
                prijs = 2.4,
                dagenHoudbaar = 70,
                },
                new IngredientType
                {
                name = "pijnboompitten",
                prijs = 3.2,
                dagenHoudbaar = 95,
                },
                new IngredientType
                {
                name = "bolognesesaus",
                prijs = 2.8,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "rundergehakt",
                prijs = 3.4,
                dagenHoudbaar = 20,
                },
                new IngredientType
                {
                name = "geitenkaas",
                prijs = 2.1,
                dagenHoudbaar = 30,
                },
                new IngredientType
                {
                name = "gevulde pasta",
                prijs = 1.8,
                dagenHoudbaar = 50,
                },
                new IngredientType
                {
                name = "bospaddenstoelen",
                prijs = 2.2,
                dagenHoudbaar = 50,
                },
                new IngredientType
                {
                name = "truffelsaus",
                prijs = 3.8,
                dagenHoudbaar = 25,
                },
                new IngredientType
                {
                name = "mosselen",
                prijs = 4.1,
                dagenHoudbaar = 15,
                },
                new IngredientType
                {
                name = "oestersaus",
                prijs = 0.8,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "oesters",
                prijs = 8.4,
                dagenHoudbaar = 10,
                },
                new IngredientType
                {
                name = "rijst",
                prijs = 0.7,
                dagenHoudbaar = 110,
                },
                new IngredientType
                {
                name = "zeewier",
                prijs = 0.2,
                dagenHoudbaar = 235,
                },
                new IngredientType
                {
                name = "zalm",
                prijs = 3.9,
                dagenHoudbaar = 25,
                },
                new IngredientType
                {
                name = "octopus",
                prijs = 4.5,
                dagenHoudbaar = 20,
                },
                new IngredientType
                {
                name = "garnaal",
                prijs = 3.6,
                dagenHoudbaar = 25,
                },
                new IngredientType
                {
                name = "krab",
                prijs = 2.6,
                dagenHoudbaar = 35,
                },
                new IngredientType
                {
                name = "komkommer",
                prijs = 0.35,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "biefstuk",
                prijs = 3.2,
                dagenHoudbaar = 25,
                },
                new IngredientType
                {
                name = "salami",
                prijs = 2.2,
                dagenHoudbaar = 35,
                },
                new IngredientType
                {
                name = "chèvre",
                prijs = 1.8,
                dagenHoudbaar = 60,
                },
                new IngredientType
                {
                name = "roombrie",
                prijs = 2.7,
                dagenHoudbaar = 70,
                },
                new IngredientType
                {
                name = "camembert",
                prijs = 3.2,
                dagenHoudbaar = 50,
                },
                new IngredientType
                {
                name = "Roquefort",
                prijs = 3.1,
                dagenHoudbaar = 55,
                },
                new IngredientType
                {
                name = "port salut",
                prijs = 5.2,
                dagenHoudbaar = 40,
                },
                new IngredientType
                {
                name = "pesto",
                prijs = 1.2,
                dagenHoudbaar = 35,
                },
                new IngredientType
                {
                name = "gekruide kip",
                prijs = 4.6,
                dagenHoudbaar = 30,
                },
                new IngredientType
                {
                name = "ui",
                prijs = 0.2,
                dagenHoudbaar = 130,
                },
                new IngredientType
                {
                name = "gemendge sla",
                prijs = 1,
                dagenHoudbaar = 50,
                },
                new IngredientType
                {
                name = "kipfilet",
                prijs = 2,
                dagenHoudbaar = 25,
                },
                new IngredientType
                {
                name = "croutons",
                prijs = 1.5,
                dagenHoudbaar = 75,
                },
                new IngredientType
                {
                name = "cesare dressing",
                prijs = 0.95,
                dagenHoudbaar = 90,
                },
                new IngredientType
                {
                name = "gerookte zalm",
                prijs = 4,
                dagenHoudbaar = 30,
                },
                new IngredientType
                {
                name = "dressing",
                prijs = 2,
                dagenHoudbaar = 95,
                },
                new IngredientType
                {
                name = "rundercarpaccio",
                prijs = 1.1,
                dagenHoudbaar = 30,
                },
            });

            return ingredients;
        }

        private BlockingCollection<string> Maak_gerechten(string gerecht_naam, DateTime bestel_Datum, BlockingCollection<Ingredient> ingredient_temp)
        {
            BlockingCollection<string> ingredienten_naam = new BlockingCollection<string>();
            if (database.ingredienten == null) database.ingredienten = new List<Ingredient>();

            if (gerecht_naam == "Pizza Salami")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Deeg",
                    prijs = 1,
                    dagenHoudbaar = 60,
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Salami",
                    prijs = 0.80,
                    dagenHoudbaar = 30
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Tomaten saus",
                    prijs = 0.60,
                    dagenHoudbaar = 15
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

            }
            else if (gerecht_naam == "Vla")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vanille vla",
                    prijs = 1.5,
                    dagenHoudbaar = 40
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

            }
            else if (gerecht_naam == "Hamburger")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Broodjes",
                    prijs = 0.10,
                    dagenHoudbaar = 10
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vlees",
                    prijs = 0.85,
                    dagenHoudbaar = 12
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Sla",
                    prijs = 0.05,
                    dagenHoudbaar = 35
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

            }
            else if (gerecht_naam == "Yoghurt")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Yoghurt",
                    prijs = 1.8,
                    dagenHoudbaar = 65
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

            }
            else if (gerecht_naam == "IJs")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Vanille ijs",
                    prijs = 1.85,
                    dagenHoudbaar = 25
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

            }
            else if (gerecht_naam == "Patat")
            {
                Ingredient ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Frituur vet",
                    prijs = 0.10,
                    dagenHoudbaar = 300
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Aardappelen",
                    prijs = 0.15,
                    dagenHoudbaar = 100
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);

                ingredient = new Ingredient
                {
                    bestel_datum = bestel_Datum,
                    ID = ingredient_temp.Count + 1,
                    name = "Friet saus",
                    prijs = 0.30,
                    dagenHoudbaar = 60
                };

                ingredient_temp.Add(ingredient);
                ingredienten_naam.Add(ingredient.name);
            }

            return ingredienten_naam;
        }

        //Deze functie is voor als je simpel een lijst van gerechten wilt zonder voorkeur
        public List<Gerechten> Make_dishes(int amount, DateTime bestel_Datum)
        {
            List<Gerechten> gerechten = new List<Gerechten>();
            List<Gerechten> Dishes = Get_standard_dishes();
            Random rnd = new Random();

            for (int a = 0; a <= amount; a++)
            {
                gerechten.Add(Dishes[rnd.Next(0, Dishes.Count)]);
            }

            return gerechten;
        }

        public List<Gerechten> Get_standard_dishes()
        {
            List<Gerechten> gerechten = new List<Gerechten>();
            gerechten.Add(new Gerechten
            {
                ID = 0,
                naam = "Tiramisu",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 5,
                dessert = true
            });
            gerechten.Add(new Gerechten
            {
                ID = 1,
                naam = "limoen-aarbei taart",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 5,
                dessert = true
            });
            gerechten.Add(new Gerechten
            {
                ID = 2,
                naam = "chocolade taart",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 5,
                dessert = true
            });
            gerechten.Add(new Gerechten
            {
                ID = 3,
                naam = "pizza margerita",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10,
                diner = true,
                Ingredienten = new List<string> 
                {
                    "tomatensaus",
                    "mozarella",
                    "basilicum"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 4,
                naam = "pizza tonno",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 11.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "tomatensaus",
                    "tonijn",
                    "mozarella",
                    "salami"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 5,
                naam = "pizza quattro",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "mozarella",
                    "gorgonzola",
                    "provolone",
                    "parmezaanse kaas"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 6,
                naam = "spicy pizza",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 11.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "tomatensaus",
                    "mozarella",
                    "pittige salami",
                    "rode pepers"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 7,
                naam = "pasta sbinala",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 11.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "tomatensaus",
                    "cherrytomaatjes",
                    "rode ui",
                    "rode peper"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 8,
                naam = "pasta funghi",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 11,
                diner = true,
                Ingredienten = new List<string>
                {
                    "roomsaus",
                    "champignons",
                    "peterselie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 9,
                naam = "pasta bianca",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 9,
                diner = true,
                Ingredienten = new List<string>
                {
                    "knoflook",
                    "peterselie",
                    "rode peper",
                    "olijfolie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 10,
                naam = "pasta pomodori",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "tomatensaus",
                    "rucola",
                    "zongedroogde tomaat",
                    "pijnboompitten",
                    "cherrytomaatjes",
                    "parmezaanse kaas"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 11,
                naam = "pasta bolognese",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "bolognesesaus",
                    "rundergehakt",
                    "parmezaanse kaas",
                    "peterselie"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 12,
                naam = "pasta quattro formaggi",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 11,
                diner = true,
                Ingredienten = new List<string>
                {
                    "roomsaus",
                    "mozarella",
                    "geitenkaas",
                    "provolone",
                    "gorgonzola",
                    "pijnboompitten"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 13,
                naam = "ravioli formioli",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 12.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "gevulde pasta",
                    "bospaddenstoelen",
                    "roomsaus",
                    "truffelsaus",
                    "champignons",
                    "parmezaanse kaas"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 14,
                naam = "mosselen",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 14,
                diner = true,
                Ingredienten = new List<string>
                {
                    "mosselen"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 15,
                naam = "oesters",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 18,
                diner = true,
                Ingredienten = new List<string>
                {
                    "oestersaus",
                    "oesters"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 16,
                naam = "sushi plank",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 12.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "rijst",
                    "zeewier",
                    "zalm",
                    "tonijn",
                    "octopus",
                    "garnaal",
                    "krab",
                    "komkommer"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 17,
                naam = "sashimi",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 13.5,
                diner = true,
                Ingredienten = new List<string>
                {
                    "biefstuk",
                    "zalm",
                    "tonijn",
                    "rucola",
                    "rijst"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 18,
                naam = "6x Tempura",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 12,
                diner = true,
                Ingredienten = new List<string>
                {
                    "garnalen"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 19,
                naam = "croissant",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 2.5,
                ontbijt = true,
                lunch = true,
                Ingredienten = new List<string>
                {

                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 20,
                naam = "stokbrood kruidenboter",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 4,
                ontbijt = true,
                lunch = true,
                Ingredienten = new List<string>
                {

                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 21,
                naam = "stokbrood met kaasplank",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 7,
                lunch = true,
                Ingredienten = new List<string>
                {
                    "chèvre",
                    "roombrie",
                    "camembert",
                    "Roquefort",
                    "port salut"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 22,
                naam = "panini mozarella",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 6,
                lunch = true,
                Ingredienten = new List<string>
                {
                    "mozarella",
                    "cherrytomaat",
                    "basilicum",
                    "pesto"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 23,
                naam = "panini pollo",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 7,
                lunch = true,
                Ingredienten = new List<string>
                {
                    "gekruide kip",
                    "mozarella",
                    "ui",
                    "rucola",
                    "pesto"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 24,
                naam = "cesare",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 7.5,
                lunch = true,
                diner = true,
                Ingredienten = new List<string>
                {
                    "rucola",
                    "gemendge sla",
                    "kipfilet",
                    "croutons",
                    "parmezaanse kaas",
                    "cesare dressing"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 25,
                naam = "Marinara",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10,
                lunch = true,
                diner = true,
                Ingredienten = new List<string>
                {
                    "gemendge sla",
                    "tonijn",
                    "gerookte zalm",
                    "cherrytomaatjes",
                    "rode ui",
                    "dressing"
                }
            });
            gerechten.Add(new Gerechten
            {
                ID = 26,
                naam = "carpaccio",
                is_populair = false,
                is_gearchiveerd = false,
                special = false,
                prijs = 10.5,
                lunch = true,
                diner = true,
                Ingredienten = new List<string>
                {
                    "rundercarpaccio",
                    "rucola",
                    "parmezaanse kaas",
                    "pijnboompitten",
                    "truffelsaus"
                }
            });

            return gerechten;
        }

        public List<Dranken> GetDranken()
        {
            List<Dranken> dranken = new List<Dranken>();

            dranken.Add(new Dranken
            {
                ID = 0,
                naam = "water",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 2
            });
            dranken.Add(new Dranken
            {
                ID = 1,
                naam = "cola",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 2,
                naam = "cola light",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 3,
                naam = "cola zero",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 4,
                naam = "chocomel",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.25
            });
            dranken.Add(new Dranken
            {
                ID = 5,
                naam = "fristi",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.25
            });
            dranken.Add(new Dranken
            {
                ID = 6,
                naam = "bitter lemon",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.25
            });
            dranken.Add(new Dranken
            {
                ID = 7,
                naam = "ice tea green",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 8,
                naam = "jus d'orange",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 4
            });
            dranken.Add(new Dranken
            {
                ID = 9,
                naam = "bruisend water",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 2.5
            });
            dranken.Add(new Dranken
            {
                ID = 10,
                naam = "fanta",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 11,
                naam = "warme chocolademelk",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3.75
            });
            dranken.Add(new Dranken
            {
                ID = 12,
                naam = "cappuccino",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3
            });
            dranken.Add(new Dranken
            {
                ID = 13,
                naam = "dubbele espresso",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 4.2
            });
            dranken.Add(new Dranken
            {
                ID = 14,
                naam = "koffie verkeerd",
                isGearchiveerd = false,
                heeftAlcohol = false,
                prijs = 3
            });
            dranken.Add(new Dranken
            {
                ID = 15,
                naam = "bier",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 3.5
            });
            dranken.Add(new Dranken
            {
                ID = 16,
                naam = "saké",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 4
            });
            dranken.Add(new Dranken
            {
                ID = 17,
                naam = "rode wijn",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 3.75
            });
            dranken.Add(new Dranken
            {
                ID = 18,
                naam = "witte wijn",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 3.75
            });
            dranken.Add(new Dranken
            {
                ID = 19,
                naam = "rosé",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 3.75
            });
            dranken.Add(new Dranken
            {
                ID = 20,
                naam = "rum",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 3.75
            });
            dranken.Add(new Dranken
            {
                ID = 21,
                naam = "wodka",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 5
            });
            dranken.Add(new Dranken
            {
                ID = 22,
                naam = "jägermeister",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 4
            });
            dranken.Add(new Dranken
            {
                ID = 23,
                naam = "gin",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 5
            });
            dranken.Add(new Dranken
            {
                ID = 24,
                naam = "whisky",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 5.5
            });
            dranken.Add(new Dranken
            {
                ID = 25,
                naam = "cognac",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 5.5
            });
            dranken.Add(new Dranken
            {
                ID = 26,
                naam = "baileys",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 5.75
            });
            dranken.Add(new Dranken
            {
                ID = 27,
                naam = "grand marnier",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 6
            });
            dranken.Add(new Dranken
            {
                ID = 28,
                naam = "likor 43",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 6
            });
            dranken.Add(new Dranken
            {
                ID = 29,
                naam = "Southern comfort",
                isGearchiveerd = false,
                heeftAlcohol = true,
                prijs = 6
            });

            return dranken;
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
                if (database.reserveringen[a].datum < DateTime.Now && (database.reserveringen[a].isBetaald || database.reserveringen[a].tafels.Count > 0))
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

            Random rnd = new Random();
            Uitgaven uitgaven = database.uitgaven;
            uitgaven.werknemer = new List<Tuple<int, DateTime>>();
            foreach (var werknemer in database.werknemers)
            {
                for (int b = rnd.Next(9, 13); b > rnd.Next(0, 6); b--)
                {
                    uitgaven.werknemer.Add(Tuple.Create(werknemer.ID, DateTime.Now.AddMonths(DateTime.Now.Month - (DateTime.Now.Month + b))));
                }
            }

            uitgaven.inboedel = new List<Inboedel>();
            for (int a = 1; a <= 20; a++)
            {
                uitgaven.inboedel.Add(new Inboedel
                {
                    ID = (a - 1) * 5,
                    item_Naam = "Tafel nummer: " + a,
                    prijs = 50,
                    verzendkosten = 5,
                    datum = DateTime.Now
                });

                for (int b = 1; b < 5; b++)
                {
                    uitgaven.inboedel.Add(new Inboedel
                    {
                        ID = (a - 1) * 5 + b,
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
                if (reservering.datum < DateTime.Now && reservering.gerechten_ID != null)
                {
                    reviews.Add(new Review
                    {
                        ID = reviews.Count,
                        klantnummer = reservering.klantnummer,
                        reservering_ID = reservering.ID,
                        Rating = rnd.Next(1, 6),
                        datum = reservering.datum.AddDays(rnd.Next(0, 50))
                    });

                    if (rnd.Next(5) == 3)
                    {
                        reviews[reviews.Count - 1].annomeme = true;
                        reviews[reviews.Count - 1].klantnummer = -1;
                        reviews[reviews.Count - 1].reservering_ID = -1;
                        reviews[reviews.Count - 1].datum = new DateTime();
                    }

                    switch (reviews[reviews.Count - 1].Rating)
                    {
                        case 1:
                            reviews[reviews.Count - 1].message = "Verschikkelijk restaurant, hier kom ik nooit meer! Wie die sushipizza heeft uitgevonden mag branden in hell!";
                            break;
                        case 2:
                            reviews[reviews.Count - 1].message = "De service was wel goed, maar het eten was niet zo goed. Ik denk dat ik hier niet meer terug wil komen. Geen aanrader voor vrienden!";
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
                if (reservering.datum < DateTime.Now && reservering.gerechten_ID != null)
                {
                    feedback.Add(new Feedback
                    {
                        ID = feedback.Count,
                        klantnummer = reservering.klantnummer,
                        reservering_ID = reservering.ID,
                        message = "test message",
                        recipient = database.werknemers[rnd.Next(0, database.werknemers.Count)].ID,
                        datum = reservering.datum.AddDays(rnd.Next(0, 50))
                    });

                    if (rnd.Next(5) == 3)
                    {
                        feedback[feedback.Count - 1].annomeme = true;
                        feedback[feedback.Count - 1].klantnummer = -1;
                        feedback[feedback.Count - 1].reservering_ID = -1;
                        feedback[feedback.Count - 1].datum = new DateTime();
                    }
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
                    prestatiebeloning = rnd.Next(0, 30) / 100,
                    ID = werknemers.Count,
                    lease_auto = 500,
                    login_gegevens = new Login_gegevens
                    {
                        email = names[rnd.Next(0, 2)][rnd.Next(0, 20)] + "." + names[2][rnd.Next(0, 40)] + "@gmail.com",
                        password = "0000",
                        type = "Medewerker",
                        klantgegevens = new Klantgegevens
                        {
                            voornaam = names[rnd.Next(0, 2)][rnd.Next(0, 20)],
                            achternaam = names[2][rnd.Next(0, 40)],
                        },
                    },
                });
            }

            database.eigenaar = new Eigenaar
            {
                ID = 0,
                salaris = 5000,
                prestatiebeloning = rnd.Next(0, 30) / 100,
                login_gegevens = new Login_gegevens
                {
                    email = "Natnael.Tefera@gmail.com",
                    password = "0000",
                    type = "Eigenaar",
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

    public abstract partial class Screen
    {
        /// <summary>
        /// You use this function if you want to make 1 box
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">This is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a string with is a box of sym around it</returns>
        protected string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        /// <summary>
        /// you use this function if you want to make a list of boxes
        /// </summary>
        /// <param name="blocks">This is a list of list string where this is a list of list of lines</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a list of boxes</returns>
        protected List<string> BoxAroundText(List<List<string>> blocks, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            List<string> output = new List<string>();

            foreach (var input in blocks)
            {
                string block = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                foreach (var line in input)
                {
                    block += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
                }

                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                if (!openbottom)
                {
                    block += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                }

                output.Add(block);
            }
            return output;
        }

        /// <summary>
        /// You use this function if you want to make a  box with custom bottom text
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <param name="bottomtext">This is a list of strings where this is an extra list of lines</param>
        /// <returns>This function returns a list of strings with consists of a list of boxes with sym around it and has custom bottom text</returns>
        protected string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom, List<string> bottomtext)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int b = 0; b < bottomtext.Count; b++)
            {
                output += sym + new string(' ', spacingside) + bottomtext[b] + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        protected List<string> MakePages(List<string> alldata, int maxblocks)
        {
            string[] output = new string[alldata.Count / maxblocks + 1];

            for (int a = 0, b = 1; a < alldata.Count; a++)
            {
                if (a < maxblocks * b)
                {
                    output[b - 1] += alldata[a];
                }
                else
                {
                    b++;
                    output[b - 1] += alldata[a];
                }
            }

            List<string> done = output.ToList();
            done.RemoveAll(x => x == null);
            return done;
        }

        protected (int, int, double) Nextpage(int page, double pos, double maxpos, int screenIndex, List<Tuple<(int, int , double), string>> choices, List<string> text)
        {
            foreach (var item in text)
            {
                Console.WriteLine(item);
            }
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            do
            {
                key = new ConsoleKeyInfo();
                key = Console.ReadKey();
            } while (IsKeyPressed(key, ENTER_KEY));
            if (IsKeyPressed(key, ESCAPE_KEY))
            {
                return (page, screenIndex, pos);
            }
            if (IsKeyPressed(key, UP_ARROW))
            {
                if (pos % 2 != 0)
                {
                    if ((pos - 1 > 6 * page && page != 0) || (pos > 2 && page == 0))
                    {
                        pos -= 2;
                    }
                }
                else
                {
                    if ((pos > 6 * page && page != 0) || (pos > 1 && page == 0))
                    {
                        pos -= 2;
                    }
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, DOWN_ARROW))
            {
                if (pos % 2 != 0)
                {
                    if (((pos + 1 < 6 * (page + 1) && page != 0) || pos < 4) && pos < maxpos - 1)
                    {
                        pos += 2;
                    }
                }
                else
                {
                    if (((pos + 2 < 6 * (page + 1) && page != 0) || pos < 4) && pos < maxpos - 1)
                    {
                        pos += 2;
                    }
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, LEFT_ARROW))
            {
                if (pos % 2 != 0 && pos > 0)
                {
                    pos -= 1;
                }
                return (page, -1, pos);
            }
            else if (IsKeyPressed(key, RIGHT_ARROW))
            {
                if ((pos % 2 == 0 || pos == 0) && pos < maxpos)
                {
                    pos += 1;
                }
                return (page, -1, pos);
            }
            
            Console.ReadKey();
            if (IsKeyPressed(key, "D0") || IsKeyPressed(key, "NumPad0"))
            {
                logoutUpdate = true;
                Logout();
                return (page, 0, pos);
            }
            foreach (var choice in choices)
            {
                if (IsKeyPressed(key, choice.Item2))
                {
                    return choice.Item1;
                }
            }

            Console.WriteLine("U moet wel een juiste keuze maken...");
            Console.WriteLine("Druk op en knop om verder te gaan.");
            Console.ReadKey();
            return (page, -1, pos);
        }

        protected (int, int) Nextpage(int page, int maxpage, int screenIndex)
        {
            if (page < maxpage)
            {
                Console.WriteLine("[1] Volgende pagina");
                Console.WriteLine("[2] Terug");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex);
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page + 1, -1);
                }
                else if (IsKeyPressed(key, "D2"))
                {
                    return (page, screenIndex);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1);
                }
            }
            else
            {
                Console.WriteLine("[1] Terug");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex);
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page, screenIndex);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1);
                }
            }
        }

        /// <summary>
        /// You use this function if you want to scroll in a table
        /// </summary>
        /// <param name="page">This is the index of the current page</param>
        /// <param name="maxpage">This is the max amount of pages</param>
        /// <param name="pos">This is the current position you have selected</param>
        /// <param name="maxpos">This is the max position you can select</param>
        /// <param name="screenIndex">This is the index you want to return to when you press esc</param>
        /// <returns>The first int is the screenindex, the second int is the page number and the double is the position</returns>
        protected (int, int, double) NextpageTable(int page, int maxpage, double pos, double maxpos, int screenIndex)
        {
            if (page < maxpage)
            {
                Console.WriteLine("[1] Volgende pagina");
                Console.WriteLine("[2] Terug"); 
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex, pos);
                }
                else if(IsKeyPressed(key, UP_ARROW))
                {
                    if (pos > 0 && pos > page * 20)
                    {
                        return (page, -1, pos - 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                else if (IsKeyPressed(key, DOWN_ARROW))
                {
                    if (pos < maxpos && pos < 20 * (page + 1) - 1)
                    {
                        return (page, -1, pos + 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page + 1, -1, (page + 1) * 20);
                }
                else if (IsKeyPressed(key, "D2"))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, "D3"))
                {
                    return (-1, -1, 0);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0, pos);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1, pos);
                }
            }
            else
            {
                Console.WriteLine("[1] Terug");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, UP_ARROW))
                {
                    if (pos > 0 && pos > page * 20)
                    {
                        return (page, -1, pos - 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                else if (IsKeyPressed(key, DOWN_ARROW))
                {
                    if (pos < maxpos && pos < 20 * (page + 1) - 1)
                    {
                        return (page, -1, pos + 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, "D3"))
                {
                    return (-1, -1, 0);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0, pos);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1, pos);
                }
            }
        }

        protected List<List<string>> ReviewsToString(List<Review> reviews)
        {
            List<Klantgegevens> klantgegevens = io.GetCustomer(reviews.Select(i => i.klantnummer).ToList());
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < reviews.Count; a++)
            {
                List<string> block = new List<string>();
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                if (!reviews[a].annomeme)
                {
                    block.Add("Voornaam: " + klantgegevens[a].voornaam + new string(' ', 50 - ("Voornaam: " + klantgegevens[a].voornaam).Length));
                    block.Add("Achternaam: " + klantgegevens[a].achternaam + new string(' ', 50 - ("Achternaam: " + klantgegevens[a].achternaam).Length));
                }
                else
                {
                    block.Add("Anoniem" + new string(' ', 50 - "Anoniem".Length));
                    block.Add(new string(' ', 50));
                }

                List<string> msgparts1 = new List<string>();
                string message = reviews[a].message;

                if (message.Length > 50 - "Review: ".Length)
                {
                    if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                    {
                        msgparts1.Add(message.Substring(0, 50 - "Review: ".Length));
                    }
                    else
                    {
                        msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Review: ").Length).LastIndexOf(' ')));
                    }

                    message = message.Remove(0, msgparts1[0].Length + 1);
                    int count = 1;
                    while (message.Length > 50)
                    {
                        if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                        {
                            msgparts1.Add(message.Substring(0, 50));
                        }
                        else
                        {
                            msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                        }

                        message = message.Remove(0, msgparts1[count].Length + 1);
                        count++;
                    }
                    msgparts1.Add(message);

                    block.Add("Review: " + msgparts1[0] + new string(' ', 50 - ("Review: " + msgparts1[0]).Length));
                    for (int b = 1; b < 4; b++)
                    {
                        if (b < msgparts1.Count)
                        {
                            block.Add(msgparts1[b] + new string(' ', 50 - msgparts1[b].Length));
                        }
                        else
                        {
                            block.Add(new string(' ', 50));
                        }
                    }
                }
                else
                {
                    block.Add("Review: " + message + new string(' ', 50 - ("Review: " + message).Length));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                }

                block.Add("Beoordeling: " + reviews[a].Rating + new string(' ', 50 - ("Beoordeling: " + reviews[a].Rating).Length));
                if (!reviews[a].annomeme)
                {
                    block.Add("Datum: " + reviews[a].datum + new string(' ', 50 - ("Datum: " + reviews[a].datum).Length));
                }
                else
                {
                    block.Add(new string(' ', 50));
                }

                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));

                output.Add(block);
            }


            return output;
        }

        /// <summary>
        /// you use this function if you want to put 2 lists of lines together to make one big box
        /// </summary>
        /// <param name="input">This is the list of list string where this is a list of list lines</param>
        /// <returns>This returns a list of list lines</returns>
        protected List<List<string>> Makedubbelboxes(List<List<string>> input)
        {
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < input.Count; a += 2)
            {
                if (a == input.Count - 1)
                {
                    output.Add(input[a]);
                    break;
                }
                List<string> blocknew = new List<string>();
                List<string> blockold1 = input[a];
                List<string> blockold2 = input[a + 1];

                for (int b = 0; b < blockold1.Count; b++)
                {
                    blocknew.Add(blockold1[b] + "##  " + blockold2[b]);
                }
                output.Add(blocknew);
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        protected List<List<string>> FeedbackToString(List<Feedback> feedback)
        {
            List<Werknemer> werknemers = new List<Werknemer>(io.GetEmployee());
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < feedback.Count; a++)
            {
                List<string> block = new List<string>();
                //block += new string('#', 56);
                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                if (!feedback[a].annomeme)
                {
                    block.Add("Voornaam: " + ingelogd.klantgegevens.voornaam + new string(' ', 50 - ("Voornaam: " + ingelogd.klantgegevens.voornaam).Length));
                    block.Add("Achternaam: " + ingelogd.klantgegevens.achternaam + new string(' ', 50 - ("Achternaam: " + ingelogd.klantgegevens.achternaam).Length));
                }
                else
                {
                    block.Add("Anoniem" + new string(' ', 50 - "Anoniem".Length));
                    block.Add(new string(' ', 50));
                }

                List<string> msgparts1 = new List<string>();
                string message = feedback[a].message;

                if (message.Length > 50 - "Feedback: ".Length)
                {
                    if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                    {
                        msgparts1.Add(message.Substring(0, 50 - "Feedback: ".Length));
                    }
                    else
                    {
                        msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Feedback: ").Length).LastIndexOf(' ')));
                    }

                    message = message.Remove(0, msgparts1[0].Length + 1);
                    int count = 1;
                    while (message.Length > 50)
                    {
                        if (message.IndexOf(' ') > 50 || message.IndexOf(' ') == -1)
                        {
                            msgparts1.Add(message.Substring(0, 50));
                        }
                        else
                        {
                            msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                        }

                        message = message.Remove(0, msgparts1[count].Length + 1);
                        count++;
                    }
                    msgparts1.Add(message);

                    block.Add("Feedback: " + msgparts1[0] + new string(' ', 50 - ("Feedback: " + msgparts1[0]).Length));
                    for (int b = 1; b < 4; b++)
                    {
                        if (b < msgparts1.Count)
                        {
                            block.Add(msgparts1[b] + new string(' ', 50 - msgparts1[b].Length));
                        }
                        else
                        {
                            block.Add(new string(' ', 50));
                        }
                    }
                }
                else
                {
                    block.Add("Feedback: " + message + new string(' ', 50 - ("Feedback: " + message).Length));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                    block.Add(new string(' ', 50));
                }

                for (int i = 0; i < werknemers.Count; i++)
                {
                    if (werknemers[i].ID == feedback[a].recipient)
                    {
                        block.Add("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam + new string(' ', 50 - ("Ontvanger: " + werknemers[i].login_gegevens.klantgegevens.voornaam + " " + werknemers[i].login_gegevens.klantgegevens.achternaam).Length));
                    }
                }

                if (!feedback[a].annomeme)
                {
                    block.Add("Datum: " + feedback[a].datum + new string(' ', 50 - ("Datum: " + feedback[a].datum).Length));
                }
                else
                {
                    block.Add(new string(' ', 50));
                }

                block.Add(new string(' ', 50));
                block.Add(new string(' ', 50));
                //block += new string('#', 56);

                output.Add(block);
            }


            return output;
        }
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

            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u uw eigen reviews bekijken en bewerken.");
            Console.WriteLine("[1] Alle reviews");
            Console.WriteLine("[2] Reviews vanaf een bepaalde datum (genoteerd als dag-maand-jaar)");
            Console.WriteLine("[3] Reviews op beoordeling (1 t/m 5 - slechtst naar best)");
            Console.WriteLine("[4] Ga terug naar het klantenmenu");

            (string, int) input = AskForInput(5);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            if (input.Item1 == "1")
            {
                int page = 0;
                double pos = 0;
                List<string> pages = new List<string>();
                do
                {
                    pages = new List<string>();
                    List<List<string>> reviewstring = Makedubbelboxes(ReviewsToString(reviews));
                    List<string> boxes = new List<string>();
                    for (int a = 0; a < reviewstring.Count; a++)
                    {
                        if (a == reviewstring.Count - 1 && reviewstring[a][1].Length < 70)
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (a != 0 && a % 6 != 0)
                                {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }
                                else
                                {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }                                
                            }
                            else
                            {
                                if (a != 0 && a %6 != 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true));
                                }
                                
                            }
                        }
                        else
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (pos % 2 == 0 || pos == 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 50),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                            }
                        }
                    }

                    int uneven = 0;
                    pages = MakePages(boxes, 3);
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw reviews op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reviews.\nDe review met de tekst '[4] Bewerken en [5] Verwijderen' is de huidig geselecteerde review.");
                    if (reviewstring[reviewstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                    {
                        Console.WriteLine(pages[page] + new string('#', 56));
                        uneven = 1;
                    }
                    else
                    {
                        Console.WriteLine(pages[page] + new string('#', 110));
                    }

                    (int, int, double) result = (0, 0, 0);
                    if (page < pages.Count - 1)
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10, 
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 10, pos), "D2") , Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") }, 
                            new List<string> { "[1] Volgende pagina", "[2] Terug" });
                    }
                    else
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page, 10, pos), "D1"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                            new List<string> { "[1] Terug" });
                    }
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
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine(MakeReviewBox(reviews[Convert.ToInt32(pos)]) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                        input = AskForInput(10);                      
                        if (input.Item2 != -1)
                        {
                            return input.Item2;
                        }
                        else if (input.Item1 == "0")
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (input.Item1 == "ja")
                        {
                            code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                            Console.WriteLine("\nReview is succesvol verwijderd.");
                            Console.WriteLine("Druk op een toets om verder te gaan.");
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
                            Console.WriteLine("Druk op een toets om verder te gaan.");
                            Console.ReadKey();
                            return 10;
                        }
                    }
                    page = result.Item1;
                } while (true);
            }
            else if (input.Item1 == "2")
            {
                Console.WriteLine("\n Typ hieronder de datum(genoteerd als dag - maand - jaar).\nLET OP! De datum moet in het verleden zijn!");
                (string, int) choice = AskForInput(10);              
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
                        Console.WriteLine("\nU moet wel een datum in het verleden invoeren.");
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
                        List<List<string>> reviewstring = Makedubbelboxes(ReviewsToString(reviews));
                        List<string> boxes = new List<string>();
                        for (int a = 0; a < reviewstring.Count; a++)
                        {
                            if (a == reviewstring.Count - 1 && reviewstring[a][1].Length < 70)
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    if (a != 0 && a % 6 != 0)
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true));
                                    }

                                }
                            }
                            else
                            {
                                if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                                {
                                    if (pos % 2 == 0 || pos == 0)
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 50),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                                    }
                                    else
                                    {
                                        boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                                    }
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                                }
                            }
                        }

                        int uneven = 0;
                        pages = MakePages(boxes, 3);
                        if (pages.Count == 0)
                        {
                            Console.WriteLine("\nVanaf deze datum heeft u nog geen reviews geschreven.");
                            Console.WriteLine("Druk op en knop om verder te gaan.");
                            Console.ReadKey();
                            return 10;
                        }
                        Console.Clear();
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine($"Dit zijn uw reviews op pagina {page + 1} van de {pages.Count}:");
                        Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reviews.\nDe review met de tekst '[4] Bewerken en [5] Verwijderen' is de huidig geselecteerde review.");
                        if (reviewstring[reviewstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                        {
                            Console.WriteLine(pages[page] + new string('#', 56));
                            uneven = 1;
                        }
                        else
                        {
                            Console.WriteLine(pages[page] + new string('#', 110));
                        }

                        (int, int, double) result = (0, 0, 0);
                        if (page < pages.Count - 1)
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 10, pos), "D2"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                                new List<string> { "[1] Volgende pagina", "[2] Terug" });
                        }
                        else
                        {
                            result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10,
                                new List<Tuple<(int, int, double), string>> { Tuple.Create((page, 10, pos), "D1"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                                new List<string> { "[1] Terug" });
                        }
                        pos = result.Item3;
                        if (result.Item2 != -1 && result.Item2 != -1)
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
                            Console.WriteLine(GetGFLogo(true));
                            Console.WriteLine(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]) + "\n");
                            Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                            input = AskForInput(10);                          
                            if (input.Item2 != -1)
                            {
                                return input.Item2;
                            }
                            else if (input.Item1 == "0")
                            {
                                logoutUpdate = true;
                                Logout();
                                return 0;
                            }
                            else if (input.Item1 == "ja")
                            {
                                code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                                Console.WriteLine("\nReview is succesvol verwijderd.");
                                Console.WriteLine("Druk op een toets om verder te gaan.");
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
                                Console.WriteLine("Druk op een toets om verder te gaan.");
                                Console.ReadKey();
                                return 10;
                            }
                        }
                        page = result.Item1;
                    } while (true);
                }
                catch
                {
                    Console.WriteLine("\nU heeft een onjuiste datum ingevoerd.\nLet op het juiste formaat (dag-maand-jaar) en of de datum in het verleden is.");
                    Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                    Console.ReadKey();
                    return 10;
                }
            }
            else if (input.Item1 == "3")
            {
                Console.WriteLine("\nTyp hieronder de beoordeling (1 t/m 5):");
                (string, int) choice = AskForInput(10);
                if (choice.Item2 != -1)
                {
                    return choice.Item2;
                }
                int page = 0;
                if (choice.Item1 != "1" && choice.Item1 != "2" && choice.Item1 != "3" && choice.Item1 != "4" && choice.Item1 != "5")
                {
                    Console.WriteLine("\nU heeft een onjuiste beoordeling ingevoerd.\nLet op dat het een cijfer is tussen de 1 en de 5 (slechtst naar best).");
                    Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                    Console.ReadKey();
                    return 10;
                }
                double pos = 0;
                List<string> pages = new List<string>();
                do
                {
                    pages = new List<string>();
                    List<Review> reviewsfiler = reviews.Where(r => r.Rating == Convert.ToInt32(choice.Item1)).ToList();
                    List<List<string>> reviewstring = Makedubbelboxes(ReviewsToString(reviews));
                    List<string> boxes = new List<string>();

                    for (int a = 0; a < reviewstring.Count; a++)
                    {
                        if (a == reviewstring.Count - 1 && reviewstring[a][1].Length < 70)
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (a != 0 && a % 6 != 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                if (a != 0 && a % 6 != 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 50, true));
                                }

                            }
                        }
                        else
                        {
                            if (a == Convert.ToInt32(Math.Floor(pos / 2)))
                            {
                                if (pos % 2 == 0 || pos == 0)
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string>{
                                    "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length) + "##  " + new string(' ', 50),
                                    "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length) + "##  " + new string(' ', 50),
                                    new string(' ', 50) + "##  " + new string(' ', 50) }));
                                }
                                else
                                {
                                    boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true, new List<string> {
                                    new string(' ', 50) + "##  " + "[4] Bewerken" + new string(' ', 50 - "[4] Bewerken".Length),
                                    new string(' ', 50) + "##  " + "[5] Verwijderen" + new string(' ', 50 - "[5] Verwijderen".Length),
                                    new string(' ', 50) + "##  " + new string(' ', 50)}));
                                }
                            }
                            else
                            {
                                boxes.Add(BoxAroundText(reviewstring[a], "#", 2, 0, 104, true));
                            }
                        }
                    }

                    int uneven = 0;
                    pages = MakePages(boxes, 3);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw reviews op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine("Gebruik de pijltjestoetsen om te navigeren door de reviews.\nDe review met de tekst '[4] Bewerken en [5] Verwijderen' is de huidig geselecteerde review.");
                    if (reviewstring[reviewstring.Count - 1][1].Length < 70 && page == pages.Count - 1)
                    {
                        Console.WriteLine(pages[page] + new string('#', 56));
                        uneven = 1;
                    }
                    else
                    {
                        Console.WriteLine(pages[page] + new string('#', 110));
                    }

                    (int, int, double) result = (0, 0, 0);
                    if (page < pages.Count - 1)
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page + 1, -1, (page + 1) * 6.0), "D1"), Tuple.Create((page, 10, pos), "D2"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                            new List<string> { "[1] Volgende pagina", "[2] Terug" });
                    }
                    else
                    {
                        result = Nextpage(page, pos, boxes.Count * 2 - (1 + uneven), 10,
                            new List<Tuple<(int, int, double), string>> { Tuple.Create((page, 10, pos), "D1"), Tuple.Create((-1, -1, pos), "D4"), Tuple.Create((-2, -2, pos), "D5") },
                            new List<string> { "[1] Terug" });
                    }
                    pos = result.Item3;
                    if (result.Item2 != -1 && result.Item2 != -1)
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
                        Console.WriteLine(GetGFLogo(true));
                        Console.WriteLine(MakeReviewBox(reviewsfiler[Convert.ToInt32(pos)]) + "\n");
                        Console.WriteLine("Weet u zeker dat u deze review wilt verwijderen? ja | nee");

                        input = AskForInput(10);                        
                        if (input.Item2 != -1)
                        {
                            return input.Item2;
                        }
                        else if (input.Item1 == "0")
                        {
                            logoutUpdate = true;
                            Logout();
                            return 0;
                        }
                        else if (input.Item1 == "ja")
                        {
                            code_gebruiker.DeleteReview(reviews[Convert.ToInt32(pos)].ID, ingelogd.klantgegevens);
                            Console.WriteLine("\nReview is succesvol verwijderd.");
                            Console.WriteLine("Druk op een toets om verder te gaan.");
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
            else if (input.Item1 == "4")
            {
                return 5;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("U moet wel een juiste keuze maken.");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                return 10;
            }
        }

        private int EditReview(string reviewstr, Review review)
        {
            Review newreview = new Review();
            Console.Clear();
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u een review bewerken.");
            Console.WriteLine(reviewstr + "\n");

            Console.WriteLine("Wilt u deze review anoniem maken? ja | nee");
            (string, int) input = AskForInput(10);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1 == "ja")
            {
                newreview.annomeme = true;
                newreview.klantnummer = -1;
                newreview.ID = review.ID;
                newreview.reservering_ID = -1;
                newreview.datum = new DateTime();
            }
            else if (input.Item1 == "nee")
            {
                newreview.annomeme = false;
                newreview.klantnummer = review.klantnummer;
                newreview.ID = review.ID;
                newreview.reservering_ID = review.reservering_ID;
                newreview.datum = review.datum;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }


            Console.WriteLine("\nTyp hieronder uw bericht (max. 160 tekens):");
            input = AskForInput(10);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (input.Item1.Length > 160)
            {
                Console.WriteLine("\nUw bericht mag niet langer zijn dan 160 tekens.");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }
            else if (input.Item1.Length == 0)
            {
                Console.WriteLine("\n u moet wel een bericht achterlaten");
                Console.WriteLine("DDruk op een toets om het opnieuw te proberen.");
                Console.ReadKey();
                EditReview(reviewstr, review);
                return 10;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            newreview.message = input.Item1;

        a:
            Console.WriteLine("\nTyp hieronder uw beoordeling (1 t/m 5):");
            input = AskForInput(10);
            if (input.Item2 != -1)
            {
                return input.Item2;
            }
            else if (!new List<string> { "1", "2", "3", "4", "5" }.Contains(input.Item1))
            {
                Console.WriteLine($"\n {input.Item1} is geen geldige beoordeling. U kunt 1 t/m 5 invullen (slechtst naar best).");
                Console.WriteLine("Druk op een toets om het opnieuw te proberen.");
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
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Bewerkte review:");
            if (!newreview.annomeme)
            {
                Console.WriteLine("Voornaam: " + ingelogd.klantgegevens.voornaam);
                Console.WriteLine("Achternaam: " + ingelogd.klantgegevens.achternaam);
            }
            Console.WriteLine("Bericht: " + newreview.message);
            Console.WriteLine("Beoordeling: " + newreview.Rating + "\n");

            Console.WriteLine("Wilt u deze review bewerken en opslaan? ja | nee");
            input = AskForInput(10);
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
                Console.WriteLine("\nDe review is succesvol bijgewerkt.");
                Console.WriteLine("Druk op een toets om verder te gaan.");
                Console.ReadKey();
                return 10;
            }
            else if (input.Item1 == "nee")
            {
                return 10;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("\n U moet wel een jusite keuze maken");
                Console.WriteLine("Druk op een toets om verder te gaan.");
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
                if (message.LastIndexOf(' ') > 50 || message.LastIndexOf(' ') == -1)
                {
                    msgparts1.Add(message.Substring(0, 50 - "Review: ".Length));
                }
                else
                {
                    msgparts1.Add(message.Substring(0, message.Substring(0, 50 - ("Review: ").Length).LastIndexOf(' ')));
                }
                    
                message = message.Remove(0, msgparts1[0].Length + 1);
                int count = 1;
                while (message.Length > 50)
                {
                    if (message.LastIndexOf(' ') > 50 || message.LastIndexOf(' ') == -1)
                    {
                        msgparts1.Add(message.Substring(0, 50));
                    }
                    else
                    {
                        msgparts1.Add(message.Substring(0, message.Substring(0, 50).LastIndexOf(' ')));
                    }
                        
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

            output += "#  " + "Beoordeling: " + review.Rating + new string(' ', 50 - ("Beoordeling: " + review.Rating).Length) + "  #\n";
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
            Console.WriteLine(GetGFLogo(true));


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
                Console.WriteLine("Hier kunt u een oude reservering kiezen, waarover u een review wilt schrijven.");
                Console.WriteLine("U kunt één review schrijven per reservering.");
                Console.WriteLine("\nU kunt kiezen uit één van de onderstaande reserveringen:");
                Console.WriteLine(new string('–', 44) + "\n|ID     |Aantal mensen | Datum             |\n" + new string('–', 44));

                //list met alle IDs van reserveringen die nog geen review hebben
                for (int i = 0; i < reserveringen.Count; i++)
                {
                    Console.WriteLine("|" + reserveringen[i].ID + new string(' ', 7 - reserveringen[i].ID.ToString().Length) + "| " + reserveringen[i].aantal + new string(' ', 13 - reserveringen[i].aantal.ToString().Length) + "| " + reserveringen[i].datum.ToShortDateString() + " " + reserveringen[i].datum.ToShortTimeString() + new string(' ', 18 - (reserveringen[i].datum.ToShortDateString() + " " + reserveringen[i].datum.ToShortTimeString()).Length) + "|\n" + new string('–', 44));
                }
               
                Console.WriteLine("\nU kunt d.m.v. het invullen van een ID een reservering selecteren, waarover u een review wilt schrijven.");

                (string, int) choice = ("", -1);
                bool succes = false;
                do
                {
                    Console.WriteLine("De ID van uw reservering:");
                    choice = AskForInput(5);
                    if (choice.Item2 != -1)
                    {
                        return choice.Item2;
                    }
                    else if (choice.Item1 == "0")
                    {
                        logoutUpdate = true;
                        Logout();
                        return 0;
                    }
                    //als de input niet een van de getallen is in de lijst met IDs, invalid input
                    if (!new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Contains(choice.Item1) && !reserveringen.Select(i => i.ID).ToList().Contains(Convert.ToInt32(choice.Item1)))
                    {
                        Console.WriteLine("\nU moet wel een juist ID selecteren.");
                        Console.WriteLine("Druk op een toets om verder te gaan.");
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
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Het is mogelijk om anoniem een review te schrijven, anoniem houdt in:");
                Console.WriteLine("-> Uw naam wordt niet opgeslagen bij de review.");
                Console.WriteLine("-> De review wordt niet gekoppeld aan deze reservering.");
                Console.WriteLine("-> U kunt deze review niet bewerken en/of verwijderen.");
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
                List<string> possibleInput = new List<string> { "1", "2", "3"};

                if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out int test))
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    Console.ReadKey();
                    //naar welk scherm gereturned moet worden als de input incorrect is
                    return 7;
                }

                //clear screen
                Console.Clear();
                Console.WriteLine(GetGFLogo(true));


                if (choice.Item1 == "1")
                {
                    //anoniem Review, alleen rating en message
                    Console.WriteLine("Op een schaal van 1 t/m 5 (slechtst naar best), welke beoordeling zou u uw ervaring in het restaurant geven?");

                    choice = AskForInput(7);
                    if (choice.Item2 != -1)
                    {
                        return choice.Item2;
                    }

                    possibleInput = new List<string> { "1", "2", "3", "4", "5"};

                    //check voor de rating
                    if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out test))
                    {
                        Console.WriteLine("\nU heeft een incorrecte beoordeling ingevoerd.");
                        Console.WriteLine("Druk op een toets om terug te gaan.");
                        Console.ReadKey();

                        //naar welk scherm gereturned moet worden als de input incorrect is
                        return 7;
                    }

                    //zet rating naar een int, word later gebruikt
                    int rating = Convert.ToInt32(choice.Item1);

                    Console.WriteLine("\nHieronder kunt u de inhoud van uw review schrijven (max. 160 tekens).");

                    succes = false;
                    string message = "";
                    do
                    {
                        message = Console.ReadLine();

                        if (message.Length > 160)
                        {
                            Console.WriteLine("Uw review mag niet langer zijn dan 160 tekens.\nHieronder kunt u opnieuw een review schrijven.");
                        }
                        else
                        {
                            succes = true;
                        }
                    } while (!succes);
                    

                    code_gebruiker.MakeReview(rating, message);
                    Console.WriteLine("Succesvol een review gemaakt.");
                    Console.WriteLine("Druk op een toets om terug te keren naar het klantenmenu.");
                    Console.ReadKey();

                    //return naar het vorige scherm pls
                    return 5;

                }
                else if (choice.Item1 == "2")
                {
                    //rating, message en klantgegevens/naam klant
                    Console.WriteLine("Op een schaal van 1 t/m 5 (slechtst naar best), welke beoordeling zou u uw ervaring in het restaurant geven?");

                    choice = AskForInput(7);

                    possibleInput = new List<string> { "1", "2", "3", "4", "5"};

                    //check voor de rating
                    if (!possibleInput.Contains(choice.Item1) || !Int32.TryParse(choice.Item1, out test))
                    {
                        Console.WriteLine("\nU heeft een incorrecte beoordeling ingevoerd.");
                        Console.WriteLine("Druk op een knop om terug te gaan.");
                        Console.ReadKey();

                        //naar welk scherm gereturned moet worden als de input incorrect is
                        return 7;
                    }

                    //zet rating naar een int, word later gebruikt
                    int rating = Convert.ToInt32(choice.Item1);

                    Console.WriteLine("\nHieronder kunt u de inhoud van uw review schrijven (max. 160 tekens).");

                    succes = false;
                    string message = "";
                    do
                    {
                        message = Console.ReadLine();

                        if (message.Length > 160)
                        {
                            Console.WriteLine("Uw review mag niet langer zijn dan 160 tekens.\nHieronder kunt u opnieuw een review schrijven.");
                        }
                        else
                        {
                            succes = true;
                        }
                    } while (!succes);
                    code_gebruiker.MakeReview(rating, ingelogd.klantgegevens, message, chosenReservation);
                    Console.WriteLine("\nSuccesvol een review aangemaakt.");
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
                else if (choice.Item1 == "0")
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

    public class IngredientsScreen : Screen
    {
        private List<string> IngredientToString(List<Ingredient> ingredients, List<string> names, double pos)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < names.Count; a++)
            {
                if (a == pos)
                {
                    output.Add("| " + names[a] + new string(' ', 30 - names[a].Length) +
                        " | " + ingredients.Select(n => n.name).Where(n => n == names[a]).ToList().Count + new string(' ', 11 - ingredients.Select(n => n.name == names[a]).ToList().Count.ToString().Length) +
                        " | € " + Convert.ToInt32(ingredients.Where(n => n.name == names[a]).Sum(p => p.prijs)) + ",00" + new string(' ', 8 - Convert.ToInt32(ingredients.Where(n => n.name == names[a]).Sum(p => p.prijs)).ToString().Length) + " [3] Verwijderen|\n" +
                        new string('–', 78) + "\n");
                }
                else
                {
                    output.Add("| " + names[a] + new string(' ', 30 - names[a].Length) +
                    " | " + ingredients.Select(n => n.name).Where(n => n == names[a]).ToList().Count + new string(' ', 11 - ingredients.Select(n => n.name == names[a]).ToList().Count.ToString().Length) +
                    " | € " + Convert.ToInt32(ingredients.Where(n => n.name == names[a]).Sum(p => p.prijs)) + ",00" + new string(' ', 8 - Convert.ToInt32(ingredients.Where(n => n.name == names[a]).Sum(p => p.prijs)).ToString().Length) + "                |\n" +
                    new string('–', 78) + "\n");
                }
            }
            return output;
        }

        private (int, int, double) NextpageTableExpitedIngredients(int page, int maxpage, double pos, double maxpos, int screenIndex)
        {
            if (page < maxpage)
            {
                Console.WriteLine("[1] Volgende pagina");
                Console.WriteLine("[2] Alle verlopen ingredienten verwijderen");
                Console.WriteLine("[3] Terug");

                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, UP_ARROW))
                {
                    if (pos > 0 && pos > page * 20)
                    {
                        return (page, -1, pos - 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                else if (IsKeyPressed(key, DOWN_ARROW))
                {
                    if (pos < maxpos && pos < 20 * (page + 1) - 1)
                    {
                        return (page, -1, pos + 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page + 1, -1, (page + 1) * 20);
                }
                else if (IsKeyPressed(key, "D3"))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, "D4"))
                {
                    return (-1, -1, 0);
                }
                else if (IsKeyPressed(key, "D2"))
                {
                    return (-2, -2, 0);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0, pos);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1, pos);
                }
            }
            else
            {
                Console.WriteLine("[1] Terug");
                Console.WriteLine("[2] Alle verlopen ingredienten verwijderen");
                ConsoleKeyInfo key = Console.ReadKey();
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, UP_ARROW))
                {
                    if (pos > 0 && pos > page * 20)
                    {
                        return (page, -1, pos - 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                else if (IsKeyPressed(key, DOWN_ARROW))
                {
                    if (pos < maxpos && pos < 20 * (page + 1) - 1)
                    {
                        return (page, -1, pos + 1);
                    }
                    else
                    {
                        return (page, -1, pos);
                    }
                }
                Console.ReadKey();
                if (IsKeyPressed(key, "D1"))
                {
                    return (page, screenIndex, pos);
                }
                else if (IsKeyPressed(key, "D2"))
                {
                    return (-2, -2, 0);
                }
                else if (IsKeyPressed(key, "D3"))
                {
                    return (-1, -1, 0);
                }
                else if (IsKeyPressed(key, "D0"))
                {
                    logoutUpdate = true;
                    Logout();
                    return (page, 0, pos);
                }
                else
                {
                    Console.WriteLine("U moet wel een juiste keuze maken...");
                    Console.WriteLine("Druk op en knop om verder te gaan.");
                    Console.ReadKey();
                    return (page, -1, pos);
                }
            }
        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u alle ingredienten zien die u nu heeft of nieuwe ingredienten aanmaken.");
            Console.WriteLine("[1] Laat aantal ingredienten op naam zien");
            Console.WriteLine("[2] Laat alle ingredienten zien die bijna verlopen zijn");
            Console.WriteLine("[3] Laat alle ingredienten zien die verlopen zijn");
            Console.WriteLine("[4] Voeg ingredienten toe aan magazijn");
            Console.WriteLine("[5] Voeg nieuwe ingredienten toe aan het systeem");
            Console.WriteLine("[6] Ga terug naar eigenaar menu scherm");

            (string, int) input = AskForInput(14);
            if (input.Item2 != -1)
            {
                return 11;
            }
            if (input.Item1 == "1")
            {
                if (code_eigenaar.GetIngredients().Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("u heeft geen ingredienten in uw magazijn.");
                    Console.WriteLine("Druk op een knop om terug te gaan.");
                    Console.ReadKey();
                    return 11;
                }

                List<Ingredient> ingredients = code_eigenaar.GetIngredients().OrderBy(i => i.name).ToList();
                List<string> names = ingredients.Select(n => n.name).Distinct().ToList();
                int page = 0;
                double pos = 0;
                do
                {                   
                    List<string> ingredientsString = IngredientToString(ingredients, names, pos);
                    List<string> pages = MakePages(ingredientsString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn de ingredienten op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 78) + "\n" + "|Naam                            |Hoeveelheid |Waarde                        |");
                    Console.WriteLine(new string('–', 78) + "\n" + pages[page]);
                    (int, int, double) result = NextpageTable(page, pages.Count - 1,pos , names.Count - 1,  14);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        code_eigenaar.DeleteIngredients(ingredients.Where(n => n.name == names[Convert.ToInt32(pos)]).ToList());
                        ingredients = code_eigenaar.GetIngredients().OrderBy(i => i.name).ToList();
                        names = ingredients.Select(n => n.name).Distinct().ToList();
                        page = 0;
                        pos = 0;
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "2")
            {
                if (code_eigenaar.GetIngredients().Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("u heeft geen ingredienten in uw magazijn.");
                    Console.WriteLine("Druk op een knop om terug te gaan.");
                    Console.ReadKey();
                    return 11;
                }

                List<Ingredient> ingredients = code_eigenaar.GetAlmostExpiredIngredients(7);
                List<string> names = ingredients.Select(n => n.name).Distinct().ToList();

                if (ingredients.Count == 0)
                {
                    Console.WriteLine("\nU heeft geen ingredienten die bijna verlopen zijn");
                    Console.WriteLine("Druk op een knop om verder te gaan...");
                    Console.ReadKey();
                    return 14;
                }
                int page = 0;
                double pos = 0;
                do
                {
                    List<string> ingredientsString = IngredientToString(ingredients, names, pos);
                    List<string> pages = MakePages(ingredientsString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn de bijna verlopen ingredienten op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 78) + "\n" + "|Naam                            |Hoeveelheid |Waarde                        |");
                    Console.WriteLine(new string('–', 78) + "\n" + pages[page]);
                    Console.WriteLine($"Totaal aantal ingredienten: {ingredients.Count}");
                    Console.WriteLine($"totaal waarde ingredienten: €{ Convert.ToInt32(ingredients.Sum(p => p.prijs))},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1,pos ,ingredients.Count - 1, 14);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        code_eigenaar.DeleteIngredients(ingredients.Where(n => n.name == names[Convert.ToInt32(pos)]).ToList());
                        ingredients = ingredients = code_eigenaar.GetAlmostExpiredIngredients(7);
                        names = ingredients.Select(n => n.name).Distinct().ToList();
                        page = 0;
                        pos = 0;
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "3")
            {
                if (code_eigenaar.GetIngredients().Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("u heeft geen ingredienten in uw magazijn.");
                    Console.WriteLine("Druk op een knop om terug te gaan.");
                    Console.ReadKey();
                    return 11;
                }

                List<Ingredient> ingredients = code_eigenaar.GetExpiredIngredients();
                List<string> names = ingredients.Select(n => n.name).Distinct().ToList();
                if (ingredients.Count == 0)
                {
                    Console.WriteLine("\nU heeft geen ingredienten die verlopen zijn");
                    Console.WriteLine("Druk op een knop om verder te gaan...");
                    Console.ReadKey();
                    return 14;
                }                
                int page = 0;
                double pos = 0;
                do
                {
                    List<string> ingredientsString = IngredientToString(ingredients, names, pos);
                    List<string> pages = MakePages(ingredientsString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn de verlopen ingredienten op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 78) + "\n" + "|Naam                            |Hoeveelheid |Waarde                        |");
                    Console.WriteLine(new string('–', 78) + "\n" + pages[page]);
                    Console.WriteLine($"Totaal aantal ingredienten: {ingredients.Count}");
                    (int, int, double) result = NextpageTableExpitedIngredients(page, pages.Count - 1,pos ,ingredients.Count - 1, 14);

                    if (result.Item2 != -1 && result.Item2 != -2)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        code_eigenaar.DeleteIngredients(ingredients.Where(n => n.name == names[Convert.ToInt32(pos)]).ToList());
                        ingredients = ingredients = code_eigenaar.GetExpiredIngredients();
                        names = ingredients.Select(n => n.name).Distinct().ToList();
                        page = 0;
                        pos = 0;

                        if (ingredients.Count == 0)
                        {
                            Console.WriteLine("\nU heeft geen ingredienten die verlopen zijn");
                            Console.WriteLine("Druk op een knop om verder te gaan...");
                            Console.ReadKey();
                            return 14;
                        }
                    }
                    else if (result.Item2 == -2)
                    {
                        code_eigenaar.DeleteExpiredIngredients();
                        return 14;
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "4")
            {
                if (code_eigenaar.GetIngredients().Count == 0)
                {
                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine("u heeft geen ingredienten in uw magazijn.");
                    Console.WriteLine("Druk op een knop om terug te gaan.");
                    Console.ReadKey();
                    return 11;
                }

                List<Ingredient> ingredienten = code_eigenaar.GetIngredients();
                List<IngredientType> ingredientNamen = io.ingredientNamen();
                List<int> ids = new List<int>();

                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Hier kunt u ingredienten toevoegen aan het magazijn.");
                Console.WriteLine(new string('–', 47) + "\n|ID      | Ingredient               |prijs    |\n" + new string('–', 47));
                for (int a = 0; a < ingredientNamen.Count; a++)
                {
                    ids.Add(a);
                    Console.WriteLine("|" + a + new string(' ', 8 - a.ToString().Length) + "| " + ingredientNamen[a].name + new string(' ', 25 - ingredientNamen[a].name.Length) + "| " + ingredientNamen[a].prijs + new string(' ', 8 - ingredientNamen[a].prijs.ToString().Length) + "|\n" + new string('–', 47));
                }
                Console.WriteLine("Kies hier het id van het ingredient die u wilt toevoegen aan het magazijn.");

                (string, int) result = ("", 0);
                bool succes = false;
                do
                {
                    result = AskForInput(14);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    if (!int.TryParse(result.Item1, out int test))
                    {
                        Console.WriteLine("U moet wel een nummer invullen.");
                    }
                    else if (!ids.Contains(Convert.ToInt32(result.Item1)))
                    {
                        Console.WriteLine("Dit nummer staat niet in de lijst.");
                    }
                    else
                    {
                        succes = true;
                    }

                } while (!succes);

                int pos = Convert.ToInt32(result.Item1);
                Console.WriteLine("\nTyp het aantal ingredienten die u aan het magazijn wilt toevoegen.");

                succes = false;
                do
                {
                    result = AskForInput(14);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (!int.TryParse(result.Item1, out int test))
                    {
                        Console.WriteLine("U moet wel een nummer invullen.");
                    }
                    else
                    {
                        succes = true;
                    }
                } while (!succes);

                int lastid = ingredienten[ingredienten.Count - 1].ID;
                List<Ingredient> ingredients = new List<Ingredient>();
                for (int b = 0; b < Convert.ToInt32(result.Item1); b++)
                {
                    ingredients.Add(new Ingredient 
                    { 
                        ID = lastid + b,
                        bestel_datum = DateTime.Now,
                        dagenHoudbaar = ingredientNamen[pos].dagenHoudbaar,
                        name = ingredientNamen[pos].name,
                        prijs = ingredientNamen[pos].prijs

                    });
                }

                code_eigenaar.SaveIngredients(ingredients);
                Console.WriteLine("\nIngredienten toegevoegd. Druk op een toets om terug te gaan.");
                Console.ReadKey();

                return 11;
            }
            else if (input.Item1 == "5")
            {
                List<string> ingredientNamen = io.ingredientNamen().Select(n => n.name).ToList();
                IngredientType newIngredient = new IngredientType();

                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine("Hier kunt u een nieuw ingredient aanmaken voor in het systeem. \nDit ingredient kunt u later toevoegen aan een nieuw gerecht die u maakt.\n");
                Console.WriteLine("Vul hier de naam van het ingredient in:");

                (string, int) result = ("", 0);
                bool succes = false;
                do
                {
                    result = AskForInput(11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (ingredientNamen.Contains(result.Item1))
                    {
                        Console.WriteLine("Deze naam bestaat al voor een ingredient.");
                    }
                    else if (!string.IsNullOrEmpty(result.Item1) && result.Item1.Length <= 25)
                    {
                        succes = true;
                    }
                    else if (string.IsNullOrEmpty(result.Item1))
                    {
                        Console.WriteLine("U moet wel een naam invoeren");
                    }
                    else
                    {
                        Console.WriteLine("De naam van het ingredient mag niet langer zijn dan 25 tekens");
                    }

                } while (!succes);

                newIngredient.name = result.Item1;

                Console.WriteLine("\nVoer hier de prijs in van het ingredient:");

                succes = false;
                do
                {
                    result = AskForInput(11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (!double.TryParse(result.Item1, out double test) || Convert.ToDouble(result.Item1) <= 0)
                    {
                        Console.WriteLine("Dit is geen geldige waarde. U kunt alleen (komma) getallen boven de 0 invoeren");
                    }
                    else if (!string.IsNullOrEmpty(result.Item1))
                    {
                        succes = true;
                    }
                    else
                    {
                        Console.WriteLine("U moet wel een bedrag invoeren");
                    }

                } while (!succes);

                result.Item1 = result.Item1.Replace('.', ',');
                newIngredient.prijs = Convert.ToDouble(result.Item1);

                Console.WriteLine("\nVoer hieronder het aantal dagen dat dit ingredient houdbaar is:");

                succes = false;
                do
                {
                    result = AskForInput(11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (!int.TryParse(result.Item1, out int test) || Convert.ToInt32(result.Item1) <= 0)
                    {
                        Console.WriteLine("Dit is geen geldige waarde. U kunt alleen getallen boven de 0 invoeren");
                    }
                    else if (!string.IsNullOrEmpty(result.Item1))
                    {
                        succes = true;
                    }
                    else
                    {
                        Console.WriteLine("U moet wel een aantal dagen invoeren");
                    }

                } while (!succes);

                newIngredient.dagenHoudbaar = Convert.ToInt32(result.Item1);

                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine(new string('#', 31));
                Console.WriteLine("#" + new string (' ', 29) + "#");
                Console.WriteLine("#" + new string(' ', 29) + "#");
                Console.WriteLine("#  " + newIngredient.name + new string(' ', 27 - newIngredient.name.Length) + "#");
                Console.WriteLine("#  €" + newIngredient.prijs + new string(' ', 26 - newIngredient.prijs.ToString().Length) + "#");
                Console.WriteLine("#  " + DateTime.Now.AddDays(newIngredient.dagenHoudbaar).ToShortDateString() + new string(' ', 27 - DateTime.Now.AddDays(newIngredient.dagenHoudbaar).ToShortDateString().Length) + "#");
                Console.WriteLine("#" + new string(' ', 29) + "#");
                Console.WriteLine("#" + new string(' ', 29) + "#");
                Console.WriteLine(new string('#', 31) + "\n");

                Console.WriteLine("Wilt u dit ingredient opslaan? ja | nee");
                succes = false;
                do
                {
                    result = AskForInput(11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }

                    if (result.Item1 != "nee" && result.Item1 != "ja")
                    {
                        Console.WriteLine("U moet wel een geldige keuze maken. Deze zijn ja of nee");
                    }
                    else if (result.Item1 == "nee")
                    {
                        return 14;
                    }
                    else
                    {
                        code_eigenaar.SaveIngredientName(newIngredient);
                        Console.WriteLine("\nHet ingredient is opgeslagen. Druk op en toets om terug te gaan.");
                        Console.ReadKey();
                        return 14;
                    }
                } while (!succes);

                return 14;
            }
            else if (input.Item1 == "6")
            {
                return 11;
            }
            else if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else
            {
                Console.WriteLine("\nU moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 14;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class IncomeScreen : Screen
    {
        private List<string> IncomeToStringMonths(List<(DateTime, double)> income, double pos, string optionMessage)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < income.Count; a++)
            {
                if (pos == a)
                {
                    output.Add("| " + income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddMonths(1).ToShortDateString() + new string(' ', 25 - (income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddMonths(1).ToShortDateString()).Length) +
                        " | " + "€" + income[a].Item2 + new string(' ', 12 - ("€" + income[a].Item2).Length) + $" {optionMessage}|\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
                else
                {
                    output.Add("| " + income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddMonths(1).ToShortDateString() + new string(' ', 25 - (income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddMonths(1).ToShortDateString()).Length) +
                        " | " + "€" + income[a].Item2 + new string(' ', 12 - ("€" + income[a].Item2).Length) + new string(' ', optionMessage.Length) + " |\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
            }

            return output;
        }

        private List<string> IncomeToStringWeeks(List<(DateTime, double)> income, double pos, string optionMessage)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < income.Count; a++)
            {
                if (pos == a)
                {
                    output.Add("| " + income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddDays(7).ToShortDateString() + new string(' ', 25 - (income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddDays(7).ToShortDateString()).Length) +
                        " | " + "€" + income[a].Item2 + new string(' ', 12 - ("€" + income[a].Item2).Length) + $" {optionMessage}|\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
                else
                {
                    output.Add("| " + income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddDays(7).ToShortDateString() + new string(' ', 25 - (income[a].Item1.ToShortDateString() + " - " + income[a].Item1.AddDays(7).ToShortDateString()).Length) +
                        " | " + "€" + income[a].Item2 + new string(' ', 12 - ("€" + income[a].Item2).Length) + new string(' ', optionMessage.Length) + " |\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
            }

            return output;
        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u alle inkomsten zien die u nu heeft");
            Console.WriteLine("[1] Laat al uw inkomsten zien per maand");
            Console.WriteLine("[2] Laat al uw inkomsten zien per week");
            Console.WriteLine("[3] Ga terug naar eigenaar menu scherm");
            (string, int) input = AskForInput(15);

            if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else if (input.Item1 == "1")
            {
                List<DateTime> dates = io.GetReservations().OrderBy(d => d.datum).Select(d => d.datum).Distinct().ToList();
                int months = ((dates[0].Year - DateTime.Now.Year) * 12) + DateTime.Now.Month - (dates[0].Month - 1);
                List<(DateTime, double)> inkomsten = new List<(DateTime, double)>();
                dates[0] = dates[0].AddDays(1 - dates[0].Day);
                for (int a = 0; a < months; a++)
                {
                    inkomsten.Add((dates[0].AddMonths(a), code_eigenaar.Inkomsten(new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a + 1))));
                }
                double pos = 0;
                int page = 0;
                do
                {
                    List<string> incomeString = IncomeToStringMonths(inkomsten, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(incomeString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw inkomsten per maand op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Maand                      |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totaal inkomen : €{ Convert.ToInt32(inkomsten.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, inkomsten.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(inkomsten[Convert.ToInt32(pos)].Item1, inkomsten[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                        detail.DoWork();
                        page = 0;
                        pos = 0;
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "2")
            {
                List<DateTime> dates = io.GetReservations().OrderBy(d => d.datum).Select(d => d.datum).Distinct().ToList();
                dates[0] = dates[0].AddDays(1 - Convert.ToInt32(dates[0].DayOfWeek));
                TimeSpan timeSpan = DateTime.Now.Date - (dates[0].Date);
                int weeks = timeSpan.Days / 7 + 1;
                List<(DateTime, double)> inkomsten = new List<(DateTime, double)>();
                for (int a = 0; a < weeks; a++)
                {
                    inkomsten.Add((dates[0].AddDays(a * 7), code_eigenaar.Inkomsten(new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays(a * 7), new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays((a + 1) * 7))));
                }

                double pos = 0;
                int page = 0;
                do
                {
                    List<string> incomeString = IncomeToStringWeeks(inkomsten, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(incomeString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw inkomsten per week op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Week                       |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totaal inkomen : €{ Convert.ToInt32(inkomsten.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, inkomsten.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(inkomsten[Convert.ToInt32(pos)].Item1, inkomsten[Convert.ToInt32(pos)].Item1.AddDays(7)));
                        detail.DoWork();
                        page = 0;
                        pos = 0;
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "3")
            {
                return 11;
            }
            else
            {
                Console.WriteLine("\nU moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 15;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class DetailIncomeScreen : Screen
    {
        private List<(DateTime, string, double)> items = new List<(DateTime, string, double)>();

        public DetailIncomeScreen(List<(DateTime, string, double)> items)
        {
            this.items = items;
        }

        private List<string> IncomeToString(List<(DateTime, string, double)> income)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < income.Count; a++)
            {
                output.Add("| " + income[a].Item1.ToShortDateString() + new string(' ', 20 - (income[a].Item1.ToShortDateString()).Length) +
                    " | " + income[a].Item2 + new string(' ', 40 - income[a].Item2.Length) +
                    " | " + "€" + income[a].Item3 + new string(' ', 12 - ("€" + income[a].Item3).Length) + " |\n" +
                    new string('–', 82) + "\n");
            }

            return output;
        }

        public override int DoWork()
        {
            int page = 0;
            do
            {
                List<string> incomeString = IncomeToString(items);
                List<string> pages = MakePages(incomeString, 20);

                Console.Clear();
                Console.WriteLine(GetGFLogo(true));
                Console.WriteLine($"Dit zijn uw inkomsten per week op pagina {page + 1} van de {pages.Count}:");
                Console.WriteLine(new string('–', 82) + "\n" + "|Datum                 |Naam                                      |Inkomsten     |");
                Console.WriteLine(new string('–', 82) + "\n" + pages[page]);
                Console.WriteLine($"totaal inkomen : €{ Convert.ToInt32(items.Select(P => P.Item3).Sum())},00\n");

                (int, int) result = Nextpage(page, pages.Count - 1, 11);
                if (result.Item2 != -1)
                {
                    return result.Item2;
                }
                page = result.Item1;
            } while (true);
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }

    public class ExpensesScreen : Screen
    {
        private List<string> ExpensesToStringMonth(List<(DateTime, double)> expenses, double pos, string optionMessage)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < expenses.Count; a++)
            {
                if (pos == a)
                {
                    output.Add("| " + expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddMonths(1).ToShortDateString() + new string(' ', 25 - (expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddMonths(1).ToShortDateString()).Length) +
                        " | " + "€" + Convert.ToInt32(expenses[a].Item2) + new string(' ', 12 - ("€" + Convert.ToInt32(expenses[a].Item2)).Length) + $" {optionMessage}|\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
                else
                {
                    output.Add("| " + expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddMonths(1).ToShortDateString() + new string(' ', 25 - (expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddMonths(1).ToShortDateString()).Length) +
                        " | " + "€" + Convert.ToInt32(expenses[a].Item2) + new string(' ', 12 - ("€" + Convert.ToInt32(expenses[a].Item2)).Length) + new string(' ', optionMessage.Length) + " |\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
            }

            return output;
        }

        private List<string> IncomeToStringWeeks(List<(DateTime, double)> expenses, double pos, string optionMessage)
        {
            List<string> output = new List<string>();

            for (int a = 0; a < expenses.Count; a++)
            {
                if (pos == a)
                {
                    output.Add("| " + expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddDays(7).ToShortDateString() + new string(' ', 25 - (expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddDays(7).ToShortDateString()).Length) +
                        " | " + "€" + Convert.ToInt32(expenses[a].Item2) + new string(' ', 12 - ("€" + Convert.ToInt32(expenses[a].Item2)).Length) + $" {optionMessage}|\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
                else
                {
                    output.Add("| " + expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddDays(7).ToShortDateString() + new string(' ', 25 - (expenses[a].Item1.ToShortDateString() + " - " + expenses[a].Item1.AddDays(7).ToShortDateString()).Length) +
                        " | " + "€" + Convert.ToInt32(expenses[a].Item2) + new string(' ', 12 - ("€" + Convert.ToInt32(expenses[a].Item2)).Length) + new string(' ', optionMessage.Length) + " |\n" +
                        new string('–', 42 + optionMessage.Length + 2) + "\n");
                }
            }

            return output;
        }

        public override int DoWork()
        {
            Console.WriteLine(GetGFLogo(true));
            Console.WriteLine("Hier kunt u alle uitgaven zien die u nu heeft");
            Console.WriteLine("[1] Laat alle uitgaven zien per maand");
            Console.WriteLine("[2] Laat alle uitgaven zien per week");
            Console.WriteLine("[3] Laat alle arbeidskosten zien per maand");
            Console.WriteLine("[4] Laat alle Inboedel kosten zien per maand");
            Console.WriteLine("[5] Laat alle Ingredienten kosten zien per maand");
            Console.WriteLine("[6] Ga terug naar eigenaar menu scherm");
            (string, int) input = AskForInput(15);

            if (input.Item1 == "0")
            {
                logoutUpdate = true;
                Logout();
                return 0;
            }
            else if (input.Item1 == "1")
            {
                List<DateTime> dates = code_eigenaar.GetUitgavenWerknemers().OrderBy(d => d.Item2).Select(d => d.Item2).Distinct().ToList();
                dates.AddRange(code_eigenaar.GetUitgavenIngredienten().OrderBy(d => d.bestel_datum).Select(d => d.bestel_datum).Distinct().ToList());
                dates = dates.OrderBy(d => d).Distinct().ToList();
                int months = ((DateTime.Now.Year - dates[0].Year) * 12) + DateTime.Now.Month - (dates[0].Month - 1);
                List<(DateTime, double)> uitgaven = new List<(DateTime, double)>();
                dates[0] = dates[0].AddDays(1 - dates[0].Day);
                for (int a = 0; a < months; a++)
                {
                    uitgaven.Add((dates[0].AddMonths(a), code_eigenaar.GetUitgavenWerknemers(new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a + 1)) +
                        code_eigenaar.GetUitgavenIngredienten(new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a + 1))));
                }
                double pos = 0;
                int page = 0;
                do
                {
                    List<string> expensesString = ExpensesToStringMonth(uitgaven, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(expensesString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw uitgaven per maand op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Maand                      |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totale uitgaven : €{ Convert.ToInt32(uitgaven.Select(P => P.Item2).Sum())},00\n");
                    
                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, uitgaven.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
/*                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(uitgaven[Convert.ToInt32(pos)].Item1, uitgaven[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                        detail.DoWork();
                        page = 0;
                        pos = 0;*/
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "2")
            {
                List<DateTime> dates = code_eigenaar.GetUitgavenWerknemers().OrderBy(d => d.Item2).Select(d => d.Item2).Distinct().ToList();
                dates.AddRange(code_eigenaar.GetUitgavenIngredienten().OrderBy(d => d.bestel_datum).Select(d => d.bestel_datum).Distinct().ToList());
                dates = dates.OrderBy(d => d).Distinct().ToList();
                dates[0] = dates[0].AddDays(1 - Convert.ToInt32(dates[0].DayOfWeek));
                TimeSpan timeSpan = DateTime.Now.Date - (dates[0].Date);
                int weeks = timeSpan.Days / 7 + 1;
                List<(DateTime, double)> uitgaven = new List<(DateTime, double)>();
                for (int a = 0; a < weeks; a++)
                {
                    uitgaven.Add((dates[0].AddDays(a * 7), code_eigenaar.GetUitgavenWerknemers(new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays(a * 7), new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays((a + 1) * 7)) +
                        code_eigenaar.GetUitgavenIngredienten(new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays(a * 7), new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddDays((a + 1) * 7))));
                }

                double pos = 0;
                int page = 0;
                do
                {
                    List<string> expensesString = IncomeToStringWeeks(uitgaven, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(expensesString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw uitgaven per week op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Week                       |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totale uitgaven : €{ Convert.ToInt32(uitgaven.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, uitgaven.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        /*                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(uitgaven[Convert.ToInt32(pos)].Item1, uitgaven[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                                                detail.DoWork();
                                                page = 0;
                                                pos = 0;*/
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "3")
            {
                List<DateTime> dates = code_eigenaar.GetUitgavenWerknemers().OrderBy(d => d.Item2).Select(d => d.Item2).Distinct().ToList();
                dates = dates.OrderBy(d => d).Distinct().ToList();
                int months = ((DateTime.Now.Year - dates[0].Year) * 12) + DateTime.Now.Month - (dates[0].Month - 1);
                List<(DateTime, double)> uitgaven = new List<(DateTime, double)>();
                dates[0] = dates[0].AddDays(1 - dates[0].Day);
                for (int a = 0; a < months; a++)
                {
                    uitgaven.Add((dates[0].AddMonths(a), code_eigenaar.GetUitgavenWerknemers(new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a + 1))));
                }
                double pos = 0;
                int page = 0;
                do
                {
                    List<string> expensesString = ExpensesToStringMonth(uitgaven, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(expensesString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw uitgaven per maand op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Maand                      |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totale uitgaven : €{ Convert.ToInt32(uitgaven.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, uitgaven.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        /*                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(uitgaven[Convert.ToInt32(pos)].Item1, uitgaven[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                                                detail.DoWork();
                                                page = 0;
                                                pos = 0;*/
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "4")
            {
                List<DateTime> dates = code_eigenaar.GetInboedel().OrderBy(d => d.Item2).Select(d => d.Item2).Distinct().ToList();
                List <(Inboedel, DateTime)> inboedel = code_eigenaar.GetInboedel();
                dates = dates.OrderBy(d => d).Distinct().ToList();
                int months = ((DateTime.Now.Year - dates[0].Year) * 12) + DateTime.Now.Month - (dates[0].Month - 1);
                List<(DateTime, double)> uitgaven = new List<(DateTime, double)>();
                for (int a = 0; a < months; a++)
                {
                    uitgaven.Add((dates[0].AddMonths(a), code_eigenaar.GetInboedel(new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, dates[0].Day).AddMonths(a + 1))));
                }

                double pos = 0;
                int page = 0;
                do
                {
                    List<string> expensesString = ExpensesToStringMonth(uitgaven, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(expensesString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw uitgaven per maand op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|maand                      |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totale uitgaven : €{ Convert.ToInt32(uitgaven.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, uitgaven.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
/*                        *//*                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(uitgaven[Convert.ToInt32(pos)].Item1, uitgaven[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                                                detail.DoWork();
                                                page = 0;
                                                pos = 0;*//**/
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "5")
            {
                List<DateTime> dates = code_eigenaar.GetUitgavenIngredienten().OrderBy(d => d.bestel_datum).Select(d => d.bestel_datum).Distinct().ToList();
                dates = dates.OrderBy(d => d).Distinct().ToList();
                int months = ((DateTime.Now.Year - dates[0].Year) * 12) + DateTime.Now.Month - (dates[0].Month - 1);
                List<(DateTime, double)> uitgaven = new List<(DateTime, double)>();
                dates[0] = dates[0].AddDays(1 - dates[0].Day);
                for (int a = 0; a < months; a++)
                {
                    uitgaven.Add((dates[0].AddMonths(a), code_eigenaar.GetUitgavenIngredienten(new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a), new DateTime(dates[0].Year, dates[0].Month, 1).AddMonths(a + 1))));
                }
                double pos = 0;
                int page = 0;
                do
                {
                    List<string> expensesString = ExpensesToStringMonth(uitgaven, pos, "[3] Meer Detail");
                    List<string> pages = MakePages(expensesString, 20);

                    Console.Clear();
                    Console.WriteLine(GetGFLogo(true));
                    Console.WriteLine($"Dit zijn uw uitgaven per maand op pagina {page + 1} van de {pages.Count}:");
                    Console.WriteLine(new string('–', 59) + "\n" + "|Maand                      |Totaal                       |");
                    Console.WriteLine(new string('–', 59) + "\n" + pages[page]);
                    Console.WriteLine($"totale uitgaven : €{ Convert.ToInt32(uitgaven.Select(P => P.Item2).Sum())},00\n");

                    (int, int, double) result = NextpageTable(page, pages.Count - 1, pos, uitgaven.Count - 1, 11);
                    if (result.Item2 != -1)
                    {
                        return result.Item2;
                    }
                    else if (result.Item1 == -1 && result.Item2 == -1)
                    {
                        /*                        DetailIncomeScreen detail = new DetailIncomeScreen(code_eigenaar.InkomstenPerItem(uitgaven[Convert.ToInt32(pos)].Item1, uitgaven[Convert.ToInt32(pos)].Item1.AddMonths(1)));
                                                detail.DoWork();
                                                page = 0;
                                                pos = 0;*/
                    }
                    else
                    {
                        pos = result.Item3;
                        page = result.Item1;
                    }
                } while (true);
            }
            else if (input.Item1 == "6")
            {
                return 11;
            }
            else
            {
                Console.WriteLine("\nU moet wel een juiste keuze maken...");
                Console.WriteLine("Druk op en knop om verder te gaan.");
                Console.ReadKey();
                return 13;
            }
        }

        public override List<Screen> Update(List<Screen> screens)
        {
            DoLogoutOnEveryScreen(screens);
            return screens;
        }
    }
}
