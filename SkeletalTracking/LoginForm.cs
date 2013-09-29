using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;
using System.Threading;

namespace SkeletalTracking
{
    public partial class LoginForm : Form
    {
        private GridClient Client;
        private EventHandler<SimConnectedEventArgs> login_handler;

        /// <summary>
        /// Creates a new LoginForm with the provided GridClient. This is how the GridClient is returned
        /// from the Form (through pass by reference).
        /// </summary>
        /// <param name="client">The GridClient to be returned by a successful login</param>
        public LoginForm(GridClient client)
        {
            this.Client = client;
            this.login_handler = new EventHandler<SimConnectedEventArgs>(Network_SimConnected);
            this.Client.Network.SimConnected += login_handler;

            InitializeComponent();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            String fName = this.FirstNameTxtBx.Text;
            String lName = this.LastNameTxtBx.Text;
            String pwd = this.PwdTxtBx.Text;

              // change for a different starting location
            string startLocation = NetworkManager.StartLocation("VDC2",
                 128, 128, 27);
            //this.Client.Settings.LOGIN_SERVER = "http://virtualdiscoverycenter.net:8002/";

            //Set login params and log into the server
            LoginParams loginParams = this.Client.Network.DefaultLoginParams(fName, lName, pwd, "Devotion", "v0.9.2");
            loginParams.Start = startLocation;
            loginParams.URI = "http://localhost:8002/";
            loginParams.AgreeToTos = false;
            try
            {
                this.Client.Network.BeginLogin(loginParams);
            }
            catch (Exception)
            {
                //TODO
            }
        }

        void Network_SimConnected(object sender, SimConnectedEventArgs e)
        {
            if (this.Client.Network.Connected)
            {
                this.Client.Network.SimConnected -= this.login_handler;
                this.Close();
            }
            else
            {
                this.ErrorMessageLbl.Text = "Failed: " + this.Client.Network.LoginMessage;
                this.Client.Network.Logout();
            }
        }
    }
}
