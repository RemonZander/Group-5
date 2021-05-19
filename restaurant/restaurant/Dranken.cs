using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Dranken
    {
        public string naam { get; set; }

        public int ID { get; set; }

        public bool isGearchiveerd { get; set; }

        public bool heeftAlcohol { get; set; }

        public bool diner { get; set; }

        public bool lunch { get; set; }

        public bool ontbijt { get; set; }
    }
}