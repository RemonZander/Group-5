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

        public Klantgegevens[] klantgegevens { get; set; }
    }
}