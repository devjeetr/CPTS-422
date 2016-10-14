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

			Thread newThread = new Thread (stopServer);

			newThread.Start ();
		}
		static void stopServer(){
			var start = DateTime.Now ;
			var secondsElapsed = (DateTime.Now - start).TotalSeconds;

			while(((secondsElapsed = (DateTime.Now - start).TotalSeconds) <= 30)){

			}
			Console.WriteLine ("Stopping server");
			WebServer.Stop();
		}
		//[Test()]
		public void TestServer(){

		}

		public void WebServer_OnSlowRequest_TimesOut(){
			
		}	
	}
}

