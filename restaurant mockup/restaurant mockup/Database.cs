using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public class Database
    {
        public Menukaart menukaart
        {
            get => default;
            set
            {
            }
        }

        public Klantgegevens[] klantgegevens { get; set; }

        public Reserveringen[] reservaties
        {
            get => default;
            set
            {
            }
        }

        public Uitgaven Uitgaven
        {
            get => default;
            set
            {
            }
        }

        public Inkomsten Inkomsten
        {
            get => default;
            set
            {
            }
        }
    }
}