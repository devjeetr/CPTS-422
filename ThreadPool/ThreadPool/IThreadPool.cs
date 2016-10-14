using System;
using System.Threading;

namespace CS422
{
	public interface IThreadPool: IDisposable
	{
		void QueueUserWorkItem(WaitCallback work, object obj);
	}
}

