using RobotinoController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSGUI
{
    public partial class ControllerPannel : Form
    {
        public Robotino robot;
        public float speed = 0.10f;
        public float rotSpeed = 0.8f;
        public ControllerPannel()
        {
            InitializeComponent();
        }

        private void MMove(float x, float y, float r)
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
                MMove(0f, speed, 0);
            }
            if (e.KeyCode == Keys.Down)
            {

                MMove(0f, -speed, 0);
            }
            if (e.KeyCode == Keys.Left)
            {
                MMove(speed, 0f, 0);

            }
            if (e.KeyCode == Keys.Right)
            {
                MMove(-speed, 0f, 0);
            }

            if (e.KeyCode == Keys.Left && e.Shift)
            {
                MMove(0, 0f, rotSpeed);

            }
            if (e.KeyCode == Keys.Right && e.Shift)
            {
                MMove(0, 0, -rotSpeed);
            }
        }

        private void ControllerPannel_Load(object sender, EventArgs e)
        {

        }

        private void ControllerPannel_KeyUp(object sender, KeyEventArgs e)
        {
            robot.ManualControlMove(0, 0, 0);
        }
    }
}
