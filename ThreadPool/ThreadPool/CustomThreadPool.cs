using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
	public class CustomThreadPool
	{
		public CustomThreadPool ()
		{
		}

		private int m_MinThreads=0;
		public int MinThreads
		{
			get
			{
				return this.m_MinThreads;
			}
			set
			{
				this.m_MinThreads = value;
			}
		}

		private int m_MaxThreads=100;
		public int MaxThreads
		{
			get
			{
				return this.m_MaxThreads;
			}
			set
			{
				this.m_MaxThreads = value;
			}
		}

		private int m_IdleTimeThreshold=5;
		public int IdleTimeThreshold
		{
			get
			{
				return this.m_IdleTimeThreshold;
			}
			set
			{
				this.m_IdleTimeThreshold = value;
			}
		}

		private Queue<WorkItem> WorkQueue;
		public int QueueLength
		{
			get
			{
				return WorkQueue.Count();
			}
		}

		private List<WorkThread> ThreadList;
		private Thread ManagementThread;
		private bool KeepManagementThreadRunning = true;


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
					FoundIdleThread = true; break;
				}
			}

			if (!FoundIdleThread)
			{
				//See if we can create a new thread to handle the
				//additional workload
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
							if (DateTime.Now.Subtract(wt.LastOperation).Seconds
								> m_IdleTimeThreshold)
							{
								wt.ShutDown();
								lock (ThreadList)
								{
									ThreadList.Remove(wt); break;
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

	}
}

