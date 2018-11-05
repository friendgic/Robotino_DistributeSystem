using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DistributeSystem;

namespace DSGUI
{
    public partial class Form1 : Form
    {
        public RobotAgent robotAgent=new RobotAgent();
        private RobotAgent showingParamerAgent;
        private int selectRobot = 1;
        private int FirstRobotIndex=0;
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
                timer2.Stop();
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
                        listBox2.Items[0] = string.Format("Name:{0}",par.v_name);
                        listBox2.Items[1] = string.Format("Speed:{0},{1}", par.v_Movement[0],par.v_Movement[1]);
                        listBox2.Items[2] = string.Format("Rotation:{0}", par.v_Movement[2]);
                        listBox2.Items[3] = string.Format("DisSensor:{0}  {1} ", par.v_DisSensor[2], par.v_DisSensor[3]);
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
            //if (robotAgent.Active)
            //{
            //    var speedx = robotAgent.parameters.v_Movement[0];
            //    robotAgent.parameters.SetSpeedX(speedx + 1);
            //}
            //if (robotAgent.Active)
            //{
            //    dt = DateTime.Now;
            //    timer3.Start();
            //}
            
                robotAgent.robot.Grab();
            

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (timer2.Enabled)
            {
                timer2.Stop();
            }
            else
            {
                if (robotAgent.Active)
                {
                    robotAgent.parameters.SetName("Robot 1");
                }
                timer2.Start();
            }
            selectRobot = 1;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (timer2.Enabled)
            {
                timer2.Stop();
            }
            else
            {
                timer2.Start();

                if (robotAgent.Active)
                {
                    robotAgent.parameters.SetName( "Robot 2");

                    var friends = robotAgent.friends;
                    FirstRobotIndex = -1;
                    for (int i = 0; i < friends.Count; i++)
                    {
                        var item = friends[i] as RobotAgent;
                        if(item.parameters.v_name=="Robot 1")
                        {
                            FirstRobotIndex = i;
                        }
                    }
                    if (FirstRobotIndex == -1)
                    {
                        MessageBox.Show("not find robot 1");
                        timer2.Stop();
                    }
                }
            }
            selectRobot = 2;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            
            if (!robotAgent.Active)
            {
                timer2.Stop();
                return;
            }
            try
            {

                switch (selectRobot)
                {
                    case 1:
                        {
                            var sensor2 = robotAgent.parameters.v_DisSensor[2];
                            var sensor3 = robotAgent.parameters.v_DisSensor[3];
                            var mid = (sensor2 + sensor3) / 2f;
                            if (mid < 0.7f)
                                robotAgent.robot.ManualControlMove(0.04f, 0f, 0f);
                            else
                                robotAgent.robot.ManualControlMove(0.0f, 0f, 0f);

                            break;
                        }
                    case 2:
                        {
                            var robot1 = robotAgent.friends[FirstRobotIndex] as RobotAgent;
                            var sensor2 = robot1.parameters.v_DisSensor[2];
                            var sensor3 = robot1.parameters.v_DisSensor[3];
                            var mid = (sensor2 + sensor3) / 2f;
                            if (mid > 0.7f)
                            {
                                robotAgent.robot.ManualControlMove(0.04f, 0f, 0f);
                            }
                            else
                            {
                                robotAgent.robot.ManualControlMove(0.0f, 0f, 0f);
                            }
                            break;
                        }
                }
            }
            catch (Exception exc)
            {
                timer2.Stop();
                MessageBox.Show(exc.ToString());
            }
        }

        private float test;
        DateTime dt;
        double mid = 0;
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (robotAgent.Active)
            {
                for(int i = 0; i < 10000; i++)
                {

                    var value = robotAgent.robot.ReadDistanceSensor(0);
                    if (test != value)
                    {

                        TimeSpan ts = DateTime.Now - dt;
                        var dtime = ts.TotalMilliseconds.ToString();
                        test = value;
                        if(ts.TotalMilliseconds>0 && ts.TotalMilliseconds < 20)
                        {
                            mid = mid * 0.2 + ts.TotalMilliseconds * 0.8;
                        }
                        Console.WriteLine("dat:" + test + " time:" + dtime + " mid:" + mid);
                        dt = DateTime.Now;
                    }
                
                    for(int k = 0; k < 10000; k++)
                    {

                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
