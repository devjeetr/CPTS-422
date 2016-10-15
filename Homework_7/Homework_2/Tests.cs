using System;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace CS422
{	
	//[TestFixture()]
	public class Tests
	{	
		
		public Tests ()
		{
			
		}

		public static void Main(){
			WebServer.AddService (new DemoService ());
			WebServer.Start (8080, 64);


		}
		class context{
			internal int processNumber;

		};

		public static void ThreadWorkTest(object obj){
			context currentContext = obj as context;
			//Console.WriteLine ("Inside thread {0}", currentContext.processNumber);
		}

		[Test()]
		public void TestThreadPool(){
			SimpleLockThreadPool pool = new SimpleLockThreadPool (64);
			context curr = new context();

				for (int i = 0; i < 10; i++) {
					curr = new context();
					curr.processNumber = i;
					pool.QueueUserWorkItem (ThreadWorkTest, curr);
				}

			pool.Dispose ();


		}



		public void WebServer_OnSlowRequest_TimesOut(){
			
		}	
	}
}

