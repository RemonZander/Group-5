using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Login_gegevens
    {
        public string email { get; set; }

        public string password { get; set; }

        public string type { get; set; }

        public Klantgegevens klantgegevens { get; set; }
    }
}