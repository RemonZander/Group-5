﻿using System;
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
    public partial class Login : Form
    {
        private Code_Login_menu Code;
        private Menu_eigenaar Menu_eigenaar;

        private restaurant.Menu_Medewerker Menu_medewerker;
        private Menu_Gebruiker Menu_gebruiker;

        private IO IO;

        private Database database;

        public Login()
        {
            InitializeComponent();
        }

    }
}
