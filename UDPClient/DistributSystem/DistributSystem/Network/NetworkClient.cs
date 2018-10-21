using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace DistributeSystem 
{
    public class NetworkClient:ThreadBase
    {
        #region Init
        public string localIP;
        public int localPort=11000;
        protected UdpClient client;

        public NetworkClient()
        {
            
        }
        ~NetworkClient()
        {
            SetNextTask(MyTask.Close);
        }
        public bool Active
        {
            get
            {
                return threadRunning;
            }
        }
        protected override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.StartClient:
                    StartClient();
                    break;
                case MyTask.Close:
                    CloseClient();
                    break;
            }
        }

        
        #endregion

        #region Network
        private void CloseClient()
        {
            
            Release();
            if (client != null)
                client.Close();

        }

        private void StartClient()
        {
            try
            {
                if (client != null)
                {
                    client.Close();
                }
                localIP = GetLocalIP();
                client = new UdpClient(localPort);

                //////////////////////////////////////////// avoid lose connection//////////////////////////////////////////////////////////
                Socket s = client.Client;
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                s.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                SetEvent(DSEvent.StartClient, "Start Client Successful\n ip="+localIP+"\nPort:"+localPort);
                client.BeginReceive(new AsyncCallback(ReceiveCallBack), client);
            }
            catch (Exception e)
            { 
                SetEvent(DSEvent.Error, "Start Client Error");
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                UdpClient u = ar.AsyncState as UdpClient;
            if (u == null) return;
            if (u.Client == null) return;
                if (client == null) return;
                if (client.Client==null) return;
                IPEndPoint e = new IPEndPoint(IPAddress.Any, localPort);
                byte[] receiveBytes = client.EndReceive(ar, ref e);
                client.BeginReceive(new AsyncCallback(ReceiveCallBack), client);
                //call receive data
                Receiving(receiveBytes);
                SetEvent(DSEvent.Receive, receiveBytes.Length + "b");
            }
            catch (Exception e)
            {
                SetEvent(DSEvent.Error, "Receive Data Error");
            }
        }

        protected virtual void Receiving(byte[] data)
        {

        }


        private string GetLocalIP()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                var ip = ipHostInfo.AddressList[i];
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                }
            }
            return ipAddress.ToString();
        }
        #endregion

        #region Interface
        public virtual bool Start(int port = 11000)
        {
            this.localPort = port;
            base.ThreadInit();
            SetNextTask(MyTask.StartClient);
            return CheckEvent(1000, DSEvent.StartClient, DSEvent.Error);
        }
        public virtual void Close()
        {
            SetNextTask(MyTask.Close);
            CheckEvent(1000, DSEvent.Released, DSEvent.Error);
        }
        protected void Send(byte[] data,string ip,int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            client.Send(data, data.Length, ipEndPoint);
        }
        #endregion

    }
}
