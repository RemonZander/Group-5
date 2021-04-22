using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public partial class Code_Gebruiker_menu
    {
        Database database = new Database();
        IO io = new IO();
        Testing_class testClass = new Testing_class();
        
        public void Debug()
        {
            if (database.menukaart == null)
            {
                database.menukaart = new Menukaart();
                database.menukaart.gerechten = new List<Gerechten>();
                database.menukaart.gerechten.AddRange(testClass.Get_standard_dishes());
            }
            else
            {
                database.menukaart.gerechten = testClass.Get_standard_dishes();
            }
            List<Gerechten> test = Getmenukaart(new List<string> { "lactose intolerantie" });
        }
        
        public Code_Gebruiker_menu()
        {
            database = io.Getdatabase();
        }

        //pakt de reservering van een klant
        public Reserveringen Get_reservation(Klantgegevens klant)
        {
            Reserveringen reservering = new Reserveringen();
            //voor elke reservering in de database, voor elk klantnummer
            foreach (var reserveringen in database.reserveringen)
            {
                foreach (var klantnummer in reserveringen.klantnummers)
                {
                    //als de klant is gevonden en datum is in de toekomst, voeg de reservering toe
                    if (klant.klantnummer == klantnummer && reserveringen.datum > DateTime.Now)
                    {
                        reservering = reserveringen;
                    }
                }
            }
            return reservering;
        }

        //reset de database
        public void Remove_reservations(Reserveringen reserveringen)
        {
            database = io.Getdatabase();
            database.reserveringen.Remove(reserveringen);
            io.Savedatabase(database);
        }

        /*public string GetMenukaart()
        {
            
            string menukaart = "";
            for(int i = 0; i< database.menukaart.gerechten.Count; i++)
            {
                //begint bij de naam van het gerecht
                menukaart += database.menukaart.gerechten[i].naam;
                
                //checked if gerecht is populair of speciaal zo ja, voeg er wat achter en enter
                if (database.menukaart.gerechten[i].is_populair && database.menukaart.gerechten[i].special)
                {
                    menukaart += " | Populair | Speciaal\n";
                }
                else if (database.menukaart.gerechten[i].is_populair)
                {
                    menukaart += " | Populair\n";
                }
                else if (database.menukaart.gerechten[i].special)
                {
                    menukaart += " | Speciaal\n";
                }
                else
                {
                    menukaart += "\n";
                }

                //voegt alle ingrediënten toe en per 7 doet een enter
                for (int j = 0; j < database.menukaart.gerechten[i].ingredienten.Count; j++)
                {
                    menukaart += "Ingrediënten: " + database.menukaart.gerechten[i].ingredienten[j];
                    if (j % 7 == 0)
                    {
                        menukaart += "\n";
                    }
                }
                
            }
            return menukaart;
        }*/

        public List<Gerechten> Getmenukaart()
        {
            return database.menukaart.gerechten;
        }

        public List<Gerechten> Getmenukaart(List<string> allergenen)
        {
            //maakt nieuwe lijst met de menukaart
            List<Gerechten> menulist = new List<Gerechten>(Getmenukaart());
            
            //voor alles in menukaart, voor alle allergenen
            for (int i = 0; i < database.menukaart.gerechten.Count; i++)
            {
                for (int j = 0; j < allergenen.Count; j++)
                {
                    //als ingredient == allergeen zet die naar null
                    if (database.menukaart.gerechten[i].allergenen.Contains(allergenen[j]))
                    {
                        menulist[i] = null;
                    }
                }
            }
            menulist.RemoveAll(x => x == null);
            return menulist;
        }
    }
}