using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DistributeSystem
{
    public class BroadCastAgent: NetworkPackageProcess
    {
        #region Init
        public List<BroadCastAgent> list = new List<BroadCastAgent>();

        #endregion

        #region Interface
        public override bool Start(int port = 11000)
        {
            var ok = base.Start(port);
            if (!ok) return false;
            //set list
            list = new List<BroadCastAgent>();
 
            return true;
        }

        public override void Close()
        { 
            base.Close();
        }

     
    
        #endregion
  
        #region Private / protect method 
        protected override void CommingPackage(NetworkPackageCompress pack)
        {
            base.CommingPackage(pack);
            byte pulse = 0;
            if (pack.GetFromBin<byte>("#", out pulse))
            {
                BroadCastAgent newAgent = new BroadCastAgent();
                newAgent.localIP = pack.targetIP;
                newAgent.localPort = pack.targetPort;

                bool find = false;
               for(int i=0;i<list.Count;i++)
                {
                    var item = list[i];
                    if(item.localIP==newAgent.localIP && item.localPort == newAgent.localPort)
                    {
                        find = true;
                        list[i] = newAgent;
                        break;
                    } 
                }
               if(!find)
                        list.Add(newAgent);

                SetEvent(DSEvent.Receive, "Pulse " + pulse.ToString() + " < " + pack.targetIP + ":" + pack.targetPort.ToString());

            }
        }
        #endregion
    }
}
