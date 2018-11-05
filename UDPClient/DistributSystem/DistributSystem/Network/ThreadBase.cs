using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DistributeSystem
{
    public class ThreadBase
    {
        #region Init

        private Thread myThread;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        public bool threadEnable = false; 
        public Queue<MyTask> taskQueue=new Queue<MyTask>();
        public Queue<DSEvent> EventQueue = new Queue<DSEvent>();
        public Queue<string> MsgQueue = new Queue<string>();
        private object locker = new object();
        public int MsgQueueLength = 15;
        #endregion

        #region Thread

        ~ThreadBase()
        {
            Release();
        }
        public ThreadBase()
        {
        }

        protected void ThreadInit()
        {
            taskQueue = new Queue<MyTask>();
            EventQueue = new Queue<DSEvent>();
            MsgQueue = new Queue<string>();
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
        protected void Release()
        {
            lock (locker)
            {
                threadEnable = false;
                autoEvent.Set();
                myThread = null;
            }
        }
        protected void RUN()
        {
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
            SetEvent(DSEvent.Released,"Program stop");
        }
        protected virtual void RunTask(MyTask task)
        {
            if (task == MyTask.None) return;

        }
        protected void SetNextTask(MyTask task, bool start = true)
        {
            lock (locker)
            {

            taskQueue.Enqueue(task);
            if (start)
                autoEvent.Set();
            }
        }

        #endregion

        #region Utility

        private void AddMsg(string msg)
        {
            lock (locker)
            {

            if (MsgQueue.Count > MsgQueueLength) MsgQueue.Dequeue();
            MsgQueue.Enqueue(msg);
            }
        }
        public string ReadADebugMsg()
        {
            string msg=string.Empty;
            lock (locker)
            {
                if(MsgQueue.Count>0)
                   msg= MsgQueue.Dequeue();
            }
            return msg;
        }
        public DSEvent GetEvent()
        {
            lock (locker)
            {

            if (EventQueue.Count > 0)
            {
                return EventQueue.Dequeue();
            }
            }
            return DSEvent.None;
        }


        public void SetEvent(DSEvent eve, string msg = " " , bool quit=false)
        {
            lock (locker)
            {
                if (EventQueue == null)
                    EventQueue = new Queue<DSEvent>();
                EventQueue.Enqueue(eve);
                string str = "[" + eve.ToString() + "] " + msg + "\n";
                if (msg != " ") {

                    AddMsg(str);
                    if(Configure.debug)Console.WriteLine(str);
                }
            }
            if(quit)
            if (eve == DSEvent.Error) throw new Exception();
        }
        public bool CheckEvent(double time, DSEvent successful, DSEvent failure)
        {
            lock (locker)
            {

            if (!threadEnable)
            {
                return false;
            }
            }
            DateTime t = DateTime.Now;
            while ((DateTime.Now - t).TotalMilliseconds < time)
            {
                int resulttest = 0;
                lock (locker)
                {
                    for (int i = 0; i < EventQueue.Count; i++)
                    {
                        var item = EventQueue.Dequeue();
                        if (item != DSEvent.None)
                        {
                            if (item == successful)
                            {
                                resulttest = 1;
                            }
                            if (item == failure)
                            {
                                resulttest = 2;
                            }
                        }
                    }
                }
                if (resulttest == 1)
                {
                    return true;
                }
                if (resulttest == 2)
                {
                    return false;
                }
                Thread.Sleep(10);
            }
            SetEvent(DSEvent.Error, "Time out");
            return false;
        }
        #endregion 
    }
}
