using System;

namespace ThreadPool
{
	public class WorkerThread
	{
		public WorkerThread ()
		{
		}

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
	}
}

