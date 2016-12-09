using System;
using System.Collections.Concurrent;

namespace CS422
{
	internal class DemoService: WebService
	{
		private const string RESPONSE_TEMPLATE =
			"<html>This is the response to the request:<br>" +
			"Method: {0}<br>Request-Target/URI: {1}<br>" +
			"Request body size, in bytes: {2}<br><br>" +
			"Student ID: {3}</html>";
		
		public DemoService()
		{
			
		}

		public override void Handler(WebRequest req){
			req.Headers = new ConcurrentDictionary<string, string>();
			req.Headers["Content-Length"] = String.Format("{0}", RESPONSE_TEMPLATE.Length + "11404808".Length);
			req.WriteHTMLResponse(String.Format(RESPONSE_TEMPLATE, req.Method, req.RequestTarget, RESPONSE_TEMPLATE.Length + "11404808".Length, "11404808"));
		}

		public override string ServiceURI {
			get{ 
				return "/";
			}
		} 
	}
}

