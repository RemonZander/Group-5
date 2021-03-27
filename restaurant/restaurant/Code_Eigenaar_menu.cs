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

        public void Debug()
        {
            DateTime endDate = DateTime.Now;
            endDate = endDate.AddDays(7);
            FillReservations();
            GetUserOrderInfo(DateTime.Now, endDate);
        }

        private void FillReservations()
        {
            List<Reserveringen> reserveringen = new List<Reserveringen>();
            for (int i = 0; i < 100; i++)
            {
                List<Tafels> tafelsTemp = new List<Tafels>();
                Random rndTafels = new Random();
                for (int j = 0; j < rndTafels.Next(4); j++)
                {
                    Tafels temp = new Tafels
                    {
                        Zetels = 4,
                        ID = rndTafels.Next(101)
                    };
                    if (temp.ID % 2 != 0)
                    {
                        temp.isRaam = true;
                    }
                    tafelsTemp.Add(temp);
                }

                List<Gerechten> gerechtenList = new List<Gerechten>();
                Random rndGerechten = new Random();
                for (int z = 0; z < 4; z++)
                {
                    int x = rndGerechten.Next(6);
                    Gerechten gerechtenTemp = new Gerechten();
                    gerechtenTemp.ID = x;
                    if (x == 0)
                    {
                        gerechtenTemp.naam = "Pizza Salami";
                    }
                    else if (x == 1)
                    {
                        gerechtenTemp.naam = "Vla";
                    }
                    else if (x == 2)
                    {
                        gerechtenTemp.naam = "Hamburger";
                    }
                    else if (x == 3)
                    {
                        gerechtenTemp.naam = "Yoghurt";
                    }
                    else if (x == 4)
                    {
                        gerechtenTemp.naam = "IJs";
                    }
                    else if (x == 5)
                    {
                        gerechtenTemp.naam = "Patat";
                    }
                    gerechtenList.Add(gerechtenTemp);
                }

                Random rndDay = new Random();
                DateTime datum = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, rndDay.Next(9, 23), 0, 0);
                datum.AddDays(rndDay.Next(8));
                Reserveringen tempRes = new Reserveringen
                {
                    ID = i,
                    tafels = tafelsTemp,
                    datum = datum,
                    gerechten = gerechtenList
                };

                reserveringen.Add(tempRes);
            }
            database.reserveringen = reserveringen;
        }

        public List<Tuple<Gerechten, int>> GetUserOrderInfo(DateTime beginDate, DateTime endDate)
        {
            List<Tuple<Gerechten, int>> populaireGerechten = new List<Tuple<Gerechten, int>>();
            //hier komt een loop waarbij populaire gerechten gevuld wordt met alle bestaande gerechten.
            //als de reservering plaats heeft gevonden binnen de gestelde tijd.
            populaireGerechten.Add(Tuple.Create(new Gerechten { 
                ID = 0,
                naam = "Pizza Salami",
            }, 0));
            populaireGerechten.Add(Tuple.Create(new Gerechten
            {
                ID = 1,
                naam = "Vla",
            }, 0));
            populaireGerechten.Add(Tuple.Create(new Gerechten
            {
                ID = 2,
                naam = "Hamburger",
            }, 0));
            populaireGerechten.Add(Tuple.Create(new Gerechten
            {
                ID = 3,
                naam = "Yoghurt",
            }, 0));
            populaireGerechten.Add(Tuple.Create(new Gerechten
            {
                ID = 4,
                naam = "IJs",
            }, 0));
            populaireGerechten.Add(Tuple.Create(new Gerechten
            {
                ID = 5,
                naam = "Patat",
            }, 0));
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