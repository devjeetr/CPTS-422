using System;
using NUnit.Framework;
using CS422;

namespace CS422
{
	[TestFixture()]
	public class NoSeekStreamTests
	{	const string TEST_STRING = "asdasdasdasdasdASDASDJKHAKHJKASHDJKFHAJKSDHFJKASLDFH";
			
		[Test()]
		public void NetworkStream_ConstructedWithBuffer_StoresBytes(){

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING);

			NoSeekMemoryStream stream = new NoSeekMemoryStream (bytes);

			byte[] buffer = new byte[bytes.Length];

			stream.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (bytes, buffer);
		}

		[Test()]
		[Ignore]
		public void NetworkStream_ConstructedWithBufferAndOffsetAndLength_StoresBytes(){

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING);

			NoSeekMemoryStream stream = new NoSeekMemoryStream (bytes , 10, 5);

			bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING.Substring(11, 5));
			byte[] buffer = new byte[bytes.Length];

			stream.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (bytes, buffer);
		}

		/*
		 * NoSeekMemoryStream	unit	test	that	attempts	to	seek	using	all	relevant	properties/methods	
		 *	that	provide	seeking	capabilities	in	a	stream,	and	makes	sure	that	each	throws	the	
		 *	NotSupportedException
		 * */
		[Test()]
		[ExpectedException( typeof(NotSupportedException) )]
		public void NetworkStream_AccessSeek_ExceptionThrown(){

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING);

			NoSeekMemoryStream stream = new NoSeekMemoryStream (bytes , 10, 5);

			stream.Seek (12, System.IO.SeekOrigin.Begin);
		}

		[Test()]
		[ExpectedException( typeof(NotSupportedException) )]
		public void NetworkStream_AccessLength_ExceptionThrown(){

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING);

			NoSeekMemoryStream stream = new NoSeekMemoryStream (bytes , 10, 5);

			var x = stream.Length;
		}

		[Test()]
		public void NetworkStream_QueryCanSeek_ReturnsFalse(){

			byte[] bytes = System.Text.Encoding.Unicode.GetBytes (TEST_STRING);

			NoSeekMemoryStream stream = new NoSeekMemoryStream (bytes , 10, 5);

			Assert.AreEqual (false, stream.CanSeek);
		}
	}
}

