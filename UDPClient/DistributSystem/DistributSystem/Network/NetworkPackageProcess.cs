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
        public void AddSendTarget(string ip,int port)
        {
            sendingPack.AddTarget(ip, port);
        }
        public void SendPackage( bool compress=false)
        { 
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
            try
            {
                var dat = sendingPack.SerializeBin();
                Send(dat, sendingPack.targetIPs, sendingPack.targetPorts);
                sendingPack.Reset();
            }
            catch (Exception)
            {
                SetEvent(DSEvent.Error, "Send Package error");
            }
        }
        protected override void Receiving(byte[] data, IPEndPoint e)
        {
            try
            {

            base.Receiving(data,e);
            NetworkPackageCompress newPack = new NetworkPackageCompress();
            newPack.DeserializeBin(data);
            newPack.AddTarget(e.Address.ToString(),e.Port);
            CommingPackage(newPack);
            }
            catch (Exception)
            {
                SetEvent(DSEvent.Error, "Receive Package error");
            }
        }
        protected virtual void CommingPackage(NetworkPackageCompress pack)
        {

        }
        #endregion
    }
}
