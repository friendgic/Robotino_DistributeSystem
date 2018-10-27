using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DistributeSystem
{
    public class BroadCastAgent : Agent
    {
        #region Interface
        public override bool Start(int port = 11000)
        {
            uniquePort = false;
            if(Configure.LINUX)
                specialIP = "255.255.255.255";
            return base.Start(port);
        }
        #endregion

        #region Private / protect method     
        protected override void CommingPackage(NetworkPackageCompress pack)
        {
            base.CommingPackage(pack);
            byte pulse = 0;
            if (pack.GetFromBin<byte>("#", out pulse))
            { 
                var ip= pack.targetIPs[0];
                var port= pack.targetPorts[0];
                AddFriend(ip, port);

                SetEvent(DSEvent.Receive, "Pulse " + pulse.ToString() + " < " + ip + ":" +port.ToString());
            }
        }
        #endregion
    }
}
