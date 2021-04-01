using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace restaurant
{
    public class Testing_class
    {
        private readonly Database database = new Database();
        private readonly IO io = new IO();

        public Testing_class()
        {
            database = io.Getdatabase();
        }
        
        public void Debug()
        {
            Fill_Userdata(100);
            Fill_reservations(100);
        }

        //In de region hierinder staat alle code voor het opslaan van Reserveringen
        #region Reserveringen

        //Deze functie is voor als je de database wilt vullen met random reserveringen
        public void Fill_reservations(int amount)
        {
            List<Reserveringen> reserveringen_list = new List<Reserveringen>();
            Random rnd = new Random();
            for (int a = 0; a < amount; a++)
            {
                List<Tafels> tafels = new List<Tafels>();
                for (int b = 0; b < rnd.Next(4); b++)
                {
                    tafels.Add(database.tafels[b]);
                }

                reserveringen_list.Add(new Reserveringen
                {
                    datum = new DateTime(DateTime.Now.Year, DateTime.Now.Month, rnd.Next(1, 30), 22, 0, 0),
                    ID = a,
                    gerechten = Make_dishes(),
                    tafels = tafels,
                    
                });
            }

            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        //Deze functie is voor als je de database wilt vullen met je eigen data.
        //Zorg wel dat iedere list even lang is als amount
        public void Fill_reservations(int amount, List<DateTime> datum, List<List<Gerechten>> gerechten, List<List<Tafels>> tafels)
        {
            if (datum.Count != amount || gerechten.Count != amount || tafels.Count != amount)
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
                    gerechten = gerechten[a],
                    tafels = tafels[a],
                });
            }

            database.reserveringen = reserveringen_list;
            io.Savedatabase(database);
        }

        #endregion

        //In de region hieronder staat alle code voor het maken van gerechten
        #region Gerechten

        //Deze functie is voor als je simpel een lijst van gerechten wilt zonder voorkeur
        public List<Gerechten> Make_dishes()
        {
            List<Gerechten> gerechten = new List<Gerechten>();

            gerechten.Add(new Gerechten
            {
                ID = 0,
                naam = "Pizza Salami",
                is_populair = true,
                is_gearchiveerd = false,
                special = true,
                prijs = 15.0
            });
            gerechten.Add(new Gerechten
            {
                ID = 1,
                naam = "Vla",
                is_populair = false,
                is_gearchiveerd = false,
                special = true,
                prijs = 8.0
            });
            gerechten.Add(new Gerechten
            {
                ID = 2,
                naam = "Hamburger",
                is_populair = true,
                is_gearchiveerd = false,
                special = false,
                prijs = 13.0
            });
            gerechten.Add(new Gerechten
            {
                ID = 3,
                naam = "Yoghurt",
                is_populair = false,
                is_gearchiveerd = true,
                special = false,
                prijs = 6.0
            });
            gerechten.Add(new Gerechten
            {
                ID = 4,
                naam = "IJs",
                is_populair = false,
                is_gearchiveerd = true,
                special = false,
                prijs = 9.5
            });
            gerechten.Add(new Gerechten
            {
                ID = 5,
                naam = "Patat",
                is_populair = true,
                is_gearchiveerd = false,
                special = false,
                prijs = 11.5
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

        public void Fill_Userdata(int amount)
        {
            string[][] names = Make_Names();
            Random rnd = new Random();
            List<Login_gegevens> login_Gegevens = new List<Login_gegevens>();
            for (int a = 0; a < amount; a++)
            {
                string Firstname = "";
                string Surname = "";
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
    }

    public partial class Code_Gebruiker_menu
    {

    }
}
