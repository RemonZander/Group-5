﻿using System;
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
    public partial class Eigenaars_menu : Form
    {
        private Eigenaars_code code;
        private Database database;
        private IO IO;

        public Eigenaars_menu()
        {
            InitializeComponent();
        }
    }
}
