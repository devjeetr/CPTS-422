// Devjeet Roy
// 11404808


using System;
using System.IO;

namespace CS422
{
	public class NoSeekMemoryStream : MemoryStream
	{
		public NoSeekMemoryStream(byte[] buffer): base(buffer){
		} // implement
		public NoSeekMemoryStream(byte[] buffer, int offset, int count): base(buffer, offset, count){
		} // implement



		// Override necessary properties and methods to ensure that this stream functions
		// just like the MemoryStream class, but throws a NotSupportedException when seeking
		// is attempted (you'll have to override more than just the Seek function!)
		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public void SetLength(int length){
			throw new NotSupportedException ();
		}


		public override long Seek (long offset, SeekOrigin loc)
		{
			
			throw new NotSupportedException ();
		}
	}	

}

