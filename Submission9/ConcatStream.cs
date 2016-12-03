using System;
using System.IO;

namespace CS422
{
	public class ConcatStream: Stream{

		bool fixedLength = false;
		Stream A;
		Stream B;
		bool canRead, canWrite, canSeek, lengthSupported;
		long length;

		public ConcatStream(Stream first, Stream second){

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

			Position = 0;

			fixedLength = false;
		}

		public ConcatStream(Stream first, Stream second, long fixedLen){
			this.fixedLength = true;
			lengthSupported = true;

			Position = 0;
			length = fixedLen;

			A = first;
			B = second;

			canSeek = first.CanSeek && second.CanSeek;
			canRead = first.CanRead && second.CanRead;
			canWrite = first.CanWrite && second.CanWrite;

		}

		// Properties

		public override long Position {
			get;
			set;
		}

		public override void Flush(){
			throw new NotSupportedException ();
		}

		public override void SetLength(long length){
			if (!lengthSupported || fixedLength)
				throw new NotSupportedException ();
			
			this.length = length;
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

		public override int Read(byte[] buffer, int offset, int count)
		{	
			if (!canRead)
				throw new NotSupportedException ();

			if (buffer.Length < offset + count) {
				throw new ArgumentException ();
			}

			if(lengthSupported && count > Position + Length)
				throw new ArgumentException(String.Format("ConcatStream.Read: Invalid count, Position={0}, Length={1}, Count={2}",
					Position, Length, count));

			int totalBytesRead = 0;

			if (Position < A.Length) {
				int available = Convert.ToInt32(A.Length - A.Position);
				int bytesToRead = count > available ? available : count;

				int bytesRead = A.Read(buffer, offset, bytesToRead);

				count -= bytesToRead;
				offset += bytesToRead;
				Position += bytesRead;
				totalBytesRead += bytesRead;
			}


			if (count > 0) {
				int bytesRead = B.Read (buffer, offset, count);

				Position += bytesRead;
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

			// Position = A.Position  + B.Position here
			int bufferCounter = offset;
			if (Position < A.Length) {
				int bytesAvailable = Convert.ToInt32(A.Length - A.Position);

				int bytesToWrite = bytesAvailable < count ? bytesAvailable : count;



				A.Write(buffer, bufferCounter, bytesToWrite);
				count -= bytesToWrite;
				bufferCounter += bytesToWrite;
				Position += bytesToWrite;
				Console.WriteLine ("Bytes Written: {0}, A.len: {1}, Position: {2}", bytesToWrite, A.Length, Position);
			}

			// TODO: 
			//	if Position > A.Length && A.Position + B.Position != Position
			//	and if B can't seek, then throw an exception
			//	else seek B to appropriate position
			if (count > 0) {
				if (Position - A.Length != B.Position) {
					if (!B.CanSeek)
						throw new ArgumentException ("Unable to write to B");
					else {
						Console.WriteLine ("Position: {0}, A.len: {1}", Position, A.Length);
						B.Seek (Position - A.Length - 1, SeekOrigin.Begin);
					}
				} 
				B.Write (buffer, bufferCounter, count);
				Position += count;
				Console.WriteLine ("Bytes written to b: {0}, Position: {1}, byffer: {2}", count, Position, bufferCounter);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException ("Seek operation not supported by ConcatStream");
			
			switch (origin) {
			case SeekOrigin.Begin: 
				Position = offset;
				break;
			case SeekOrigin.Current: 
				Position += offset;
				break;
			case SeekOrigin.End: 
				Position = length - offset;
				break;
			}

			if (Position > Length || Position < 0)
				throw new ArgumentException ("Invalid Seek offset");

			if (Position < A.Length) {
				A.Seek (Position, SeekOrigin.Begin);
				B.Seek (0, SeekOrigin.Begin);
			} else {
				A.Seek (A.Length - 1, SeekOrigin.Begin);

				B.Seek (Position - A.Length, SeekOrigin.Begin);
			}

			return 0;	
		}
	}
}
