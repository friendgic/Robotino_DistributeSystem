using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
 
namespace DistributeSystem
{
    public class NetworkPackageProcess:NetworkClient
    {
        #region Init
        protected NetworkPackageCompress sendingPack=new NetworkPackageCompress();
        #endregion
        #region Interface

        public override bool Start(int port = 11000)
        {
            bool ok=base.Start(port);
            
            return ok;
        }
        public void SendPackage(string ip,int port,bool compress=false)
        {
            sendingPack.targetIP = ip;
            sendingPack.targetPort = port;
            sendingPack.needToCompress = compress;
            SetNextTask(MyTask.Pack_Send);
        }

        #endregion

        #region Thread
        protected override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.Pack_Send:
                    SendPackageExe();
                    break;
            }
        }
        #endregion

        #region Private/Protect Method
        private void SendPackageExe()
        {
            var dat= sendingPack.SerializeBin();
            Send(dat, sendingPack.targetIP, sendingPack.targetPort);
            sendingPack.Reset();
        }
        protected override void Receiving(byte[] data, IPEndPoint e)
        {
            base.Receiving(data,e);
            NetworkPackageCompress newPack = new NetworkPackageCompress();
            newPack.DeserializeBin(data);
            newPack.targetIP = e.Address.ToString();
            newPack.targetPort = e.Port;
            CommingPackage(newPack);
        }
        protected virtual void CommingPackage(NetworkPackageCompress pack)
        {

        }
        #endregion
    }
}
