using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DistributeSystem
{
    public class RobotAgent:NetworkPackageProcess
    {
        #region Init
        public List<RobotAgent> list = new List<RobotAgent>();

        private byte pulseCount = 0;

        #endregion
        #region Interface
        Timer aTimer;
        public override bool Start(int port = 11000)
        {
            var ok=  base.Start(port);
            if (!ok) return false;
            //start timmer
            aTimer = new System.Timers.Timer(1000); 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            //set list
            list = new List<RobotAgent>();
            list.Add(this);

            //set other
            pulseCount = 0;
            return true;
        }

        public override void Close()
        {
            if(aTimer!=null)
            aTimer.Close();
            base.Close();
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            SetNextTask(MyTask.Agent_Pulse);
        }
        #endregion

        #region Thread
        protected override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.Agent_Pulse:
                    SendOutPulse();
                    break;
            }
        }
        #endregion

        #region Private / protect method


        private void SendOutPulse()
        {
            sendPackage.Add("#", pulseCount++);
            SendPackage("255.255.255.255", localPort);
        }

        protected override void CommingPackage(NetworkPackageCompress pack)
        {
            base.CommingPackage(pack);
            byte pulse = 0;
            if (pack.GetFromBin<byte>("#", out pulse))
            {
                SetEvent(DSEvent.Receive, "Pulse "+pulse.ToString());
            }
        }
        #endregion
    }
}
