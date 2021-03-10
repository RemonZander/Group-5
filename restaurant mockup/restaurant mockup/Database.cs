using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant_mockup
{
    public struct Database
    {
        public Menukaart menukaart
        {
            get => default;
            set
            {
            }
        }

        public Klantgegevens[] klantgegevens
        {
            get => default;
            set
            {
            }
        }

        public Reserveringen[] reservaties
        {
            get => default;
            set
            {
            }
        }
    }
}