using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace teachting_winforms_and_classes
{
    public partial class Form1 : Form
    {

        private calculations cal = new calculations();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("U moet wel 2 waardes invullen.");
                return;
            }
            else if (textBox1.Text.Contains(".") || textBox3.Text.Contains("."))
            {
                MessageBox.Show("U moet een komma gebruiken ipv punt.");
                textBox1.Text = null;
                textBox3.Text = null;
                return;
            }

            try
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    textBox2.Text = cal.Add(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox3.Text)).ToString();
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    textBox2.Text = cal.substract(textBox1.Text, textBox3.Text).ToString();
                }
                else if (comboBox1.SelectedIndex == 2)
                {
                    textBox2.Text = cal.times(textBox1.Text, textBox3.Text).ToString();
                }
                else if (comboBox1.SelectedIndex == 3)
                {
                    textBox2.Text = cal.devision(textBox1.Text, textBox3.Text).ToString();
                }
                else
                {
                    MessageBox.Show("U heeft geen operator gekozen");
                }
            }
            catch
            {
                comboBox1.SelectedIndex = -1;
                textBox1.Text = null;
                textBox3.Text = null;
                MessageBox.Show("U heeft een verkeerde waarde ingevoerd.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }
    }
}
