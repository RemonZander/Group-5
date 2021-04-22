using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace restaurant
{
    public class Feedback : review_feedback_base
    {
        public int recipient { get; set; }
    }
}