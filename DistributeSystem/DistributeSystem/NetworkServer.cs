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

    public class NetworkServer : NetworkBase
    {
        #region Init
        public NetworkServer(int port=11000,NetworkConfig config=null)
        {
            if (config == null)
            {
                config = new NetworkConfig();
                config.networkType = NetworkType.Server;
                config.port = port;
                config.localIP = GetLocalIP();
                config.serverIP = config.localIP;
            }
            this.config = config;
            base.ThreadInit( );
            SetNextTask(MyTask.StartServer);
        }
        private Socket server;
        public static List<StateObject> stateObjectList = new List<StateObject>();
        #endregion

        #region Thread
        public void Start()
        {
            SetNextTask(MyTask.StartServer);
        }
        public override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.StartServer:
                    StartServer();
                    break;
                case MyTask.WaitForAConnection:
                    WaitForAConnection();
                    break;
                case MyTask.ServerClose:
                    ThreadClose();
                    break;
                case MyTask.ServerSend:
                    SendPackages(packageNeedToSend._target);
                    break;
                case MyTask.ServerBoardCast:
                    BoardCast();
                    break;
            }
        }

    

        public void Close()
        {
            SetNextTask(MyTask.ServerClose);

        }

        public void ThreadClose()
        {
            AddMsg("ServerClose");
            Release();
            try
            {
                for (int i = 0; i < stateObjectList.Count; i++)
                {
                    var item = stateObjectList[i];
                    var s = item.workSocket;

                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }
            }
            catch (Exception) { }
            try { 
                server.Close();
                server = null;           
            }
            catch (Exception)
            {
                server = null;
                return;
            }
        }
        #endregion

        #region Network
        private void WaitForAConnection()
        {
            try
            {
                server.BeginAccept(new AsyncCallback(AcceptCallback), server);
            }
            catch (Exception) { }
            Console.WriteLine("Wait for more connection");
        }
        private void StartServer()
        {
            try
            {
                var port = config.port;
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                {
                    var ip = ipHostInfo.AddressList[i];
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ip;
                        Console.WriteLine("Local IP Address: {0}", ip.ToString());
                    }
                }
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                server = new Socket(ipAddress.AddressFamily,
                                                                 SocketType.Stream, ProtocolType.Tcp);
                server.Bind(localEndPoint);
                server.Listen(100);
                server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                SetEvent(DSEvent.StartServerSuccessful);
                AddMsg("Server Start Successful");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                SetEvent(DSEvent.StartServerFailure);
                AddMsg("Server Start Failure");
            }
        }
        public bool CheckStartServer(double time)
        {
            return CheckEvent(time, DSEvent.StartServerSuccessful, DSEvent.StartServerFailure);
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {

                // Get the socket that handles the client request.  
                Socket lis = (Socket)ar.AsyncState;
                IPEndPoint point = (IPEndPoint)lis.LocalEndPoint;
                Console.WriteLine("Build Up A Connection with " + point.Address.ToString());
                AddMsg("Build Up A Connection with " + point.Address.ToString());

                Socket handler = lis.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                stateObjectList.Add(state);

                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);

                SetNextTask(MyTask.WaitForAConnection);
            }
            catch (Exception) { }
        }

        //Decode Incomming package
        public override void ReceiveFinsh(StateObject state)
        {
            base.ReceiveFinsh(state);
            var content = state.sb.ToString();
            NetworkPackage np = new NetworkPackage();
            np.Deserialize(content);
            var cmd = np.Get<string>("CMD");
            if (cmd != null)
            {
                AddMsg("CMD: " + cmd);
                switch (cmd)
                {
                    case "NeedConnectedIPs":
                        var list = GetConnectedIP();
                        packageNeedToSend.Add("ConnectedIPs", list);
                        packageNeedToSend.SetTarget(state.workSocket); //send back to original 
                        SetNextTask(MyTask.ServerSend);
                        break;
                    case "SendCMDtoALL":
                        var dat = np.Get<string>("DAT");
                        packageNeedToSend.Add("DAT", dat);
                        SetNextTask(MyTask.ServerBoardCast);

                        break;
                }
            }
        }
    
        private void CheckClients()
        {
            int i = 0;
            while (i < stateObjectList.Count)
            {
                var item = stateObjectList[i];
                var s = item.workSocket;
                if (s == null)
                {
                    stateObjectList.RemoveAt(i);
                    continue;
                }
                if (!s.Connected)
                {
                    stateObjectList.RemoveAt(i);
                    continue;
                }
                try
                {

                if (!s.Poll(500, SelectMode.SelectWrite))
                {
                    stateObjectList.RemoveAt(i);
                    continue;

                }
                //////????????????????????????? Read = true means failuare?
                else if (s.Poll(500, SelectMode.SelectRead))
                {
                    stateObjectList.RemoveAt(i);
                    continue;

                }
                else if (s.Poll(500, SelectMode.SelectError))
                {
                    stateObjectList.RemoveAt(i);
                    continue;

                }
                }
                catch (Exception)
                {
                    stateObjectList.RemoveAt(i);
                    continue;
                }

                i++;
            }
        }
        public List<string> GetConnectedIP()
        {
            CheckClients();
            List<string> list = new List<string>();
            for(int i = 0; i < stateObjectList.Count; i++)
            {
                var item = stateObjectList[i];
                var s = item.workSocket;
                IPEndPoint localIpEndPoint = s.RemoteEndPoint as IPEndPoint;
                var str = localIpEndPoint.Address.ToString();
                list.Add(str);
            }
            return list;
        }

        private void BoardCast()
        {
            if (!packageNeedToSend.isEmpty())
            {
                var str = packageNeedToSend.Serialize();
                for (int i = 0; i < stateObjectList.Count; i++)
                {
                    var item = stateObjectList[i];
                    var socket = item.workSocket;
                    Send(socket,str);
                }
            }
        }
        #endregion

    }
}
