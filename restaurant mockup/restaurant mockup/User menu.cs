using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace restaurant_mockup
{
    public partial class User_menu : Form
    {
        private Database database;
        private User_code code;
        private IO IO;

        public User_menu()
        {
            InitializeComponent();
        }

        /*
        private void test()
        {
            Database data = new Database();

            data.Uitgaven.Inboedel[0].Afzender_gegevens.Adres.land = "nl";

            for (int a = 0; a < 10; a++)
            {
                data.klantgegevens[a].login_gegevens.Password = "";
                data.klantgegevens[a].adres.land = "NL";
                data.klantgegevens[a].Achternaam = "";
            }
        }
        */
    }
}
