﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebexTeamsAPITest
{
    public partial class Form1 : Form
    {
        WebInteractions webInter = new WebInteractions();

        public Form1()
        {
            InitializeComponent();
            
        }

        public void Form1_Shown(object sender, EventArgs e)
        {
            while(true)
            {

                webInter.Login();

                webInter.postToTeams();

                System.Threading.Thread.Sleep(45000);

            }
            
        }

       
    }
}
