using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Medewerker : Persoon
    {
        public Medewerker()
        {
            Type = "Medewerker";
            is_uitgave = true;
        }
    }
}