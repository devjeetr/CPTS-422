using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CS422
{
	class FilesWebService: WebService
	{
		//  JPEG, PNG, PDF, MP4, TXT, HTML and XML files

		private string[] CONTENT_TYPES = new string[]{"image/jpeg", "image/png", "application/pdf", "application/mp4", "text/plain", "text/html", "application/xml"};
		private string[][] CONTENT_FILE_TYPES = new string[][]{new string[]{".jpg", ".jpeg"}, new string[]{".png"}, new string[]{".pdf"}, new string[]{".mp4"}, 
			new string[]{".txt"}, new string[]{".html"}, new string[]{".xml"}};
		private const string RESPONSE_FORMAT = 
							@"<html>
									 <h1>Folders</h1>
									 
									 {0}
									 <h1>Files</h1>
											{1}
									 <br>
							</html>";

		private const string RESPONSE_ENTRY_FORMAT = 
			@"<a href='{0}'>{1}</a>
									 <br>";

		private FileSys422 fileSystem;

		public FilesWebService (FileSys422 fs)
		{
			fileSystem = fs;
		}


		Dir422 getParentDir(string path){
			var dirStructure = path.Split('/');

			if (dirStructure.Length == 1 && dirStructure [0].Length == 0)
				return fileSystem.GetRoot ();

			Console.WriteLine (dirStructure [0]);
			var root = fileSystem.GetRoot ();

			int i = 0;
			for(i = 0; i < dirStructure.Length - 1; i++) {
				if (!root.ContainsDir (dirStructure[i], false))
					return null;
				else 
					root = root.GetDir(dirStructure[i]);
			}


				return root;
		}

		public override void Handler(WebRequest req)
		{
			// TODO maybe change this
			var url = req.RequestTarget.Substring("/files/".Length);

			if (url.Length == 0) {
				Console.WriteLine ("Root requested");
				req.WriteHTMLResponse (BuildDirHTML (fileSystem.GetRoot()));
				return;
			}

			if (url.EndsWith ("/"))
				url = url.Remove (url.Length - 1, 1);
			
			var dir = getParentDir (url);

			Console.WriteLine (url);

			var tokens = url.Split ('/');
			var fileOrFolderName = tokens [tokens.Length - 1];

			if (dir == null) {
				Console.WriteLine ("Not found null");
				return;
			}else if (dir.ContainsFile (fileOrFolderName, false)) {
				Console.WriteLine ("file");

				SendFile (dir.GetFile (fileOrFolderName), req);


			}else if(dir.ContainsDir(fileOrFolderName, false)){
				Console.WriteLine ("folder: {0}, parent: {1}", fileOrFolderName, dir.GetDir (fileOrFolderName).Name);
				req.Headers = new System.Collections.Concurrent.ConcurrentDictionary<string, string> ();
				req.WriteHTMLResponse (BuildDirHTML (dir.GetDir (fileOrFolderName)));
			}
			else{
				req.WriteNotFoundResponse(@"<h1>File not found</h1>");
			}	
			Console.WriteLine ("done");
         }

		string getPath(Dir422 directory){
			string path = "";
			var x = directory;
			if (x.Parent == null)
				return "/files/";
			
			while (x != null && x.Parent != null) {
				path = x.Name + @"/" + path;
				x = x.Parent;
			}
			
			return "/files/" + path;

		}
		string BuildDirHTML(Dir422 directory){
			
			var root = directory.Name + "/";
			var files = directory.GetFiles ();
			var dirs = directory.GetDirs ();
			Console.WriteLine ("inside build {0} dirs, {1} files",dirs.Count, files.Count);

			var dirStr = "";
			var path = getPath (directory);
			Console.WriteLine ("-------------------------");
			Console.WriteLine (path);

			foreach (var dir in dirs) {
				dirStr += String.Format (RESPONSE_ENTRY_FORMAT, path + dir.Name, dir.Name);
			}
			var fileStr = "";
			foreach (var file in files) {
				fileStr += String.Format (RESPONSE_ENTRY_FORMAT, path + file.Name, file.Name);
			}

			return String.Format(RESPONSE_FORMAT, dirStr, fileStr);
		}


		public override string ServiceURI
		{
			get{
				return "/files";
			}
		}


		void SendFile(File422 file, WebRequest req){

			// check if request contains range header
			if (req.Headers.ContainsKey ("Range")) {
				var range = req.Headers ["Range"];
				Match match = Regex.Match (range, @".*bytes=([0-9]+)-([0-9]+).*");
			

				long start = long.Parse(match.Groups [1].Value);

				long end = 0;

				// if end match not found then value remains 0
				if(match.Groups.Count > 2)
					end = long.Parse(match.Groups [2].Value);

				SendFileRange (file, start, end, 500, req);
				//
			} else {
				//replace old headers
				SendFileRange (file, 0, 0, 500, req);
			}
		}

		void SendFileRange(File422 file, long start, long end, long chunkSize, WebRequest req){

			if (end < start)
				throw new ArgumentException ("Invalid start and end range");

			var fileStream = file.OpenReadOnly ();

			long fileSize = fileStream.Length;

			if (start > end)
				req.WriteNotFoundResponse ("Invalid Range Header Specified");
			
			if(end == 0){
				end = fileSize;
			}
			req.Headers = new System.Collections.Concurrent.ConcurrentDictionary<string, string> ();

			if (end - start + 1 <  chunkSize) {
				// only need to send 1 response
				string contentType = GetContentType(file.Name);
				if (contentType != null)
					req.Headers ["Content-Type"] = contentType;
				else
					req.Headers ["Content-Type"] = "text/plain";
				var fileContents = GetFileRange (fileStream, start, end);
				req.WriteResponse ("206 Partial Content", fileContents);


			}else{
				
				// need to send multiple responses
				string boundary = "5187ab27335732";

				string contentType = GetContentType(file.Name);
				if (contentType != null)
					req.Headers ["Content-Type"] = contentType;
				

				long offset = start;
				long sent = 0;
				req.Headers ["Accept-Ranges"] = "bytes";

				long sizeToSend = end - start + 1;

				while (sent <= sizeToSend) {
					long currentSize = (sizeToSend - sent) < chunkSize ? sizeToSend - sent: chunkSize;

					if (offset + currentSize > fileSize)
						currentSize = fileSize - offset + 1;


					if (currentSize <= 0)
						break;
					//Console.WriteLine ("Getting file range [{0}, {1}]", offset, offset + currentSize);
					var fileContents = GetFileRange (fileStream, offset, offset + currentSize);

					req.Headers ["Content-Range"] = String.Format ("bytes {0}-{1}/{2}", offset, currentSize + offset, sizeToSend);


					req.WriteResponse ("206 PartialContent", fileContents);

					offset += currentSize + 1;
					sent += currentSize;
				}
			}




		}

		string GetContentType(string fileName){
			//Console.WriteLine ("Name: {0}", fileName);
			string extension = Path.GetExtension (fileName);
			//Console.WriteLine ("extension: {0}", extension);
			for (int i = 0; i < CONTENT_FILE_TYPES.Length; i++) {
				for (int j = 0; j < CONTENT_FILE_TYPES [i].Length; j++) {
					if (CONTENT_FILE_TYPES [i] [j] == extension) {
						return CONTENT_TYPES [i];
					}

					//Console.WriteLine ("ola" + CONTENT_FILE_TYPES [i] [j]);
				}
			}

			return null;
		}


		string GetFileRange(Stream fileStream, long start, long end){
			
			long size = end - start + 1;
			if (start + size >= fileStream.Length)
				size = fileStream.Length - start;

			Console.WriteLine ("Size: {0}", size);

			byte[] buf = new byte[size];

			Console.WriteLine ("Start: {0}", start);

			fileStream.Seek (start, SeekOrigin.Begin);
			fileStream.Read (buf, 0, Convert.ToInt32(size));

			return System.Text.Encoding.ASCII.GetString(buf);
		}


	}
}

