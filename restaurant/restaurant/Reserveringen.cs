using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public struct Reserveringen
    {
        public int ID { get; set; }

        public DateTime datum { get; set; }

        public List<Klantgegevens> klantgegevens { get; set; }

        //positie 1 is ID en positie 2 is aantal zetels per tafel
        public int[,] Tafels { get; set; }
    }
}