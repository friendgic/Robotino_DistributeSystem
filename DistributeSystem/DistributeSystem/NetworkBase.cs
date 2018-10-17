using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DistributeSystem
{
  
    public class NetworkBase
    {
        #region Init

        private Thread myThread;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        public bool threadEnable = false;
        public bool threadRunning;
        public Queue<MyTask> taskQueue;
        public Queue<DSEvent> EventQueue=new Queue<DSEvent>();
        public Queue<string> MsgQueue = new Queue<string>();
        private object locker = new object();
        public NetworkConfig config;
        public NetworkPackage packageNeedToSend = new NetworkPackage();

        #endregion

        #region Thread

        ~NetworkBase()
        {
            Release();
        }
        public NetworkBase()
        {
        }

        /// <summary>
        /// Start Thread
        /// </summary>
 
        public void ThreadInit()
        {
            taskQueue = new Queue<MyTask>();
            if (!threadEnable)
            {
                threadEnable = true;
                if (myThread != null)
                {
                    Release();
                }
                myThread = new Thread(new ThreadStart(RUN));
                myThread.Start();
            }
        }
        public void Release()
        {
            threadEnable = false;
            autoEvent.Set();
            myThread = null;
        }
        public void RUN()
        {
            threadRunning = true;
            while (threadEnable)
            {
                //CALL MAIN FUNCITON
                while (taskQueue.Count != 0)
                {
                    if (!threadEnable) break;
                    var task = taskQueue.Dequeue();
                    RunTask(task);
                }

                autoEvent.WaitOne();
                if (!threadEnable) break;
            }
            threadRunning = false;
        }
        public virtual void RunTask(MyTask task)
        {
            if (task == MyTask.None) return;

        }
        public void SetNextTask(MyTask task, bool start = true)
        {
            taskQueue.Enqueue(task);
            if (start)
                autoEvent.Set();
        }

        #endregion

        #region Network
        // Network
        public bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
        public void ReadCallback(IAsyncResult ar)
        {

            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    content = state.sb.ToString();
                    var endpos = content.IndexOf("<EOF>");
                    if (endpos> -1)
                    {
                        state.sb.Remove(endpos, 5);//remove <EOF>
                        ReceiveFinsh(state);
                        state.Clear();
                    }

                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }

            }
            catch (Exception e)
            {
                //SocketError err=e.SocketErrorCode;
                Console.WriteLine(e.ToString());
                try
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
                catch (Exception)
                {
                    
                }
                SetEvent(DSEvent.Error);
            }
        }
        public virtual void ReceiveFinsh(StateObject state)
        {
            var content = state.sb.ToString();
            Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                   content.Length, content);
        }
        public void SendPackages(Socket handler)
        {
            if (!packageNeedToSend.isEmpty())
            {
                var str = packageNeedToSend.SerializeJson();
                Send(handler, str);
                packageNeedToSend.Reset();
            }
        }
        public virtual bool Send(Socket handler, string data)
        {
            try
            {

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

                SetEvent(DSEvent.ClientSend);
                return true;
            } catch(Exception)
            {
                SetEvent(DSEvent.Error);
                return false;
            }
        }
        public virtual void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes.", bytesSent);
 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Utility
        public string GetLocalIP()
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
        public bool CheckEvent(double time,DSEvent successful,DSEvent failure)
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
        public void AddMsg(string msg)
        {
            if (MsgQueue.Count > 20) MsgQueue.Dequeue();
            MsgQueue.Enqueue(msg);
        }
        #endregion

    }
}
