using System;
// using NUnit.Framework;
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
			StandardFileSystem a = StandardFileSystem.Create("/Users/devjeetroy");
			//Console.WriteLine (a.GetRoot ().GetFiles ()[0].Name);
			WebServer.AddService (new FilesWebService(a));
			WebServer.Start (8080, 64);

	}


	}
}

