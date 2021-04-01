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
        
        public void Debug()
        {

        }
        
        public Code_Gebruiker_menu()
        {
            database = io.Getdatabase();
        }
        //tijden beschikbaar, kan tafel bij raam,
        public List<Tuple<DateTime, List<Tafels>>> Reservering_beschikbaarheid(int dagen) 
        {
            //maakt een lijst met tuples die beheert alle beschikbare plekken op int aantal dagen
            List<Tuple<DateTime, List<Tafels>>> beschikbaar = new List<Tuple<DateTime, List<Tafels>>>();

            //de eerste for loop, maakt voor iedere dag een nieuwe tijd aan
            //de tweede for loop, voegt alle beschikbare tijden voor iedere dag toe (om de twee uur vanaf 1000 tot 2200)
            for (int a= 0; a<=dagen; a++)
            {
                DateTime temp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
                temp = temp.AddDays(a);
                for (int b = 0; b<=6; b++)
                {
                    temp.AddHours(b * 2);
                    beschikbaar.Add(Tuple.Create(temp,database.tafels));
                }
            }
            
            foreach (var reservering in database.reserveringen)
            {
                // reservering heeft een tijdsblok van 2 uur
                if(reservering.datum.Date == DateTime.Now.Date || reservering.datum.Date <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day+dagen, 22, 0, 0))
                {
                    List<Tafels> tempTableList = beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))].Item2;
                    foreach (var tafel in reservering.tafels)
                    {
                        tempTableList.Remove(tafel);
                    }
                    if(tempTableList.Count == 0)
                    {
                        beschikbaar.Remove(Tuple.Create(reservering.datum, tempTableList));
                    }
                    else
                    {
                        beschikbaar[beschikbaar.IndexOf(Tuple.Create(reservering.datum, database.tafels))] = Tuple.Create(reservering.datum, tempTableList);
                    }
                    
                }
            }
            return beschikbaar;
        }
    }
}