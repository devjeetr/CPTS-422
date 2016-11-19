using System;
using System.IO;

namespace CS422
{
	public class ConcatStream: Stream{
		private Stream A;
		private Stream B;
		private bool fixedLength = false;
		private bool canRead, canWrite, canSeek, lengthSupported;
		private long position;
		private long length;

		public ConcatStream(Stream first, Stream second){
			if(first == null || second == null)
				throw new ArgumentException("null stream passed to ConcatStream(Stream, Stream)");
			Console.WriteLine(first.CanSeek);
			if(!first.CanSeek)
				throw new ArgumentException("Length property not found in first stream");

			if (first.CanSeek && second.CanSeek) {
				lengthSupported = true;
				length = first.Length + second.Length;
			} else {
				lengthSupported = false;
			}


			canSeek = first.CanSeek && second.CanSeek;
			canRead = first.CanRead && second.CanRead;
			canWrite = first.CanWrite && second.CanWrite;

			A = first;
			B = second;

			position = 0;

			fixedLength = false;
		}

		public ConcatStream(Stream first, Stream second, long fixedLen){
			if(first == null || second == null || fixedLen < 0)
				throw new ArgumentException("null stream passed to ConcatStream(Stream, Stream)");


			fixedLength = true;
			lengthSupported = true;

			position = 0;
			length = fixedLen;

			A = first;
			B = second;

			canSeek = first.CanSeek && second.CanSeek;
			canRead = first.CanRead && second.CanRead;
			canWrite = first.CanWrite && second.CanWrite;

		}

		// Properties

		public override long Position {
			get{ 
				return position;
			}

			set{ 
				Seek (value, SeekOrigin.Begin);
			}
		}

		public override void Flush(){
			throw new NotSupportedException ();
		}

		public override void SetLength(long len){
			if (!lengthSupported)
				throw new NotSupportedException ();

			Console.WriteLine ("A: {0}, B: {1}", A.Length, B.Length);

			if (len > A.Length && len <= B.Length) {
				int seekPosition = Convert.ToInt32 (len - A.Length);
				B.SetLength (seekPosition);
			} else {
				Console.WriteLine ("hi");
			}

			this.length = len;
			if (len != position)
				Position = len;
		}

		public override bool CanRead{
			get{
				return canRead;
			}
		}

		public override bool CanWrite{
			get{ 
				return canWrite;
			}
		}

		public override bool CanSeek{
			get{ 
				return canSeek;
			}
		}

		public override long Length{
			get{ 
				if (!lengthSupported)
					throw new NotSupportedException ();
				
				if (fixedLength)
					return length;
				else
					return A.Length + B.Length;
			}
		}

		// Methods
		// TODO
		// Add support for fixed length property
		public override int Read(byte[] buffer, int offset, int count)
		{	
			if (!canRead)
				throw new NotSupportedException ();

			if (buffer.Length < offset + count) {
				throw new ArgumentException ();
			}

			if(lengthSupported && count + Position > Length)
				throw new ArgumentException(String.Format("ConcatStream.Read: Invalid count, Position={0}, Length={1}, Count={2}",
					Position, Length, count));

			int totalBytesRead = 0;

			if (position < A.Length) {
				int available = Convert.ToInt32(A.Length - A.Position);
				int bytesToRead = count > available ? available : count;

				int bytesRead = A.Read(buffer, offset, bytesToRead);

				count -= bytesToRead;
				offset += bytesToRead;
				position += bytesRead;
				totalBytesRead += bytesRead;
			}


			if (count > 0) {
				int bytesRead = B.Read (buffer, offset, count);

				position += bytesRead;
				totalBytesRead += bytesRead;
			}
			return totalBytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count){
			if (!canWrite)
				throw new NotSupportedException ();

			if (buffer.Length < offset + count) {
				throw new ArgumentException ();
			}

		
			// If this stream is fixed length, then 
			// we must truncate 'count' to the length
			if (this.fixedLength && position + count > Length)
				throw new ArgumentException ("Expanding not supported on fixed length stream");

			// Position = A.Position  + B.Position here
			int bufferCounter = offset;

			if (Position < A.Length) {
				int bytesAvailable = Convert.ToInt32(A.Length - A.Position);

				// make sure we don't write more bytes than available
				int bytesToWrite = bytesAvailable < count ? bytesAvailable : count;

				A.Write(buffer, Convert.ToInt32(bufferCounter), Convert.ToInt32(bytesToWrite));

				// update counters
				count -= Convert.ToInt32(bytesToWrite);
				bufferCounter += bytesToWrite;
				position += bytesToWrite;

				Console.WriteLine ("Bytes Written: {0}, A.len: {1}, Position: {2}", bytesToWrite, A.Length, Position);
			}

			// TODO: 
			//	if Position > A.Length && A.Position + B.Position != Position
			//	and if B can't seek, then throw an exception
			//	else seek B to appropriate position
			if (count > 0) {
				if (position - A.Length != B.Position) {
					if (!B.CanSeek)
						throw new ArgumentException ("Unable to write to B");
					else {
						Console.WriteLine ("Position: {0}, A.len: {1}", position, A.Length);
						B.Seek (position - A.Length, SeekOrigin.Begin);
					}
				} 
				B.Write (buffer, Convert.ToInt32(bufferCounter), count);
				position += count;
				Console.WriteLine ("Bytes written to b: {0}, Position: {1}, byffer: {2}", count, position, bufferCounter);
			}

		}

		// TODO
		// add support for fixed length
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException ("Seek operation not supported by ConcatStream");
			
			switch (origin) {
			case SeekOrigin.Begin: 
				position = offset;
				break;
			case SeekOrigin.Current: 
				position += offset;
				break;
			case SeekOrigin.End: 
				Console.WriteLine ("Length: {0}, offset: {1}", Length, offset);
				position = Length + offset;
				Console.WriteLine ("Position: {0}", position);
				break;
			}

			// Now truncate position
			if (position > Length)
				position = Length;

			if (position < 0)
				position = 0;

			if (position < A.Length) {
				A.Seek (position, SeekOrigin.Begin);
				B.Seek (0, SeekOrigin.Begin);
			} else {
				if(A.Length > 0)
					A.Seek (A.Length - 1, SeekOrigin.Begin);

				long seekPosition = position - A.Length;

				if((seekPosition < B.Length))
					B.Seek (seekPosition, SeekOrigin.Begin);
			}

			return 0;	
		}
	}
}
