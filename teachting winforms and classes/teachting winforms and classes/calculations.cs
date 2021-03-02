using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace teachting_winforms_and_classes
{
    public class calculations
    {
        public double Add(double input_a, double input_b)
        {
            return input_a + input_b;
        }

        public double substract(string input_a, string input_b)
        {
            return Convert.ToDouble(input_a) - Convert.ToDouble(input_b);
        }

        public double times(string input_a, string input_b)
        {
            return Convert.ToDouble(input_a) * Convert.ToDouble(input_b);
        }

        public double devision(string input_a, string input_b)
        {
            return Convert.ToDouble(input_a) / Convert.ToDouble(input_b);
        }
    }
}