using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public abstract class Werker : Type_BTW_Prijs
    {
        public double inkomstenbelasting { get; set; }

        public double lease_auto { get; set; }

        public double onkostenvergoeding { get; set; }

        public double pentioenpremie { get; set; }

        public double prestatiebeloning { get; set; }

        public double salaris { get; set; }

        public double salaris_13e_maand { get; set; }

        public double vakantiegeld { get; set; }

        public double werkkleding { get; set; }

        public double ziektekostenverzekering { get; set; }

        public Klantgegevens Klantgegevens { get; set; }
    }
}