using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    //reserveringen moeten gesorteerd zijn op datum
    public struct Reserveringen
    {
        public int ID { get; set; }

        public DateTime datum { get; set; }

        public List<int> klantnummers { get; set; }

        //Bij het reserveren van een plek in het restaurant is deze lijst gevult
        //Bij het afhalen van eten (of laten brengen) is deze lijst leeg
        public List<Tafels> tafels { get; set; }

        //Dit is een lijst met de Id's van de gerechten
        //Bij het reserveren van een plek in het restaurant is deze lijst leeg
        //Bij het afhalen van eten (of laten brengen) is deze lijst gevult
        public List<int> gerechten_ID { get; set; }
    }
}