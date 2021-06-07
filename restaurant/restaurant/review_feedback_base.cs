using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public abstract class review_feedback_base
    {
        public int ID { get; set; }

        public int klantnummer { get; set; }

        public string message { get; set; }

        public int reservering_ID { get; set; }

        public bool annomeme { get; set; }

        public DateTime datum { get; set; }
    }
}