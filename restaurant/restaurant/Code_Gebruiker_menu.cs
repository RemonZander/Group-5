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
            //de tweede for loop, gaat door alle uren heen,
            //de derde loop voegt steeds een kwartier toe aan de tijd,
            //als laatste word tijd teruggezet naar 1000 de volgende dag
            DateTime possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
            for (int days = 0; days <= dagen; days++)
            {
                for (int hours = 0; hours < 12; hours++)
                {
                    for (int quarter = 0; quarter < 4; quarter++)
                    {
                        beschikbaar.Add(Tuple.Create(possibleTime, database.tafels));
                        possibleTime = possibleTime.AddMinutes(15);
                    }
                }
                possibleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 10, 0, 0);
            }

            foreach (var reservering in database.reserveringen)
            {
                // reservering heeft een tijdsblok van 2 uur
                //als die een reservering vind in de database moet die bij de gereserveerde tafel voor een x aantal uur en y aantal minuten de beschikbare reservering weghalen
                if(reservering.datum.Date == DateTime.Now.Date || reservering.datum.Date <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day+dagen, 21, 0, 0))
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