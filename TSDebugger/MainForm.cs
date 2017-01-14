using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSDebugger
{
    public partial class MainForm : Form
    {
        TSConductorClient tsClient = new TSConductorClient();

        int selectedVariable = -1;
        BindingList<TSConductorClient.TSVariable> varList = null;

        static String WEBSITE_URL = "https://github.com/jonathanp0/TSInspector";

        public MainForm()
        {
            InitializeComponent();
        }

        private async void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await tsClient.StopPollingAsync();
            tsClient.Disconnect();
            
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
            statusLabel.Text = "Disconnected";
        }

        private async void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectDialog connectDialog = new ConnectDialog();
            connectDialog.ShowDialog();
            if (connectDialog.DialogResult == DialogResult.OK)
            {
                try
                {
                    statusLabel.Text = "Connecting to " + connectDialog.TSAddress + "...";
                    await tsClient.ConnectAsync(connectDialog.TSAddress, Int32.Parse(connectDialog.TSPort), "TSInspector");
                    statusLabel.Text = "Reading initial data from " + connectDialog.TSAddress;
                    connectToolStripMenuItem.Enabled = false;
                    disconnectToolStripMenuItem.Enabled = true;

                    Task<TSConductorClient.LocoData> getLocoTask = tsClient.GetLocoNameAsync();
                    if(await Task.WhenAny(getLocoTask, Task.Delay(5000)) != getLocoTask)
                    {
                        throw new InvalidOperationException("Timed out waiting for response from server, please try reconnecting");
                    }
                    TSConductorClient.LocoData locoName = await tsClient.GetLocoNameAsync();
                    locoLabel.Text = "Locomotive: " + locoName.Provider + " " + locoName.Product + " " + locoName.EngineName;

                    statusLabel.Text = "Connected to " + connectDialog.TSAddress;

                    Progress <List<TSConductorClient.TSVariable>> progressCallback = new Progress<List<TSConductorClient.TSVariable>>(updateTable);
                    await tsClient.StartPollingAsync(1000, progressCallback);

                    varList = null;
                    statusLabel.Text = "Polling terminated by server, please reconnect";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    statusLabel.Text = "Connection error";
                }
            }
        }

        private void updateTable(List<TSConductorClient.TSVariable> vars)
        {
            if (varList == null)
            {
                varList = new BindingList<TSConductorClient.TSVariable>(vars);
                tsVariableBindingSource.DataSource = vars;
            }

            for(int i = 0; i < vars.Count; ++i)
            {
                varList[i].ID = vars[i].ID; tsVariableBindingSource.ResetItem(i);
            }
            
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            TSConductorClient.TSVariable var = varList[e.RowIndex];
            if (var.ID >= 400)
                return;
            selectedVariable = var.ID;
            setValueText.Text = "Set: " + var.Name;
            setValueInput.Text = var.Current.ToString();
        }

        private async void setValueButton_Click(object sender, EventArgs e)
        {
            if(selectedVariable != -1 && tsClient.Connnected)
            {
                float result = 0;
                if (float.TryParse(setValueInput.Text, out result))
                    await tsClient.SetControllerValueAsync(selectedVariable, result);
                else
                    MessageBox.Show("Cannot convert " + setValueInput.Text + " to a number", "Invalid Number", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form about = new AboutBox();
            about.ShowDialog();
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(WEBSITE_URL);
        }
    }
}
