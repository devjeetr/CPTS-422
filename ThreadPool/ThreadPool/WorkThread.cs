using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreadPool
{
    /// <summary>
    /// Defines the Worker Thread used for the Thread Pool
    /// </summary>
    class WorkThread
    {
        #region Fields and Properties
        /// <summary>
        /// Variable storing the actual worker thread
        /// </summary>
        private Thread m_WorkProcess = null;

        /// <summary>
        /// Boolean variable used to determine if the thread should continue running
        /// </summary>
        private bool m_KeepRunning = true;

        /// <summary>
        /// Used to provide notification of the current status of the thread process
        /// </summary>
        private bool m_Busy = false;
        public bool Busy
        {
            get
            {
                return m_Busy;
            }
        }

        /// <summary>
        /// Used to determine the last operation performed by this thread
        /// </summary>
        DateTime m_LastOperation = DateTime.Now;
        public DateTime LastOperation
        {
            get
            {
                return m_LastOperation;
            }
        }

        /// <summary>
        /// Copy of the overall Work Queue
        /// </summary>
        private Queue<WorkItem> m_WorkQueue;

        #endregion

        #region Constructor and Destructor
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkThread"/> class.
        /// </summary>
        public WorkThread(ref Queue<WorkItem> WorkQueue)
        {
            m_WorkQueue = WorkQueue;
            m_WorkProcess = new Thread(new ThreadStart(Worker));
            m_WorkProcess.Start();
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="WorkThread"/> is reclaimed by garbage collection.
        /// </summary>
        ~WorkThread()
        {
            if (m_WorkProcess != null)
            {
                m_Busy = false;
                m_KeepRunning = false;
                if (m_WorkProcess.ThreadState == ThreadState.WaitSleepJoin)
                {
                    m_WorkProcess.Interrupt();
                }
                m_WorkProcess.Join();
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Instructs the thread to perform work on the Work Item.
        /// </summary>
        public void WakeUp()
        {
            if (m_WorkProcess.ThreadState == ThreadState.WaitSleepJoin)
            {
                m_WorkProcess.Interrupt();
            }
            m_Busy = true;
        }

        /// <summary>
        /// Used to Shutdown the worker thread
        /// </summary>
        public void ShutDown()
        {
            m_KeepRunning = false;
            if (m_WorkProcess.ThreadState == ThreadState.WaitSleepJoin)
            {
                m_WorkProcess.Interrupt();
            }
            m_WorkProcess.Join();
            m_WorkProcess = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Actual Worker process thread
        /// </summary>
        private void Worker()
        {
            WorkItem wi;

            while (m_KeepRunning)
            {
                try
                {
                    while (m_WorkQueue.Count > 0)
                    {
                        wi = null;

                        lock (m_WorkQueue)
                        {
                            wi = m_WorkQueue.Dequeue();
                        }

                        if (wi != null)
                        {
                            m_LastOperation = DateTime.Now;
                            m_Busy = true;
                            wi.Delegate.Invoke(wi.WorkObject);
                        }
                    }

                }
                catch { }

                try
                {
                    m_Busy = false;
                    Thread.Sleep(1000);
                }
                catch { }
            }
        }

        #endregion
    }
}
