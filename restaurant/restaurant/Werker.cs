using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public abstract class Werker
    {
        public double lease_auto { get; set; }

        public int ID { get; set; }

        public double prestatiebeloning { get; set; }

        public double salaris { get; set; }

        public Login_gegevens login_gegevens { get; set; }
    }
}