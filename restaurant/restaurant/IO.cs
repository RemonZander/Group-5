using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace restaurant
{
    public partial class IO
    {
        private Database database = new Database();
        public void Savedatabase(Database database)
        {
            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }

            
            string output = JsonConvert.SerializeObject(database, Formatting.Indented);
            // @ neemt tekst letterlijk, geen \n bijv.
            File.WriteAllText(@"..\database\database.Json", output);
        }

        public Database Getdatabase()
        {
            Database database = new Database();
            
            if (!File.Exists(@"..\database\database.Json")) return database;
            string output = File.ReadAllText(@"..\database\database.Json");
            database = JsonConvert.DeserializeObject<Database>(output);

            List<Tafels> temp = new List<Tafels>();
            for (int i = 0; i < 100; i++)
            {
                Tafels tafel = new Tafels
                {
                    ID = i,
                    Zetels = 4
                };

                if (i % 2 != 0) tafel.isRaam = true;

                temp.Add(tafel);
            }
            database.tafels = temp;

            return database;
        }
        
        //Reset de database
        public void Reset_filesystem()
        {
            try
            {
                FileSystem.DeleteDirectory(@"..\database\", DeleteDirectoryOption.DeleteAllContents);
            }
            catch
            {
            }

            if (!FileSystem.DirectoryExists(@"..\database\"))
            {
                FileSystem.CreateDirectory(@"..\database\");
            }
        }

        //pakt alle beschikbare tijden en tafels tussen nu en aantal ingevoerde dagen
        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(int dagen)
        {
            database = Getdatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
            for (int days = 0; days <= dagen; days++)
            {
                //45 kwaterieren van 1000 tot 2100
                for (int i = 0; i < 45; i++)
                {
                    beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                    possibleTime = possibleTime.AddMinutes(15);
                }
                //gaat naar de volgende dag met de openingsuren
                possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 10, 0, 0);
            }

            //verantwoordelijk voor het communiceren met de database
            foreach (var reservering in database.reserveringen)
            {
                //voor de datum tussen nu en de ingevoerde dag
                if (reservering.datum >= DateTime.Now && reservering.datum <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + dagen, 21, 0, 0))
                {
                    //temptablelist bevat alle tafels
                    //List<Tafels> tempTableList = beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))].Item2;
                    List<Tafels> tempTableList = new List<Tafels>();
                    List<Tafels> removed_tables = new List<Tafels>();
                    //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
                    for (int d = 0; d < beschikbaar.Count; d++)
                    {
                        if (beschikbaar[d].Item1 == reservering.datum)
                        {
                            tempTableList = new List<Tafels>(beschikbaar[d].Item2);
                            foreach (var tafel in reservering.tafels)
                            {
                                tempTableList.Remove(tafel);
                                removed_tables.Add(tafel);
                            }
                            break;
                        }
                    }

                    //als er geen tafels meer vrij zijn haalt hij de tafel weg
                    if (tempTableList.Count == 0)
                        for (int a = 0; a < beschikbaar.Count; a++)
                        {
                            if (beschikbaar[a].Item1 == reservering.datum)
                            {
                                for (int b = 0; b <= 8; b++)
                                {
                                    beschikbaar.RemoveAt(a + b);
                                }
                                break;
                            }
                        }
                    //maakt tuple met tafels die wel beschikbaar zijn
                    else
                    {
                        for (int a = 0; a < beschikbaar.Count; a++)
                        {
                            if (beschikbaar[a].Item1 == reservering.datum)
                            {
                                beschikbaar[a] = Tuple.Create(reservering.datum, tempTableList);
                                for (int b = 1; b <= 8; b++)
                                {
                                    if ((a + b) >= beschikbaar.Count) break;
                                    beschikbaar[a + b] = Tuple.Create(reservering.datum.AddMinutes(15 * b), beschikbaar[a + b].Item2.Except(removed_tables).ToList());
                                    if (beschikbaar[a + b].Item2.Count == 0)
                                    {
                                        beschikbaar.RemoveAt(a + b);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return beschikbaar;
        }

        //pakt alle beschikbare tijden en tafels voor de ingevoerde dag
        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(DateTime date)
        {
            database = Getdatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //vult de List met alle beschikbare momenten en tafels
            DateTime possibleTime = new DateTime(date.Year, date.Month, date.Day, 10, 0, 0);
            
            //45 kwaterieren van 1000 tot 2100
            for (int i = 0; i < 45; i++)
            {
                beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                possibleTime = possibleTime.AddMinutes(15);
            }

            //verantwoordelijk voor het communiceren met de database
            foreach (var reservering in database.reserveringen)
            {
                //voor de datum tussen nu en de ingevoerde dag
                if (reservering.datum >= new DateTime(date.Year, date.Month, date.Day, 10, 0, 0) && reservering.datum <= new DateTime(date.Year, date.Month, date.Day, 21, 0, 0))
                {
                    //temptablelist bevat alle tafels
                    //List<Tafels> tempTableList = beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))].Item2;
                    List<Tafels> tempTableList = new List<Tafels>();
                    List<Tafels> removed_tables = new List<Tafels>();
                    //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
                    for (int d = 0; d < beschikbaar.Count; d++)
                    {
                        if (beschikbaar[d].Item1 == reservering.datum)
                        {
                            tempTableList = new List<Tafels>(beschikbaar[d].Item2);
                            foreach (var tafel in reservering.tafels)
                            {
                                tempTableList.Remove(tafel);
                                removed_tables.Add(tafel);
                            }
                            break;
                        }
                    }

                    //als er geen tafels meer vrij zijn haalt hij de tafel weg
                    if (tempTableList.Count == 0)
                        for (int a = 0; a < beschikbaar.Count; a++)
                        {
                            if (beschikbaar[a].Item1 == reservering.datum)
                            {
                                for (int b = 0; b <= 8; b++)
                                {
                                    beschikbaar.RemoveAt(a + b);
                                }
                                break;
                            }
                        }
                    //maakt tuple met tafels die wel beschikbaar zijn
                    else
                    {
                        for (int a = 0; a < beschikbaar.Count; a++)
                        {
                            if (beschikbaar[a].Item1 == reservering.datum)
                            {
                                beschikbaar[a] = Tuple.Create(reservering.datum, tempTableList);
                                for (int b = 1; b <= 8; b++)
                                {
                                    if ((a + b) >= beschikbaar.Count) break;
                                    beschikbaar[a + b] = Tuple.Create(reservering.datum.AddMinutes(15 * b), beschikbaar[a + b].Item2.Except(removed_tables).ToList());
                                    if (beschikbaar[a + b].Item2.Count == 0)
                                    {
                                        beschikbaar.RemoveAt(a + b);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return beschikbaar;
        }
    }
}