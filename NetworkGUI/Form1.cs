using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using DistributeSystem;

namespace NetworkGUI
{
    public partial class Form1 : Form
    {
        private NetworkServer server;
        private NetworkClient client;
        private bool init = false;

        public Form1()
        {
            InitializeComponent();
            timer2.Start();
        }


        private void Init(bool t = true)
        {
            init = t;
            ServerSelect.Enabled = !t;
            ClientSelect.Enabled = !t;
            InitButton.Enabled = !t;
        }
        private void Init_Button(object sender, EventArgs e)
        {
            Init();
            var port = 11000;
            if (!int.TryParse(port_text.Text, out port))
            {
                port = 11000;
                port_text.Text = "11000";
            }
            if (ServerSelect.Checked)
            {
                server = new NetworkServer(port);
                ServerIP.Text = server.GetLocalIP();
                if (!server.CheckStartServer(500))
                {
                    MessageBox.Show("Error , build server error!");
                    return;
                }
                refresh_button(null, null);
            }
            else
            {
                string ip = ServerIP.Text;
                client = new NetworkClient(ip, port);
                if (!client.CheckStartClient(500))
                {
                    MessageBox.Show("Error , Connect to server timeout!");
                    return;
                }

                refresh_button(null, null);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.Close();
                server = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        private void AddItemToList(string rolle, string ip)
        {
            ListViewItem item = new ListViewItem(rolle);
            item.SubItems.Add(ip);
            listView1.Items.Add(item);
        }
        private void RefreshList()
        {
            if (server != null && ServerSelect.Checked)
            {

                listView1.Items.Clear();
                AddItemToList("Server", server.GetLocalIP());
                var list = server.GetConnectedIP();
                for (int i = 0; i < list.Count; i++)
                {
                    AddItemToList("Client", list[i]);

                }
            }
            if (client != null && ClientSelect.Checked)
            {
                listView1.Items.Clear();
                AddItemToList("Server", client.GetServerIP());
                var list = client.GetConnectedIP();
                if (list == null)
                {
                    MessageBox.Show("Time Out");
                    return;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    AddItemToList("Client", list[i]);
                }
            }
        }
        private void refresh_button(object sender, EventArgs e)
        {
            RefreshList();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                server.Close();
                server = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
            Init(false);
            listView1.Items.Clear();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool set = checkBox1.Checked;
            if (set)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ip = "";
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                ip = item.SubItems[1].Text;
            }
            selectedIP_Text.Text = ip;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //NetworkPackage np = new NetworkPackage();
            //np.Add("test", t.Millisecond);
            //var str=np.Serialize();
            //NetworkPackage np2 = new NetworkPackage();
            //np2.Deserialize(str);
            //double dat = np2.Get<double>("test");

            NetworkPackageCompress np = new NetworkPackageCompress();
            np.Add("test", (int)(1522131233));
            np.Add("testb", (byte)(255));
            np.Add("test1", (float)(15.33321365));
            np.Add("test2", (double)(155.33213365));
            np.Add("test3", "abcdsfa213124123d");
            var testList = new List<int>() { 121312, 23123, 3213 };
            var testList2 = new List<float>() { 1.2131f, 2.213122f, 3.3213f };
            var testList3 = new List<double>() { 1.1231, 2.3122, 3.33324 };
            var testList4 = new List<byte>() { 133, 232, 32, 41 };
            np.Add("test4", testList);
            np.Add("test5", testList2);
            np.Add("test6", testList3);
            np.Add("test7", testList4);

       

            DateTime t = DateTime.Now;
            var dat = np.Serialize();
            var datComp = np.Zip(dat);
            NetworkPackageBin npjson = new NetworkPackageBin();
            npjson.Deserialize(dat);
            DateTime t2 = DateTime.Now;
            double djson = (t2 - t).TotalMilliseconds;
             
            t = DateTime.Now;
            var dat2 = np.SerializeBin();
            var dat2Com = np.ZipBin(dat2.ToArray());
            NetworkPackageBin np2 = new NetworkPackageBin();
            np2.DeserializeBin(dat2);
            t2 = DateTime.Now;
            double dbin = (t2 - t).TotalMilliseconds;

            var test = np2.Get<byte>("testb");

            MessageBox.Show(djson.ToString() + "\n"
                                            + dbin.ToString() + "\n" + test.ToString());
        }

        private void SendToALL_Button(object sender, EventArgs e)
        {
            var cmd = Command_text.Text;
            if (client != null)
            {
                if (!client.SendCommandToALL(cmd))
                {
                    MessageBox.Show("Send CMD Error");
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            if (server != null)
            {
                list = server.MsgQueue.ToList<string>();
            }
            else if (client != null)
            {
                list = client.MsgQueue.ToList<string>();
            }
            else
            {
                return;
            }
            msgListView.BeginUpdate();
            msgListView.Items.Clear();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                ListViewItem it = new ListViewItem(item);

                msgListView.Items.Add(item);
            }
            msgListView.EndUpdate();

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            bool set = checkBox2.Checked;
            if (set)
            {
                timer2.Start();
            }
            else
            {
                timer2.Stop();
            }
        }
    }
}
