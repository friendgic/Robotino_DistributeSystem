using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RobotinoController;

namespace DistributeSystem
{
    public partial class ControllerPannel : Form
    {
        public Robotino robot;
        public float speed = 0.10f;
        public float rotSpeed = 0.9f;
        public ControllerPannel()
        {
            InitializeComponent();
        }
        
        private void Move(float x, float y, float r)
        {
            try
            {
                robot.ManualControlMove(x, y, r);
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                return;
            }
        }
        private void ControllerPannel_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Up)
            {
                Move(0f, speed, 0);
            }
            if (e.KeyCode == Keys.Down)
            {

                Move(0f, -speed, 0);
            }
            if (e.KeyCode == Keys.Left)
            {
                Move(speed, 0f, 0);

            }
            if (e.KeyCode == Keys.Right)
            {
                Move(-speed, 0f, 0);
            }

            if (e.KeyCode == Keys.Left && e.Shift)
            {
                Move(0, 0f, speed);

            }
            if (e.KeyCode == Keys.Right && e.Shift)
            {
                Move(0, 0, -speed);
            }
        }

        private void ControllerPannel_Load(object sender, EventArgs e)
        {

        }
    }
}
