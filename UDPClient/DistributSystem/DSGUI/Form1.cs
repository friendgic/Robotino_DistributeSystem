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
        public RobotAgent robotAgent=new RobotAgent();
        private RobotAgent showingParamerAgent;
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
            for (int i = 0; i < 20; i++)
            {
                listBox1.Items.Add("");

            }
            for (int i = 0; i < 20; i++)
            {
                listBox3.Items.Add("");

            }
            for (int i = 0; i < 20; i++)
            {
                listBox2.Items.Add("");
            }


        }
         
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            robotAgent.Close();
        }
         
        private void StartButton(object sender, EventArgs e)
        {
            if (!robotAgent.Active)
            {
                int port =int .Parse( portInput.Text);
                robotAgent.Start(port);
            }
            else
            {
                robotAgent.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string test = robotAgent.ReadADebugMsg();
            if (test != string.Empty) 
            debugText.Text = test + debugText.Text;
            if (debugText.Text.Length > 1000) debugText.Text = "";
            if (robotAgent != null)
            {
                if (robotAgent.Active)
                {
                    button1.Text = "Stop";

                    List<string> ips = new List<string>(); 
                    ips=robotAgent.GetConnectedAgentFromBC( );

                    for (int i =0; i < listBox1.Items.Count; i++)
                    {
                        if (i < ips.Count)
                        {
                            var str = ips[i];
                            listBox1.Items[i] = str;
                        }
                        else
                        {
                        listBox1.Items[i] = " ";

                        }
                    }
                    for(int i = 0; i < listBox3.Items.Count; i++)
                    {
                        if (i < robotAgent.friends.Count)
                        {
                            var str = robotAgent.friends[i].localIP+":"+robotAgent.friends[i].localPort;
                            listBox3.Items[i] = str;
                        }
                        else
                        {
                            listBox3.Items[i] = " ";
                        }
                    }

                    if (showingParamerAgent != null)
                    {
                        var par = showingParamerAgent.parameters;
                        listBox2.Items[1] = string.Format("Speed:{0},{1},{2}", par.speed_x,par.speed_y,par.speed_z);
                        listBox2.Items[2] = string.Format("Rotation:{0}", par.rot);
                        listBox2.Items[3] = string.Format("DisSensor:{0}  {1} ", par.v_DisSensor[0], par.v_DisSensor[1]);
                    }
                    else
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            listBox2.Items[i]="";
                        }
                    }
                    

                }
                else
                {
                    button1.Text = "Start";
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            var select = listBox1.SelectedItem as string;
            if (select.Length < 5)
            {
                return;
            }
            if (robotAgent.Active)
            {
                robotAgent.AddTarget(select);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (robotAgent.Active)
            {
                robotAgent.parameters.speed_x++;
            }
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int select = listBox3.SelectedIndex;
            if (robotAgent.Active)
            {
                if(select<robotAgent.friends.Count && select>=0)
                    showingParamerAgent = robotAgent.friends[select] as RobotAgent;
                else
                {
                    showingParamerAgent = null;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string rip = RobotIP.Text;

            try
            {
                robotAgent.robot.Connect(rip);
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                return;
            }
            MessageBox.Show("Connect Successful!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                robotAgent. robot.DisConnect();
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                return;
            }
            MessageBox.Show("Disconnect!");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ControllerPannel cp = new ControllerPannel();
            cp.robot = robotAgent. robot;
            cp.Show();
        }
    }
}
