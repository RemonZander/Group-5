using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Code_Eigenaar_menu
    {
        private Database database = new Database();

        IO io = new IO();
        Testing_class instance = new Testing_class();

        public Code_Eigenaar_menu()
        {
        }

        public void Debug()
        {
            database = io.Getdatabase();
            Menukaart menukaart = new Menukaart();
            menukaart.gerechten = instance.Get_standard_dishes();
            
            database.menukaart = menukaart;
            io.Savedatabase(database);

            DateTime endDate = DateTime.Now;
            endDate = endDate.AddDays(7);
            instance.Fill_Userdata(10);
            instance.Fill_reservations(100, 1, 12, 1, 28);
            database = io.Getdatabase();
            GetUserOrderInfo(DateTime.Now, endDate);
            MakeDishPopular(0);
        }

        private bool IfDishExists(int id)
        {
            for (int i = 0; i < database.menukaart.gerechten.Count; i++)
            {
                if (database.menukaart.gerechten[i].ID == id)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void MakeDishPopular(int id)
        {
            if (IfDishExists(id))
            {
                int dishIndex = database.menukaart.gerechten.FindIndex(x => x.ID == id);
                database.menukaart.gerechten[dishIndex] = new Gerechten
                {
                    ID = database.menukaart.gerechten[dishIndex].ID,
                    ingredienten = database.menukaart.gerechten[dishIndex].ingredienten,
                    is_gearchiveerd = database.menukaart.gerechten[dishIndex].is_gearchiveerd,
                    is_populair = true,
                    naam = database.menukaart.gerechten[dishIndex].naam,
                    prijs = database.menukaart.gerechten[dishIndex].prijs,
                    special = database.menukaart.gerechten[dishIndex].special,
                };
                io.Savedatabase(database);
            }
        }

        public List<Tuple<Gerechten, int>> GetUserOrderInfo(DateTime beginDate, DateTime endDate)
        {
            List<Tuple<Gerechten, int>> populaireGerechten = new List<Tuple<Gerechten, int>>();
            //hier komt een loop waarbij populaire gerechten gevuld wordt met alle bestaande gerechten.
            //als de reservering plaats heeft gevonden binnen de gestelde tijd.
            for (int i = 0; i < 6; i++)
            {
                populaireGerechten.Add(Tuple.Create(instance.Get_standard_dishes()[i], 0));

            }
            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum.Date >= beginDate.Date && reservering.datum.Date <= endDate.Date)
                {
                    foreach (var gerechtID in reservering.gerechten_ID)
                    {
                        for (int i = 0; i < populaireGerechten.Count; i++)
                        {
                            if (populaireGerechten[i].Item1.ID == gerechtID)
                            {
                                populaireGerechten[i] = Tuple.Create(populaireGerechten[i].Item1, populaireGerechten[i].Item2 + 1);
                                break;
                            }
                        }
                    }
                }
            }
            return populaireGerechten;
        }
    }
}