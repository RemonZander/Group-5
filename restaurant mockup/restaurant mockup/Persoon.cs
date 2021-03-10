using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public abstract class Persoon : type_BTW_prijs
    {
        public double Salaris
        {
            get
            {
                return Prijs;
            }
            set
            {
                Prijs = value;
            }
        }

        public double Inkomstenbelasting
        {
            get
            {
                return BTW;
            }
            set
            {
                BTW = value;
            }
        }

        public double Vakantiegeld
        {
            get => default;
            set
            {
            }
        }

        public double Salaris_13e_maand
        {
            get => default;
            set
            {
            }
        }

        public double Onkostenvergoeding
        {
            get => default;
            set
            {
            }
        }

        public double Werkkleding
        {
            get => default;
            set
            {
            }
        }

        public double Prestatiebeloning
        {
            get => default;
            set
            {
            }
        }

        public double Lease_auto
        {
            get => default;
            set
            {
            }
        }

        public double Ziektekostenverzekering
        {
            get => default;
            set
            {
            }
        }

        public double Pentioenpremie
        {
            get => default;
            set
            {
            }
        }
    }
}