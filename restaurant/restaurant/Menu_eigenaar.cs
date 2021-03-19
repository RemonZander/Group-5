using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace restaurant
{
    public partial class Menu_eigenaar : Form
    {
        private restaurant.Code_Eigenaar_menu Code;

        private IO IO;

        private Database database;
        public Menu_eigenaar()
        {
            InitializeComponent();
        }
    }
}
