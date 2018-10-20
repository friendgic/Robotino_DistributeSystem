using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DistributeSystem;

namespace DSGUI
{
    public partial class Form1 : Form
    {
        public NetworkClient client=new NetworkClient();
        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        { 
            client.Start(11000);
        }

        private void button2_Click(object sender, EventArgs e)
        { 
            client.Start(11001);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] test = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9 ,0};
            client.Send(test, "127.0.0.1", 11000);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            client.Start(11002);
        }
    }
}
