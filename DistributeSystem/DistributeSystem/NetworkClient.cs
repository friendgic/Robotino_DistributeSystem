using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DistributeSystem
{
    // State object for reading client data asynchronously  
   
    public class NetworkClient : NetworkBase
    {
        #region Init
        public NetworkClient(string ServerIP,int port=11000)
        {
            ServerIP=ServerIP.Replace(" ", string.Empty);
            var config = new NetworkConfig();
            config.localIP = GetLocalIP();
            config.serverIP = ServerIP;
            config.port = port;
            config.networkType = NetworkType.Client;

            this.config = config;
            base.ThreadInit();
            SetNextTask(MyTask.StartClient);

        }
        #endregion

        #region Thread
        public override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.StartClient:
                    StartClient();
                    break;
                case MyTask.ClientClose:
                    CloseClient();
                    break;
                case MyTask.ClientSend:
                    SendPackages(client);
                    break;
            }
        }

   
        #endregion

        #region Network
        public Socket client;
        public List<string> connectedIP = new List<string>();

        public void Start()
        {
            SetNextTask(MyTask.StartClient);
        }
        private void StartClient()
        {
            try
            {

            var ip = config.serverIP;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, config.port);

            // Create a TCP/IP socket.  
            Socket s = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            s.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), s);

            }
            catch
            {
                AddMsg("Start Client Failure");

            }
            
        }

        public bool CheckStartClient(double time)
        {
            return CheckEvent(time, DSEvent.StartClientSuccessful, DSEvent.StartClientFailure);
        }

        private void CloseClient()
        {
            AddMsg("Client Close");
            Release();
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception) { }
        }
        public void Close()
        {
            SetNextTask(MyTask.ClientClose);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                  client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);
                var str = client.RemoteEndPoint.ToString();

                Console.WriteLine("Socket connected to {0}",str);
                AddMsg("Start Client Successful, Connect to "+str);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);

                SetEvent(DSEvent.StartClientSuccessful);
            }
            catch (Exception e)
            {
                SetEvent(DSEvent.StartClientFailure);
                Console.WriteLine(e.ToString());
                AddMsg("Start Client Failure");
            }
        }

        public List<string> GetConnectedIP(float waitTime=1000)
        {
            if (!CheckConnection())
            {
                return new List<string>();
            }

            AddMsg("Send NeedConnectedIPs ");
            packageNeedToSend.Add("CMD", "NeedConnectedIPs");
         
            SetNextTask(MyTask.ClientSend);

            if (CheckEvent(waitTime, DSEvent.ClientSendAndReceive, DSEvent.Error))
            {
                return connectedIP;
            }

            return null;
        }
        public string GetServerIP()
        {
            if (!CheckConnection())
            {
                return "";
            }
            IPEndPoint localIpEndPoint = client.RemoteEndPoint as IPEndPoint;
            var str = localIpEndPoint.Address.ToString();
            return str;
        }
        private bool CheckConnection()
        {
            if (client == null)
            {
                return false;
            }
            if (!client.Connected)
            {
                return false;
            }
            try
            {

                if (!client.Poll(500, SelectMode.SelectWrite))
                {
                    return false;

                }
                //////????????????????????????? Read = true means failuare?
                else if (client.Poll(500, SelectMode.SelectRead))
                {
                    return false;

                }
                else if (client.Poll(500, SelectMode.SelectError))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public override void ReceiveFinsh(StateObject state)
        {
            base.ReceiveFinsh(state);
            var content = state.sb.ToString();
            NetworkPackage np = new NetworkPackage();
            np.DeserializeJson(content);
            var dat = np.Get<List<string>>("ConnectedIPs");
            if (dat != null)
            {
                connectedIP = dat;
                SetEvent(DSEvent.ClientSendAndReceive);
                AddMsg("Receive ConnectedIPs list");
            }
            var cmd = np.Get<string>("CMD");
            if (cmd != null)
            {
                AddMsg("Receive CMD:" + cmd);
                switch (cmd)
                {
                    
                }
            }
        }

        public bool SendCommandToALL(string command, float waitTime = 1000)
        {
            if (!CheckConnection())
            {
                return false;
            }
            AddMsg("Start to send CMD to ALL, cmd:" + command);
            packageNeedToSend.Add("CMD", "SendCMDtoALL");
            packageNeedToSend.Add("DAT", command);

            SetNextTask(MyTask.ClientSend);

            if (CheckEvent(waitTime, DSEvent.ClientSend, DSEvent.Error))
            {
                return true;
            }
            return false;
        }
        #endregion

    }
}
