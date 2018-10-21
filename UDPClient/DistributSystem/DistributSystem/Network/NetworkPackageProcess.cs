using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace DistributeSystem
{
    public class NetworkPackageProcess:NetworkClient
    {
        #region Init
        protected NetworkPackageCompress sendPackage=new NetworkPackageCompress();
        #endregion
        #region Interface

        public override bool Start(int port = 11000)
        {
            bool ok=base.Start(port);
            
            return ok;
        }
        public void SendPackage(string ip,int port,bool compress=false)
        {
            sendPackage.targetIP = ip;
            sendPackage.targetPort = port;
            sendPackage.needToCompress = compress;
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
            var dat= sendPackage.SerializeBin();
            Send(dat, sendPackage.targetIP, sendPackage.targetPort);
            sendPackage.Reset();
        }
        protected override void Receiving(byte[] data)
        {
            base.Receiving(data);
            NetworkPackageCompress newPack = new NetworkPackageCompress();
            newPack.DeserializeBin(data);
            CommingPackage(newPack);
        }
        protected virtual void CommingPackage(NetworkPackageCompress pack)
        {

        }
        #endregion
    }
}
