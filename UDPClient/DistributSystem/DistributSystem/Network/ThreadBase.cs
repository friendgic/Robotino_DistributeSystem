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
        public bool threadRunning;
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
            threadEnable = false;
            autoEvent.Set();
            myThread = null;
        }
        protected void RUN()
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
            SetEvent(DSEvent.Released);
        }
        protected virtual void RunTask(MyTask task)
        {
            if (task == MyTask.None) return;

        }
        protected void SetNextTask(MyTask task, bool start = true)
        {
            taskQueue.Enqueue(task);
            if (start)
                autoEvent.Set();
        }

        #endregion


        #region Utility

        public void AddMsg(string msg)
        {
            if (MsgQueue.Count > MsgQueueLength) MsgQueue.Dequeue();
            MsgQueue.Enqueue(msg);

        }
        public DSEvent GetEvent()
        {
            if (EventQueue.Count > 0)
            {
                return EventQueue.Dequeue();
            }
            return DSEvent.None;
        }


        public void SetEvent(DSEvent eve, string msg = " ")
        {
            if (EventQueue == null)
                EventQueue = new Queue<DSEvent>();
            EventQueue.Enqueue(eve);
            string str = "[" + eve.ToString() + "] " + msg;
            AddMsg(str);
            Console.WriteLine(str);
            if (eve == DSEvent.Error) throw new Exception();
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
