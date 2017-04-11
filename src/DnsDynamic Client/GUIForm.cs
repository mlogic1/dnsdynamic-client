using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace DnsDynamic_Client
{
    public partial class GUIForm : Form
    {
        /* config  file variables */
        FileIniDataParser iniParser;
        IniData iniData;


        /* application variables */
        private String username;
        private String password;
        private String ipAddress;
        private String dnsServer;
        private int updateInterval;

        private const String UserAgent = "DNS Dynamic Unofficial Client";
        private bool UserQuit = false;


        /* system tray variables */
        NotifyIcon trayIcon;


        



        public GUIForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.internet;
            iniParser = new FileIniDataParser();
            trayIcon = CreateNotifyIcon();

            ReloadConfiguration();
        }

        private void GUIForm_Load(object sender, EventArgs e)
        {
            timerDataSender.Start();
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
            
        }


        private NotifyIcon CreateNotifyIcon()
        {
            NotifyIcon trayicon = new NotifyIcon();
            trayicon.Icon = Properties.Resources.internet;
            trayicon.Visible = true;
            trayicon.Text = "DNS Client";
            trayicon.ContextMenuStrip = TrayIconMenuStrip;
            trayicon.DoubleClick += Trayicon_DoubleClick;
            return trayicon;
        }

        private void Trayicon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        public void UpdateIpAddress()
        {
            WebRequest request = WebRequest.Create("http://myip.dnsdynamic.org/");
            ((HttpWebRequest)request).UserAgent = UserAgent;
            string responseFromServer = "";

            try
            {
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(responseStream);
                // Read the content.  
                responseFromServer = reader.ReadToEnd();
                // Display the content.  
                Console.WriteLine(responseFromServer);
                // Clean up the streams and the response.  
                reader.Close();
                response.Close();
                ipAddress = responseFromServer;
                textBoxIPAddress.Text = ipAddress;
            }
            catch (Exception exception)
            {
                StatusLabel.Text = "Error Getting IP";
                String logMessage = DateTime.Now.ToString() + ": " + exception.Message.ToString() + Environment.NewLine;
                File.AppendAllText("logfile.log", logMessage);
            }
        }


        public void SendDataToApi()
        {
            StatusLabel.Text = "Sending data to API";
            String api = String.Format("https://www.dnsdynamic.org/api/?hostname={0}&myip={1}", dnsServer, ipAddress);


            WebRequest request = WebRequest.Create(api);
            ((HttpWebRequest)request).UserAgent = UserAgent;
            request.Credentials = new NetworkCredential(username, password);
            string responseFromServer = "";


            try
            {
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(responseStream);
                // Read the content.  
                responseFromServer = reader.ReadToEnd();
                // Display the content.  
                // Clean up the streams and the response.  
                reader.Close();
                response.Close();
            }
            catch (Exception exception)
            {
                StatusLabel.Text = "Error sending data";
                String logMessage = DateTime.Now.ToString() + ": " + exception.Message.ToString() + Environment.NewLine;
                File.AppendAllText("logfile.log", logMessage);
            }

            StatusLabel.Text = responseFromServer;
        }

        private void buttonUpdateIpAddress_Click(object sender, EventArgs e)
        {
            UpdateIpAddress();
        }

        private void timerDataSender_Tick(object sender, EventArgs e)
        {
            trayIcon.Icon = Properties.Resources.internet_busy;
            UpdateIpAddress();
            SendDataToApi();
            trayIcon.Icon = Properties.Resources.internet;
        }

        private void TrayIconMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            String name = e.ClickedItem.Name;

            switch(name){
                case "menuShow":
                    this.Visible = true;
                    break;
                case "menuExit":
                    UserQuit = true;
                    this.Close();
                    break;

                default:
                    break;
            }
        }

        private void GUIForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.ShowInTaskbar = true;
            }
            else
            {
                this.ShowInTaskbar = false;
            }
            
        }

        private void toSystemTrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void TrayIconMenuStrip_DoubleClick(object sender, EventArgs e)
        {

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void GUIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!UserQuit)
            {
                e.Cancel = true;                this.Visible = false;
            }
        }

        private void updateNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trayIcon.Icon = Properties.Resources.internet_busy;
            UpdateIpAddress();
            SendDataToApi();
            trayIcon.Icon = Properties.Resources.internet;
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Preferences prefDialog = new Preferences();
            DialogResult result =  prefDialog.ShowDialog(this);

            if(result == DialogResult.OK)
            {
                ReloadConfiguration();
            }
        }


        private void ReloadConfiguration()
        {
            try
            {
                iniData = iniParser.ReadFile("config.ini");
            }
            catch (Exception e)
            {
                MessageBox.Show("A configuration file was not found. Creating a default configuration.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                File.WriteAllText("config.ini", Properties.Resources.config);
                String logMessage = DateTime.Now.ToString() + ": " + e.Message.ToString() + Environment.NewLine;
                File.AppendAllText("logfile.log", logMessage);
                try
                {
                    iniData = iniParser.ReadFile("config.ini");
                }
                catch (Exception e2)
                {
                    MessageBox.Show("Configuration file is corrupt. Please delete it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    String logMessage2 = DateTime.Now.ToString() + ": " + e2.Message.ToString() + Environment.NewLine;
                    File.AppendAllText("logfile.log", logMessage2);
                    this.Close();
                }
            }

            username = iniData["DNS"]["username"];
            password = iniData["DNS"]["password"];
            dnsServer = iniData["DNS"]["dns_server"];
            updateInterval = int.Parse(iniData["DNS"]["updateinterval"]);
            timerDataSender.Interval = updateInterval;

            textBoxUsername.Text = username;
            textBoxPassword.Text = password;
            textBoxDNSServer.Text = dnsServer;
            textBoxInterval.Text = (updateInterval / 1000).ToString();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }
    }
}
