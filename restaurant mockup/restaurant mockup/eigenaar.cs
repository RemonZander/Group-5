using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Eigenaar : Persoon
    {
        public Eigenaar()
        {
            Type = "Medewerker";
            is_uitgave = true;
        }
    }
}