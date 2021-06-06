using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Ingredient : IngredientType
    {
        public int ID { get; set; }

        public DateTime bestel_datum { get; set; }

        [Obsolete("Gebruik dagenhoudbaar")]
        public DateTime houdbaarheids_datum { get; set; }
    }
}