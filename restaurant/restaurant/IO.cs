using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.IO;

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

            //voor elke reservering die gemaakt is
            foreach (var reservering in database.reserveringen)
            {
                //op de dag die is ingevoerd, pak alle beschikbare tijden en tafels
                if (reservering.datum >= new DateTime(date.Year, date.Month, date.Day, 10, 0, 0) && reservering.datum <= new DateTime(date.Year, date.Month, date.Day, 21, 0, 0))
                {
                    beschikbaar = verwijder_Reservering(beschikbaar, reservering);
                }
            }
            return beschikbaar;
        }

        //pakt alle beschikbare tijden en tafels tussen nu en aantal ingevoerde dagen
        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(int start_maand, int eind_maand, int start_dag, int eind_dag)
        {
            database = Getdatabase();
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = Calc_totale_beschikbaarheid(start_maand, eind_maand, start_dag, eind_dag);


            //verantwoordelijk voor het communiceren met de database
            foreach (var reservering in database.reserveringen)
            {
                //voor de datum tussen nu en de ingevoerde dag
                if (reservering.datum >= new DateTime(DateTime.Now.Year, start_maand, start_dag, 10, 0, 0) && reservering.datum <= new DateTime(DateTime.Now.Year, eind_maand, eind_dag, 21, 0, 0))
                {
                    beschikbaar = verwijder_Reservering(beschikbaar, reservering);
                }
            }
            return beschikbaar;
        }

        //verwijderd de reservering uit beschikbaar en returned beschikbaar
        private List<Tuple<DateTime, List<Tafels>>> verwijder_Reservering(List<Tuple<DateTime, List<Tafels>>> beschikbaar, Reserveringen reservering)
        {
            //bevat alle tafels
            List<Tafels> tempTableList = new List<Tafels>();
            //bevat alle tafels die al geboekt zijn en haalt die later weg
            List<Tafels> removed_tables = new List<Tafels>();
            //slaat de locatie op van 
            int location = 0;
            
            //gaat door alle gereserveerde tafels in die reservering en haalt deze weg
            for (int d = 0; d < beschikbaar.Count; d++)
            {
                //als beschikbare datum staat gelijk aan een datum met een reservering
                if (beschikbaar[d].Item1 == reservering.datum)
                {
                    tempTableList = new List<Tafels>(beschikbaar[d].Item2);
                    //voor alle tafels die een reservering hebben op die tijd, haal die weg uit beschikbaar en voeg die toe aan removed_tabels
                    foreach (var tafel in reservering.tafels)
                    {
                        tempTableList.Remove(tafel);
                        removed_tables.Add(tafel);
                    }
                    location = d;
                    //stopt de if statement en gaat verder door beschikbaar
                    break;
                }
            }

            //als er geen tafels meer vrij zijn haalt hij de tafel weg
            if (tempTableList.Count == 0)
            {
                beschikbaar.RemoveAt(location);
            }
            //maakt tuple met tafels die wel beschikbaar zijn
            else
            {
                //maakt op locatie in beschikbaar, een tuple met reservering datum en alle ongeboekte tafels
                beschikbaar[location] = Tuple.Create(reservering.datum, tempTableList);
                // ,1-8 want er zitten 8 kwartieren in 2uur
                for (int b = 1; b <= 8; b++)
                {
                    //als location+b out of range gaat, break
                    if ((location + b) >= beschikbaar.Count) break;
                    //haalt alle tafels uit beschikbaar die in removed_tables staan
                    beschikbaar[location + b] = Tuple.Create(reservering.datum.AddMinutes(15 * b), beschikbaar[location + b].Item2.Except(removed_tables).ToList());
                    //als er helemaal geen tafels meer beschikbaar zijn voor een gegeven tijd, haal die weg
                    if (beschikbaar[location + b].Item2.Count == 0)
                    {
                        beschikbaar.RemoveAt(location + b);
                    }
                }
            }
            return beschikbaar;
        }
        
        //maakt alle mogelijke plekken aan voor een starttijd en een eindtijd
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
                        //voegt alle beschikbare tijden en tafels aan beschikbaar voor ieder kwartier
                        beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                        possibleTime = possibleTime.AddMinutes(15);
                    }
                }
                possibleTime = new DateTime(DateTime.Now.Year, maanden, start_dag, 10, 0, 0);
            }
            return beschikbaar;
        }

        //ordered reserveringen op ID
        public void orderReserveringID()
        {
            //pakt de database
            database = Getdatabase();
            //ordered bij een lambda, in dit geval ID
            database.reserveringen = database.reserveringen.OrderBy(s => s.ID).ToList();
            Savedatabase(database);
        }
        
        //ordered reserveringen op datum
        public void orderReserveringDatum()
        {
            //pakt de database
            database = Getdatabase();
            //ordered bij een lambda, in dit geval datum
            database.reserveringen = database.reserveringen.OrderBy(s => s.datum).ToList();
            Savedatabase(database);
        }
    }
}