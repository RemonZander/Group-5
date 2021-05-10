using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Review : review_feedback_base
    {
        //Er kan een rating gegeven worden tussen de 1 en de 5
        public int Rating { get; set; }
    }
}