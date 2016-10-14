/*
 * Devjeet Roy
 * Student ID: 11404808
 * 
 * */
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace CS422
{
	class WebServer
	{
		private const string STUDENT_ID = "11404808";
		private const string CRLF = @"\r\n";
		private const string DEFAULT_TEMPLATE =
			"HTTP/1.1 200 OK\r\n" +
			"Content-Type: text/html\r\n" +
			"\r\n\r\n" +
			"<html>ID Number: {0}<br>" +
			"DateTime.Now: {1}<br>" +
			"Requested URL: {2}</html>";
		private const string URL_REGEX_PATTERN = @"(\/.*).*";
		private const string HTTP_VERSION = "HTTP/1.1";
		private const string HEADER_REGEX_PATTERN = @"(.*):(.*)";
		private const string GET_REQUEST_STRING = "GET";

		private static Thread listenerThread;
		private volatile static TcpListener Listener;
		private static List<WebService> services = new List<WebService> ();
		private volatile static int processCount = 0;
		private static volatile bool stopped = false;
		private static SimpleLockThreadPool threadPool;

		public static bool Start(int port, int nThreads = 64)
		{
			
			threadPool = new SimpleLockThreadPool (nThreads);

			Listener = new TcpListener (System.Net.IPAddress.Any, port);
			listenerThread = new Thread(ListenProc);
			Listener.Start ();

			listenerThread.Start();
		
			return true;
		}

		private static void ListenProc(){
			try{
				
				while(true){
					if(Listener == null)
						break;
					
					TcpClient client = Listener.AcceptTcpClient ();
					Console.WriteLine ("Client accepted!");

					threadPool.QueueUserWorkItem(ThreadWork, client);

				}
			}
			catch(SocketException e) {
				Console.WriteLine ("SocketException: {0}", e);
			}
		}

		public static void Stop(){
			
			Terminate ();
		}


		private static WebRequest BuildRequest(TcpClient client){
			WebRequest newWebRequest = new WebRequest (client.GetStream());
			ConcatStream bodyStream = null;
			Console.WriteLine("Building Request...");
			List<byte> bufferedRequest = new List<byte>();
			bool done = false;
			int length = -1;

			while(!done){

				NetworkStream networkStream = client.GetStream ();

				int available = client.Available;

				while(client.Available > 0)
				{ 
					byte[] buf = new byte[client.Available];
					networkStream.Read(buf,0,client.Available);
					bufferedRequest.AddRange(buf);

					string request  = System.Text.ASCIIEncoding.UTF8.GetString(bufferedRequest.ToArray());

					if(!isValidRequest(bufferedRequest)){
						client.Close ();
						//listener.Stop ();

						return null;
					}

					if (request.Length >= 4 && request.Contains (@"\r\n\r\n")) {
						Console.WriteLine ("breaking inner");
						done = true;
						break;
					} 
				}
				var reqString  = System.Text.ASCIIEncoding.UTF8.GetString(bufferedRequest.ToArray());
				if (length == -1 && reqString.Split (new String[] { CRLF}, 
					StringSplitOptions.None).Length > 2) {
					//check for content length
					var headers = parseHeaders(reqString);
					if (headers.ContainsKey ("Content-Length")) {
						
						length = int.Parse(headers ["Content-Length"]);

					}
				}

				if (done) {
					// find part of body that has been read already
					string reqStr = System.Text.ASCIIEncoding.UTF8.GetString (bufferedRequest.ToArray ());

					int index = reqStr.IndexOf(@"\r\n\r\n");
					var alreadyReadBody = reqStr.Substring (index + @"\r\n\r\n".Count());
					Console.WriteLine ("Already: {0}", alreadyReadBody);
					MemoryStream already = new MemoryStream (System.Text.ASCIIEncoding.UTF8.GetBytes(alreadyReadBody));
					if (length != -1) {
						bodyStream = new ConcatStream (already, networkStream, length);		
					} else {
						bodyStream = new ConcatStream (already, networkStream);	
					}
				}
			}

			// request has been buffered, now build it
			newWebRequest.Method = "GET";
			string requestString = System.Text.ASCIIEncoding.UTF8.GetString(bufferedRequest.ToArray());
			newWebRequest.Body = bodyStream;
			string[] firstLine = requestString.Split (@"\r\n".ToCharArray()) [0].Split(' ');



			newWebRequest.HTTPVersion = firstLine[2];
			newWebRequest.RequestTarget = firstLine [1];
			newWebRequest.Headers = parseHeaders (requestString);

			return newWebRequest;

		}

		static ConcurrentDictionary<string, string> parseHeaders(string request){
			String[] requestLines = request.Split(new String[] { CRLF}, 
				StringSplitOptions.None);

			ConcurrentDictionary<string, string> headers = new ConcurrentDictionary<string, string>();
			//Console.WriteLine ("Searching for headers");
			for(int i = 1; i < requestLines.Length; i++){
				if(requestLines[i]  != ""){
					Regex r = new Regex(HEADER_REGEX_PATTERN);

					Match match = r.Match(requestLines[i]);
					string header = match.Groups [1].Value;
					string value = match.Groups [2].Value;
					if (!headers.TryAdd (header, value))
						Console.WriteLine ("Failed to add");

				}	
			}


			return headers;

		}

		static void Terminate(){
			Console.WriteLine ("terminating");
			listenerThread.Abort ();

			if(Listener != null)
				Listener.Stop ();
			Listener = null;
			threadPool.Dispose ();

			Thread.CurrentThread.Abort ();
		}

		static void ThreadWork(object clientObj){
			TcpClient client = clientObj as TcpClient;
			WebRequest request = BuildRequest (client);
			processCount++;
			Console.WriteLine ("process: {0}", processCount);
			Console.WriteLine ("Request built");
			if (request == null) {
				client.Close ();
				Console.WriteLine("Closing connection");
				processCount--;
				if (processCount == 0 && stopped)
					Terminate ();
				Console.WriteLine ("process: {0}", processCount);

				return;
			}

			var handlerService = fetchHandler (request);

			if (handlerService == null) {
				Console.WriteLine ("Handler not found, writing not found response");
				request.WriteNotFoundResponse ("NoasdasdasdasdtFound");
			} else {
				Console.WriteLine ("Handler found, delegating response");
				handlerService.Handler (request);
			}
			Console.WriteLine ("process: {0}", processCount);
			processCount--;
			if (processCount <= 0 && stopped)
				Terminate ();
			Thread.CurrentThread.Abort ();
		}


		public static void AddService(WebService service){

			services.Add (service);
		}

		private static WebService fetchHandler(WebRequest request){
			
			foreach (var service in services) {
				if (request.RequestTarget != null && request.RequestTarget.StartsWith (service.ServiceURI))
					return service;
			}

			return null;
		}

		private static bool isValidRequest(List<byte> requestBytes){

			String request = System.Text.ASCIIEncoding.UTF8.GetString (requestBytes.ToArray());

			String[] requestLines = request.Split(new String[] { CRLF}, 
												StringSplitOptions.None);

			// DEBUG

			// process first line for request
			if (requestLines.Length >= 1) {
				if (!processFirstLine (requestLines [0]))
					return false;
			}

			// process headers
			if (requestLines.Length >= 2) {
			
				for(int i = 1; i < requestLines.Length; i++){
				
					if(requestLines[i]  != ""){
						Regex r = new Regex(HEADER_REGEX_PATTERN);

						if ((!r.IsMatch (requestLines [i])) && 
							(i < requestLines.Length - 1 
								&& requestLines[i+1] == "")) {
							return false;
						}
					}	
				}
			}

			return true;
		}


		private static bool processFirstLine(String line){

			String[] tokens = line.Trim().Split (new char[]{ ' ' }, StringSplitOptions.None);

			if (tokens.Length > 3) {
				return false;
			}
			if (!GET_REQUEST_STRING.Contains (tokens [0]))
				return false;
			if (tokens.Length == 2) {
				Regex r = new Regex (URL_REGEX_PATTERN);

				if (!r.IsMatch (tokens [1]))
					return false;
			}
			if (tokens.Length == 3) {
				if (!HTTP_VERSION.Contains (tokens [2]) && !tokens[2].Contains(HTTP_VERSION)) {
					return false;
				}
				
			}
			return true;
		}
	}
}
	

