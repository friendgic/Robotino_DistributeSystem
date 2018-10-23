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
        public UDPClient_2 client;
        public Form1()
        {
            InitializeComponent();
            client = new UDPClient_2(11000);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.myPort = 11000;
            client.Start( );
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.myPort = 11001;
            client.Start();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            client.Send(bytes, "127.0.0.1", 11000);
        }
    }
}
