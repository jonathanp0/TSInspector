using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSDebugger
{
    public partial class ConnectDialog : Form
    {
        public ConnectDialog()
        {
            InitializeComponent();
            LinkLabel.Link tscLink = new LinkLabel.Link(0, 100, "http://tsconductor.trainsim.org/");
            linkLabel1.Links.Add(tscLink);
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            TSAddress = addressText.Text;
            TSPort = portText.Text;
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private String _address;
        public String TSAddress { get { return _address; } set { _address = value; } }
        private String _port;
        public String TSPort { get { return _port; } set { _port = value; } }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }

    
}
