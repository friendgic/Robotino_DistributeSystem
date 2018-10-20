using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DistributeSystem
{
    public class UDPClient_2:NetworkBase
    {
        public int myPort;
        public Queue<string> msgQueue;
        public Queue<DSEvent> EventQueue = new Queue<DSEvent>();
        public int msgQueueLength = 15;

        private string myIP;
        private Socket mySocket;
        #region Init
        public UDPClient_2(int port=11000)
        {
            msgQueue = new Queue<string>();
            myPort = port;
            myIP = GetLocalIP();
            //SetEvent(DSEvent.Init, "Client");
            base.ThreadInit();
        }
         ~UDPClient_2()
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
        private void StartClient()
        {
            try
                {
                if (mySocket != null)
                {
                    mySocket.Close();
                    mySocket = null;
                }

                var iep = new IPEndPoint(IPAddress.Any, myPort);
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    mySocket.Bind(iep);
                    StateObject state = new StateObject();

                mySocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                     new AsyncCallback(ReadCallback), state);

                SetEvent(DSEvent.Init,MyFullIP);
            }catch(Exception e)
            {
                SetEvent(DSEvent.Error,e.ToString());
            }
        }

        private void CloseClient()
        {
            Release();
            if(mySocket!=null)
            mySocket.Close();
        }
        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            if (handler == null) return;
            try
            {
                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 4)
                {
                    int length = BitConverter.ToInt32(state.buffer, 0);

                    // There  might be more data, so store the data received so far.  
                    byte[] usefulData = new byte[bytesRead];
                    for (int i = 0; i < bytesRead; i++)
                    {
                        usefulData[i] = state.buffer[i];
                    }
                    state.data.AddRange(usefulData);

                    if (bytesRead >= length)
                    {
                        ReceiveFinsh(state);
                        state.Clear();
                    }
                }
                else
                {
                    return;
                }
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                                           new AsyncCallback(ReadCallback), state);
            }
            catch (Exception)
            {
             
                SetEvent(DSEvent.Error);
                try
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    state.Clear();
                }
                catch (Exception)
                {

                }
            }
        }
        public virtual void ReceiveFinsh(StateObject state)
        {
            //remove 4 byte of the length information
            state.data.RemoveRange(0, 4);
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

        public void Start()
        {
            SetNextTask(MyTask.StartClient);
        }
        public void Close()
        {
            SetNextTask(MyTask.Close);
        }
        public void Send(byte[] data, string remoteIP, int port)
        {
            try
            {

            var ip = IPAddress.Parse(remoteIP);
            var iep = new IPEndPoint(ip,port);

            mySocket.BeginSend(data, 0, data.Length, 0,
              new AsyncCallback(SendCallback), mySocket);
            }catch(Exception e)
            { 
                SetEvent(DSEvent.Error);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes.", bytesSent);

            }
            catch (Exception)
            {
                SetEvent(DSEvent.Error);
            }
        }

        public string MyFullIP
        {
            get
            {
                return myIP + ":" + myPort;

            }
        }
        #endregion

    }
}
