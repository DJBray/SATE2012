using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpensimTestServer
{
    public partial class Form1 : Form
    {
        HttpForwarder loginService;

        public Form1()
        {
            InitializeComponent();
            loginService = new HttpForwarder();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            loginService.StartListening();
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            loginService.StopListening();
        }
    }
}
