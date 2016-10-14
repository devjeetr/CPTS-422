using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreadPool
{
    /// <summary>
    /// Used to create an indepenent Thread Pool
    /// </summary>
    class MyThreadPool
    {

        #region Fields and Properties

        // Internal Value for the MinThreads
        private int m_MinThreads=0;
        /// <summary>
        /// Gets or Sets the MinThreads 
        /// </summary>
        public int MinThreads
        {
            get { return this.m_MinThreads; }
            set { this.m_MinThreads = value; }
        }

        // Internal Value for the MaxThreads
        private int m_MaxThreads=100;
        /// <summary>
        /// Gets or Sets the MaxThreads 
        /// </summary>
        public int MaxThreads
        {
            get { return this.m_MaxThreads; }
            set { this.m_MaxThreads = value; }
        }

        // Internal Value for the IdleTimeThreshold
        private int m_IdleTimeThreshold=5;
        /// <summary>
        /// Gets or Sets the IdleTimeThreshold 
        /// </summary>
        public int IdleTimeThreshold
        {
            get { return this.m_IdleTimeThreshold; }
            set { this.m_IdleTimeThreshold = value; }
        }

        //Stores the list of Work Queue
        private Queue<WorkItem> WorkQueue;

        //Returns the length of the queue
        public int QueueLength
        {
            get
            {
                return WorkQueue.Count();
            }
        }

        //Stores the list of Threads
        private List<WorkThread> ThreadList;

        // Performs management of the other threads in the thread pool
        private Thread ManagementThread;
        private bool KeepManagementThreadRunning = true;
        #endregion

        #region Constructor and Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MyThreadPool()
        {
            ManagementThread = new Thread(new ThreadStart(ManagementWorker));
            ManagementThread.Start();

            WorkQueue = new Queue<WorkItem>();
            ThreadList = new List<WorkThread>();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MyThreadPool()
        {
            //Stop the Management thread
            KeepManagementThreadRunning = false;
            if (ManagementThread != null)
            {
                if (ManagementThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    ManagementThread.Interrupt();
                }
                ManagementThread.Join();
            }

            //Stop each of the threads
            foreach (WorkThread t in ThreadList)
            {
                t.ShutDown();
            }
            ThreadList.Clear();

            //Empty the Work Queue
            WorkQueue.Clear();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Used to add work to the queue.
        /// </summary>
        /// <param name="WorkObject">The work object.</param>
        /// <param name="Delegate">The delegate.</param>
        public void QueueWork(object WorkObject, WorkDelegate Delegate)
        {
            WorkItem wi = new WorkItem();

            wi.WorkObject = WorkObject;
            wi.Delegate = Delegate;
            lock (WorkQueue)
            {
                WorkQueue.Enqueue(wi);
            }
            
            //Now see if there are any threads that are idle
            bool FoundIdleThread = false;
            foreach (WorkThread wt in ThreadList)
            {
                if (!wt.Busy)
                {
                    wt.WakeUp();
                    FoundIdleThread = true;
                    break;
                }
            }

            if (!FoundIdleThread)
            {
                //See if we can create a new thread to handle the additional workload
                if (ThreadList.Count < m_MaxThreads)
                {
                    WorkThread wt = new WorkThread(ref WorkQueue);
                    lock (ThreadList)
                    {
                        ThreadList.Add(wt);
                    }
                }
            }
        }

        /// <summary>
        /// Used to shutdown the thread pool
        /// </summary>
        public void Shutdown()
        {
            //Stop the Management thread
            KeepManagementThreadRunning = false;
            if (ManagementThread != null)
            {
                if (ManagementThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    ManagementThread.Interrupt();
                }
                ManagementThread.Join();
            }
            ManagementThread = null;

            //Stop each of the threads
            foreach (WorkThread t in ThreadList)
            {
                t.ShutDown();
            }
            ThreadList.Clear();

            //Empty the Work Queue
            WorkQueue.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Worker Management Process used to manage the threads in the thread pool
        /// </summary>
        private void ManagementWorker()
        {
            while (KeepManagementThreadRunning)
            {
                try
                {
                    //Check to see if we have idle thread we should free up
                    if (ThreadList.Count > m_MinThreads)
                    {
                        foreach (WorkThread wt in ThreadList)
                        {
                            if (DateTime.Now.Subtract(wt.LastOperation).Seconds > m_IdleTimeThreshold)
                            {
                                wt.ShutDown();
                                lock (ThreadList)
                                {
                                    ThreadList.Remove(wt);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    Thread.Sleep(1000);
                }
                catch { }
            }
        }

        #endregion
    }
}
