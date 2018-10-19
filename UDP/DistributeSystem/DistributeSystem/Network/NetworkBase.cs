﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool threadRunning=false;
        public Queue<MyTask> taskQueue;
        #endregion

        #region Thread

        ~NetworkBase()
        {
            Release();
        }
        public NetworkBase()
        {
        }
         
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
    }
}
