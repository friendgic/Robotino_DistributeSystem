using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTest2
{
    public class NetworkServer:NetworkBase
    {
        private Socket listener;
        public static List<StateObject> stateObjectList = new List<StateObject>();

        public NetworkServer(NetworkConfig config)
        {
            this.config = config;
            base.Init(config);
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
            }
        }

        private void WaitForAConnection()
        {
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
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
                listener = new Socket(ipAddress.AddressFamily,
                                                                 SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);
                listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
                 
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // State object for reading client data asynchronously  
        public class StateObject
        {
            // Client  socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 1024;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();

            public void Clear()
            {
                buffer = new byte[BufferSize];
                sb.Clear();

            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {

            // Get the socket that handles the client request.  
            Socket lis = (Socket)ar.AsyncState;
            IPEndPoint point = (IPEndPoint)lis.LocalEndPoint;
            Console.WriteLine("Build Up A Connection with " + point.Address.ToString());

            Socket handler = lis.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            stateObjectList.Add(state);

            SetNextTask(MyTask.WaitForAConnection);
        }


        }
}
