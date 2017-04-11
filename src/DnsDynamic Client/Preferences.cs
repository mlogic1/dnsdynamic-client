using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DnsDynamic_Client
{
    public partial class Preferences : Form
    {
        /* config  file variables */
        FileIniDataParser iniParser;
        IniData iniData;


        /* application variables */
        private String username;
        private String password;
        private String dnsServer;
        private int updateInterval;





        public Preferences()
        {
            InitializeComponent();
            iniParser = new FileIniDataParser();

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

            textBoxUsername.Text = username;
            textBoxPassword.Text = password;
            textBoxDNSServer.Text = dnsServer;
            textBoxInterval.Text = (updateInterval / 1000).ToString();

        }

        private void Preferences_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // convert seconds to miliseconds
            updateInterval = (int.Parse(textBoxInterval.Text) * 1000);
            username = textBoxUsername.Text;
            password = textBoxPassword.Text;
            dnsServer = textBoxDNSServer.Text;

            iniData["DNS"]["username"] = username;
            iniData["DNS"]["password"] = password;
            iniData["DNS"]["dns_server"] = dnsServer;
            iniData["DNS"]["updateinterval"] = updateInterval.ToString();

            iniParser.WriteFile("config.ini", iniData);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
