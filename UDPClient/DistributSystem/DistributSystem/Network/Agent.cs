using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DistributeSystem
{
    public class Agent: NetworkPackageProcess
    {
        #region Init
        public List<Agent> friends = new List<Agent>();
      
        private byte lastPulse = 0;
        private Timer aTimer;
        #endregion

        #region Interface
        public override bool Start(int port = 11000)
        {
            var ok = base.Start(port);
            if (!ok) return false;
            //set list
            friends = new List<Agent>();
            //set timer
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            return true;
        }
        public override void Close()
        {
            if (aTimer != null)
            {
                aTimer.Close();
            }
            base.Close();
        }
        /// <summary>
        /// feed the watchdog
        /// </summary>
        /// <param name="food"></param>
        public void Feed(byte food)
        {
            lastPulse = food;
        }
        public bool isDead()
        {
            if (lastPulse <= 0)
            {
                return true;
            }
            else
            {
                lastPulse--;
            }
            return false;
        }

        
        public virtual void AddFriend(string ip,int port)
        {
            Agent newAgent = new Agent();
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
        public Agent GetFriend(string ip,int port)
        {
            for(int i = 0; i < friends.Count; i++)
            {
                var item = friends[i];
                if(item.localIP==ip && item.localPort == port)
                {
                    return item;
                }
            }
            return null;
        }
        #endregion


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            int i = 0;
            while (i < friends.Count)
            {
                var item = friends[i];
                if (item.isDead())
                {
                    friends.RemoveAt(i);
                    continue;
                }
                i++;
            }
        }

    }
}
