﻿using System;
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
        public RobotAgent robot=new RobotAgent();
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
            
        }
         
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            robot.Close();
        }
         
        private void StartButton(object sender, EventArgs e)
        {
            if (!robot.Active)
            {
                int port =int .Parse( portInput.Text);
                robot.Start(port);
            }
            else
            {
                robot.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string test = robot.ReadADebugMsg();
            if (test != string.Empty) 
            debugText.Text = test + debugText.Text;
            if (debugText.Text.Length > 1000) debugText.Text = "";
            if (robot != null)
            {
                if (robot.Active)
                {
                    button1.Text = "Stop";
                }
                else
                {
                    button1.Text = "Start";
                }
            }
        }
    }
}
