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

        public Code_Eigenaar_menu()
        {
            database = io.Getdatabase();
        }

        public List<Tuple<Gerechten, int>> GetUserOrderInfo(DateTime beginDate, DateTime endDate)
        {
            List<Tuple<Gerechten, int>> populaireGerechten = new List<Tuple<Gerechten, int>>();
            //hier komt een loop waarbij populaire gerechten gevuld wordt met alle bestaande gerechten.
            //als de reservering plaats heeft gevonden binnen de gestelde tijd.
            foreach (var reservering in database.reserveringen)
            {
                if (reservering.datum.Date >= beginDate.Date && reservering.datum.Date <= endDate.Date)
                {
                    foreach (var gerecht in reservering.gerechten)
                    {
                        for (int i = 0; i < populaireGerechten.Count; i++)
                        {
                            if (populaireGerechten[i].Item1.Equals(gerecht))
                            {
                                populaireGerechten[i] = Tuple.Create(gerecht, populaireGerechten[i].Item2 + 1);
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