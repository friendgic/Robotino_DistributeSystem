using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using RobotinoController;

namespace DistributeSystem
{
    public class RobotAgent:Agent
    {
        #region Init
        public Parameters parameters=new Parameters();
        public RobotinoController.Robotino robot = new RobotinoController.Robotino();

        private BroadCastAgent bcAgent;
        private Timer pulseTimer,paraTimer;
        private byte pulseCount=0;
        
        #endregion

        #region Interface
        public override bool Start(int port = 11000)
        {
            Configure.Read();
            uniquePort = true;
            var ok=  base.Start(port);
            if (!ok)
            {
                Close();
                return false;
            }
            //start timmer
            pulseTimer = new System.Timers.Timer(1000);
            pulseTimer.Elapsed += OnTimedEvent_Pulse;
            pulseTimer.AutoReset = true;
            pulseTimer.Enabled = true;

            paraTimer = new System.Timers.Timer(20);
            paraTimer.Elapsed += OnTimedEvent_Para;
            paraTimer.AutoReset = true;
            paraTimer.Enabled = true;
            //broadcastAgent
            bcAgent = new BroadCastAgent();
            bcAgent.Start(12000);
            //robot
            robot.mChangedEvent -= robotChangedEvent;
            robot.mChangedEvent += robotChangedEvent;
            //parameter
            parameters.name = Configure.name;
            return true;
        }


        public override void Close()
        {
            if(pulseTimer!=null)
            pulseTimer.Close();

            if (bcAgent != null)
            {
                bcAgent.Close();
            }
            base.Close();
        }
        public List<string> GetConnectedAgentFromBC()
        {
            List<string> ret = new List<string>();
            for(int i = 0; i < bcAgent.friends.Count; i++)
            {
                var item = bcAgent.friends[i];
                ret.Add(item.localIP + ":" + item.localPort);
            }
            return ret;
        }
        
        public void AddTarget(string target)
        {
            string ip;
            int port;
            try
            {
                var ipspilt = target.Split(':');
                  ip = ipspilt[0];
                  port = int.Parse(ipspilt[1]);
            }
            catch (Exception e)
            {
                SetEvent(DSEvent.Error, "Invalid ip");
                return;
            }
            if (ip == null) return;
            AddFriend(ip, port);

        }
        #endregion
        
        #region Thread
        protected override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.RobotAgent_Para:
                    SendOutPara();
                    break;
                case MyTask.RobotAgent_Pulse:
                    SendOutPulse();
                    break;
            }
        }
        #endregion

        #region override
        public override void AddFriend(string ip, int port)
        {
            RobotAgent newAgent = new RobotAgent();
            newAgent.localIP = ip;
            newAgent.localPort = port;
            newAgent.Feed(3);

            bool find = false;
            for (int i = 0; i < friends.Count; i++)
            {
                var item = friends[i];
                if (item.localIP == newAgent.localIP && item.localPort == newAgent.localPort)
                {
                    find = true;
                    friends[i].Feed(3);
                    break;
                }
            }
            if (!find)
            {
                friends.Add(newAgent);
                SetEvent(DSEvent.Agent, "New Agent");
            }

        }
        #endregion

        #region Private / protect method

        private void robotChangedEvent(float x,float y,float r)
        {
            parameters.speed_x = x;
            parameters.speed_y = y;
            parameters.rot = r;
        }
        private void OnTimedEvent_Para(object sender, ElapsedEventArgs e)
        {
            for(int i = 0; i < 9; i++)
            {
                var sensor=robot.ReadDistanceSensor(i);
                parameters.SetDisSensor(i, sensor);
            }
            Random r = new Random();
            var value = r.Next(0, 100);
            parameters.SetDisSensor(2, value);
            SetNextTask(MyTask.RobotAgent_Para);
        }
        private void OnTimedEvent_Pulse(Object source, ElapsedEventArgs e)
        {
            SetNextTask(MyTask.RobotAgent_Pulse);
        }

        private void SendOutPara()
        {
            if (parameters.changed)
            { 
                parameters.BuildUpPackage(ref sendingPack);
                //send to friend
                for (int i = 0; i < friends.Count; i++)
                {
                    AddSendTarget(friends[i].localIP, friends[i].localPort);
                }

                SendPackage();
            }
        }
        private void SendOutPulse()
        {
            sendingPack.Add("#", pulseCount++);
            
            //boardcast
            AddSendTarget("255.255.255.255", bcAgent.localPort);

            //send to friend
            for(int i = 0; i < friends.Count; i++)
            {
                AddSendTarget(friends[i].localIP, friends[i].localPort);
            }

            SendPackage();
        }

        protected override void CommingPackage(NetworkPackageCompress pack)
        {
            base.CommingPackage(pack);
                var ip = pack.targetIPs[0];
                var port = pack.targetPorts[0];

            byte pulse = 0;
            if (pack.GetFromBin<byte>("#", out pulse))
            {
                AddFriend(ip, port);
                parameters.MarkAllChanged();
            }

            var friend = GetFriend(ip, port) as RobotAgent;
            if (friend == null)
            {
                AddFriend(ip, port);
                parameters.MarkAllChanged();
                return;
            }
            friend.parameters.ReceivePackage(pack);
        }
        #endregion
    }
}
