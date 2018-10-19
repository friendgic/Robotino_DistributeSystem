using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DistributeSystem.Network
{
    public class UDPClient:NetworkBase
    {
        public int myPort;
        public Queue<string> msgQueue;
        public Queue<DSEvent> EventQueue = new Queue<DSEvent>();
        public int msgQueueLength = 15;

        private string myIP;
        private Socket mySocket;
        #region Init
        public UDPClient(int port)
        {
            msgQueue = new Queue<string>();
            myPort = port;
            myIP = GetLocalIP();
            AddMsg(myIP);
            base.ThreadInit();
        }
        public override void RunTask(MyTask task)
        {
            base.RunTask(task);
            switch (task)
            {
                case MyTask.StartClient:
                    AddMsg("Start Client");
                    StartClient();
                    break;
            }
        }
        #endregion

        #region Network
        private void StartClient()
        {
            var iep = new IPEndPoint(IPAddress.Any, myPort);
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            StateObject state = new StateObject();

            mySocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                 new AsyncCallback(ReadCallback), state);
        }
        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
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
            catch (Exception e)
            {
                AddMsg(e.ToString());
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
        public void Send(string msg, string remoteIP, string remotePort)
        {
            var ip = IPAddress.Parse(remoteIP);

        }
            #endregion

            #region Utility
            public void AddMsg(string msg)
        {
            if (msgQueue.Count > msgQueueLength) msgQueue.Dequeue();
            msgQueue.Enqueue(msg);
            Console.WriteLine(msg);
        }
        public DSEvent GetEvent()
        {
            if (EventQueue.Count > 0)
            {
                return EventQueue.Dequeue();
            }
            return DSEvent.None;
        }
        public void SetEvent(DSEvent str)
        {
            if (EventQueue == null)
                EventQueue = new Queue<DSEvent>();
            EventQueue.Enqueue(str);
        }
        public bool CheckEvent(double time, DSEvent successful, DSEvent failure)
        {
            DateTime t = DateTime.Now;
            while ((DateTime.Now - t).TotalMilliseconds < time)
            {
                var eve = GetEvent();
                if (eve != DSEvent.None)
                {
                    if (eve == successful)
                    {
                        return true;
                    }
                    if (eve == failure)
                    {
                        return false;
                    }
                    SetEvent(eve);//turn back event;
                }
                Thread.Sleep(1);
            }

            return false;
        }
        #endregion
    }
}
