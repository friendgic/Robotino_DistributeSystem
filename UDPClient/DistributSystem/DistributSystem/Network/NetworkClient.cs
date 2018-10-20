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
        protected int localPort=11000;
        protected UdpClient client;

        public NetworkClient()
        {
            localIP = GetLocalIP();
            base.ThreadInit();
        }
        ~NetworkClient()
        {
            SetNextTask(MyTask.Close);
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
                if (u.Client==null) return;
                IPEndPoint e = new IPEndPoint(IPAddress.Any, localPort);
                byte[] receiveBytes = client.EndReceive(ar, ref e);

                client.BeginReceive(new AsyncCallback(ReceiveCallBack), client);

                SetEvent(DSEvent.Receive, receiveBytes.Length + "b");
            } catch(Exception e)
            {
                SetEvent(DSEvent.Error, "Receive Data Error");
            }
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
        public void Start(int port = 11000)
        {
            this.localPort = port;
            SetNextTask(MyTask.StartClient);
        }
        public void Close()
        {
            SetNextTask(MyTask.Close);
            CheckEvent(1000, DSEvent.Released, DSEvent.Error);
        }
        public void Send(byte[] data,string ip,int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            client.Send(data, data.Length, ipEndPoint);
        }
        #endregion

    }
}
