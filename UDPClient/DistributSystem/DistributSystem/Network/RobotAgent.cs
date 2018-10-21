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
 
        public List<string> targetList = new List<string>();

        private byte pulseCount = 0;
        private BroadCastAgent bcAgent=new BroadCastAgent();
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
            
            targetList = new List<string>();
            //set other
            pulseCount = 0;

            //broadcastAgent
            bcAgent.Start(12000);
            return true;
        }

        public override void Close()
        {
            if(aTimer!=null)
            aTimer.Close();

            if (bcAgent != null)
            {
                bcAgent.Close();
            }
            base.Close();
        }

        public void GetConnectedAgentFromBC(out List<string>ips,out List<int>ports)
        {
            ips = new List<string>();
            ports = new List<int>();
            for(int i = 0; i < bcAgent.list.Count; i++)
            {
                var item = bcAgent.list[i];
                ips.Add( item.localIP);
                ports.Add(item.localPort);
            }
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

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            SetNextTask(MyTask.Agent_Pulse);
        }

        private void SendOutPulse()
        {
            sendingPack.Add("#", pulseCount++);
            SendPackage("255.255.255.255", bcAgent.localPort);
        }

        protected override void CommingPackage(NetworkPackageCompress pack)
        {
            base.CommingPackage(pack);
            byte pulse = 0;
            //if (pack.GetFromBin<byte>("#", out pulse))
            //{
            //    SetEvent(DSEvent.Receive, "Pulse "+pulse.ToString() +" < "+ pack.targetIP+":"+pack.targetPort.ToString());
            //}
        }
        #endregion
    }
}
