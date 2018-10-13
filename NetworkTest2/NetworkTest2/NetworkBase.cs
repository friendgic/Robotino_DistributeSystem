using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTest2
{
    public enum NetworkType
    {
        None,Server,Client
    }
    public enum MyTask
    {
        None,
        StartServer,
        WaitForAConnection
    }
    public class NetworkConfig
    {
        public NetworkType networkType=NetworkType.None;
        public string localIP = "";
        public string remoteIP = "";
        public int port = 11000;
    }
    public class NetworkBase
    {
        private Thread myThread;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        public bool threadEnable=false;
        public bool threadRunning;
        public Queue<MyTask> taskQueue;
        private object locker = new object();
        public NetworkConfig config;

         ~NetworkBase()
        {
            Release();
        }
        public NetworkBase()
        { 
        }
        public void Init(NetworkConfig config=null)
        {
            this.config = config;
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
            while (threadRunning)
            {
                threadEnable = false;
                autoEvent.Set();
            }
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
                    var task=taskQueue.Dequeue();
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
        public void SetNextTask(MyTask task,bool start=true)
        {
            taskQueue.Enqueue(task);
            if(start)
            autoEvent.Set();
        }
    }
}
