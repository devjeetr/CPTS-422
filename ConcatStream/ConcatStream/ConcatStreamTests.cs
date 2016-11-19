// Devjeet Roy
// 11404808

using CS422;
using NUnit.Framework;
using System.IO;
using System;
using System.Net.Sockets;
using System.Linq;

namespace CS422{
	[TestFixture()]
	class ConcatStreamUnitTests{
		private string TEST_STRING_A = RandomString (200);
		private string TEST_STRING_B = RandomString (5000);
		private static Random r = new Random();


		/////////////////////////////////////////////////////////////////////
		///
		/// 					stream.Read() Tests
		/// 
		////////////////////////////////////////////////////////////////////
		[Test()]
		public void TestReadBasic(){
			Stream a = new MemoryStream(), b = new MemoryStream();
			string toWrite = "Asdasd";

			byte[] buffer = System.Text.Encoding.Unicode.GetBytes (toWrite);

			a.Write (buffer, 0, buffer.Length);
			buffer = System.Text.Encoding.Unicode.GetBytes ("Secondsasdasd");
			b.Write (buffer, 0, buffer.Length);

			a.Seek (0, SeekOrigin.Begin);
			b.Seek (0, SeekOrigin.Begin);


			ConcatStream concatStream = new ConcatStream (a, b);

			Assert.AreEqual (true, concatStream.CanRead);
			Assert.AreEqual (true, concatStream.CanWrite);
			Assert.AreEqual (true, concatStream.CanSeek);

			Assert.AreEqual (0, concatStream.Position);

			buffer = new byte[a.Length + b.Length];
			concatStream.Read (buffer, 0, Convert.ToInt32(a.Length + b.Length));

			string actual = System.Text.Encoding.Unicode.GetString (buffer);

			Assert.AreEqual (a.Length + b.Length, concatStream.Position);
			Assert.AreEqual ("AsdasdSecondsasdasd", actual);

		}


		[Test()]
		public void ConcatStream_SecondStreamCannotSeek_CanSeekSetToFalse(){
			NoSeekMemoryStream noSeekStream = new NoSeekMemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (seekStream, noSeekStream);

			Assert.AreEqual (false, concatStream.CanSeek);
		}

		[Test()]
		public void ConcatStream_BothStreamsCanSeek_CanSeekSetToTrue(){
			MemoryStream noSeekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (noSeekStream, seekStream);

			Assert.AreEqual (true, concatStream.CanSeek);
		}


		[Test()]
		public void ConcatStream_OneStreamCannotWrite_CanWriteSetToFalse(){
			MemoryStream noSeekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A), false);
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (noSeekStream, seekStream);

			Assert.AreEqual (false, concatStream.CanWrite);
		}

		[Test()]
		public void ConcatStream_OneStreamCannotWrite_CanWriteSetToTrue(){
			MemoryStream noSeekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (noSeekStream, seekStream);

			Assert.AreEqual (true, concatStream.CanWrite);
		}




		// Test Read

		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void Read_LengthGreaterThanConcatStreamLength_ExceptionThrown(){

			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));
			byte[] buffer = new byte[seekStream.Length + 30]; 
			int n = seekStream.Read (buffer, 0, Convert.ToInt32(seekStream.Length + 90));

			Assert.AreEqual (seekStream.Length, n);
		}

		/*
		 * ConcatStream	unit	test	that	combines	two	memory	streams,	reads	back	all	the	data	in	random
		 * chunk	sizes,	and	verifies	it	against	the	original	data
		 * */
		[Test()]
		public void Read_FragmentedReading_BytesReadInCorrectOrder(){


			MemoryStream a = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream b = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));
			ConcatStream c = new ConcatStream (a, b);

			byte[] expected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A + TEST_STRING_B);
			byte[] actual = new byte[a.Length + b.Length];

			int count = Convert.ToInt32(a.Length + b.Length);
			int offset = 0;

			while (count > 0) {
				int randCount = r.Next (0, count + 1);

				if (r.Next (1, count + 1) % 2 == 0) {
					// change both streams position to messs wwith concatstream
					a.Position = r.Next (0, count + 1);
					b.Position = r.Next (0, count + 1);
				} else {
					a.Seek (r.Next (0, count + 1), SeekOrigin.Current);
					b.Seek (r.Next (0, count + 1), SeekOrigin.Current);
				}

				int bytesRead = c.Read (actual, offset, randCount);
				offset += bytesRead;
				count -= randCount;
			}

			Assert.AreEqual (expected, actual);
		}

		/*
		 * ConcatStream	unit	test	that	combines	a	memory	stream	as	the	first	and	a	
			NoSeekMemoryStream	as	the	second,	and	verifies	that	all	data	can	be	read
		 * */
		[Test()]
		public void Read_NoSeekAndMemoryStreamConcat_DataIsPreserved(){


			MemoryStream a = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			byte[] bBuf = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_B);
			NoSeekMemoryStream b = new NoSeekMemoryStream (bBuf);
			ConcatStream c = new ConcatStream (a, b);

			byte[] expected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A + TEST_STRING_B);
			byte[] actual = new byte[a.Length + bBuf.Length];

			int count = Convert.ToInt32(a.Length + bBuf.Length);
			int offset = 0;

			while (count > 0) {
				int randCount = r.Next (0, count + 1);
				int bytesRead = c.Read (actual, offset, randCount);
				offset += bytesRead;
				count -= randCount;
			}

			Assert.AreEqual (expected, actual);
		}


		/**
		* ConcatStream	tests that	query	Length	property	in	both	circumstances	where	it	can(1)	and	cannot	
		*	be	computed	without	exception(2).
		* 
		* */
		// --> 1
		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void ConcatStream_SecondStreamDoesNotSupportLength_ExceptionThrown(){
			NoSeekMemoryStream networkStream = new NoSeekMemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (networkStream, seekStream);
			var x = concatStream.Length;
		}
		// --> 2
		[Test()]
		public void ConcatStream_BothStreamsSupportLength_ExceptionThrown(){
			MemoryStream networkStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			MemoryStream seekStream = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_B));

			ConcatStream concatStream = new ConcatStream (networkStream, seekStream);
			// test that it does not throw eception
			Assert.DoesNotThrow(() => { var x = concatStream.Length;});
		}


		/////////////////////////////////////////////////////////////////////
		///
		/// 					stream.Write() Tests
		/// 
		////////////////////////////////////////////////////////////////////
		[Test()]
		public void Write_FragmentedWriting_BytesReadInCorrectOrder(){


			MemoryStream a = new MemoryStream ();
			MemoryStream b = new MemoryStream ();
			ConcatStream c = new ConcatStream (a, b);
			string test = "";

			for (int i = 0; i < 10; i++) {
				test += TEST_STRING_B;
			}


			byte[] expected = System.Text.Encoding.Unicode.GetBytes (test);
			byte[] actual = new byte[expected.Length];

			int count = Convert.ToInt32(expected.Length);
			int offset = 0;

			while (count > 0) {

				int randCount = r.Next (0, count + 1);
				c.Write (expected, offset, randCount);
				offset += randCount;
				count -= randCount;
				Assert.AreEqual (offset, c.Position);
			}

			c.Seek (0, SeekOrigin.Begin);
			//c.Position = 0;
			c.Read (actual, 0, expected.Length);
			Console.WriteLine ("actual: " + System.Text.Encoding.Unicode.GetString (actual));
			Assert.AreEqual (expected, actual);
		}


		[Test()]
		public void SetLength_TestWrite_DataIsCorrectlyWritten(){
			const int MAX_STRING_LENGTH = int.MaxValue / 100;

			String randomString = RandomString (0);
			byte[] completeBuf = System.Text.Encoding.Unicode.GetBytes (randomString);                                                                                           

			// now create two streams
			int strALength = r.Next(0, randomString.Length);

			String strA = randomString.Substring (0, strALength);
			byte[] aBuf = System.Text.Encoding.Unicode.GetBytes (strA);

			String strB = randomString.Substring (strALength);
			byte[] bBuf = System.Text.Encoding.Unicode.GetBytes (strB);


			Stream A = new MemoryStream (0);
			Stream B = new MemoryStream (aBuf);

			ConcatStream concat = new ConcatStream (A, B);

			concat.Seek (aBuf.Length, SeekOrigin.Begin);

			int bytesToWrite = bBuf.Length;


			while (bytesToWrite > 0) {
				int nBytesToWrite = r.Next (0, bytesToWrite + 1);

				concat.Write (bBuf, bBuf.Length - bytesToWrite, nBytesToWrite);
				bytesToWrite -= nBytesToWrite;
			}

			concat.Seek (0, SeekOrigin.Begin);
			byte[] actual = new byte[concat.Length];

			int nBytesRead = concat.Read (actual, 0, actual.Length);


			Assert.AreEqual (completeBuf.Length, nBytesRead);
			Assert.AreEqual (completeBuf, actual);

		}

		[Test()]
		[Ignore()]
		public void Write_TestWriteOnOffset_DataIsCorrectlyWritten(){
			const int MAX_STRING_LENGTH = int.MaxValue / 100;

			String randomString = RandomString (10000 + MAX_STRING_LENGTH);
			byte[] completeBuf = System.Text.Encoding.Unicode.GetBytes (randomString);                                                                                           

			// now create two streams
			int strALength = r.Next(0, randomString.Length);

			string strBase = randomString.Substring (0, 10000);
			byte[] baseBuf = System.Text.Encoding.Unicode.GetBytes (strBase);

			String strA = randomString.Substring (10000, strALength);
			byte[] aBuf = System.Text.Encoding.Unicode.GetBytes (strA);

			String strB = randomString.Substring (strALength);
			byte[] bBuf = System.Text.Encoding.Unicode.GetBytes (strB);


			Stream A = new MemoryStream (baseBuf);
			Stream B = new MemoryStream (aBuf);

			ConcatStream concat = new ConcatStream (A, B);

			concat.Seek (aBuf.Length, SeekOrigin.Begin);

			int bytesToWrite = bBuf.Length;


			while (bytesToWrite > 0) {
				int nBytesToWrite = r.Next (0, bytesToWrite + 1);

				concat.Write (bBuf, bBuf.Length - bytesToWrite, nBytesToWrite);
				bytesToWrite -= nBytesToWrite;
			}

			concat.Seek (0, SeekOrigin.Begin);
			byte[] actual = new byte[concat.Length];

			int nBytesRead = concat.Read (actual, 0, actual.Length);


			Assert.AreEqual (completeBuf.Length, nBytesRead);
			Assert.AreEqual (completeBuf, actual);

		}

		// this test can take some time, so uncomment
		// the following line if you need
		// [Ignore()]
		[Test()]
		public void Write_OnMultipleFragmentedWritesWithFirstStreamOfZeroLength_DataIsSavedProperly(){
			string test = new string('x', 20000);
			MemoryStream a = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(test));
			MemoryStream b = new MemoryStream ();
			ConcatStream c = new ConcatStream (a, b);


			byte[] expected = System.Text.Encoding.Unicode.GetBytes (test);
			byte[] actual = new byte[expected.Length];

			uint count = 0;
			while (count < int.MaxValue) {

				c.Write (expected, 0, expected.Length);
				count += Convert.ToUInt32(expected.Length);
				Assert.AreEqual (count, c.Position);

			}

			c.Seek (0, SeekOrigin.Begin);
			c.Read (actual, 0, expected.Length);
			Assert.AreEqual (expected, actual);
		}

		[Test()]
		public void Write_OnOverWriteBoth_StreamBehavesProperly(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);
			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b);

			byte[] actual = new byte[a.Length];
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (actual, aInitialExpected);

			stream.Seek (0, SeekOrigin.Begin);

			Assert.AreEqual (0, stream.Position);

			byte[] aFinalExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_B);

			stream.Write (aFinalExpected, 0, aFinalExpected.Length);



			Assert.AreEqual (aFinalExpected.Length, stream.Position);

			actual = new byte[aFinalExpected.Length];

			stream.Seek (0, SeekOrigin.Begin);
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (aFinalExpected, actual);
		}

		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void Write_OnOverWriteWithFixedLength_ExceptionThrown(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);

			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b, aInitialExpected.Length);

			byte[] actual = new byte[a.Length];
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (actual, aInitialExpected);

			stream.Seek (0, SeekOrigin.Begin);

			Assert.AreEqual (0, stream.Position);

			byte[] aFinalExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_B);

			stream.Write (aFinalExpected, 0, aFinalExpected.Length);



			Assert.AreEqual (aFinalExpected.Length, stream.Position);

			actual = new byte[aFinalExpected.Length];

			stream.Seek (0, SeekOrigin.Begin);
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (aFinalExpected, actual);
		}


		/////////////////////////////////////////////////////////////////////
		///
		/// 					Seek Tests
		/// 
		////////////////////////////////////////////////////////////////////
		[Test()]
		public void Seek_OnSeek_DifferentOffsetsWork(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);

			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b, aInitialExpected.Length);

			stream.Seek (0, SeekOrigin.End);
			Assert.AreEqual (stream.Length, stream.Position);

			stream.Seek (-1 * stream.Length, SeekOrigin.Current);
			Assert.AreEqual (0, stream.Position);


			stream.Seek (stream.Length, SeekOrigin.Begin);
			Assert.AreEqual (stream.Length, stream.Position);
		}

		[Test()]
		public void Seek_OnSeekBeyondLengthPositive_PositionIsNotTruncated(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);

			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b, aInitialExpected.Length);

			int position = Convert.ToInt32(stream.Length) + r.Next (0, 2000);

			stream.Seek (position, SeekOrigin.Begin);
			Assert.AreEqual(position, stream.Position);
		}

		[Test()]
		public void Seek_OnSeekBeyondLengthNegative_PositionIsTruncated(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);

			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b, aInitialExpected.Length);

			stream.Seek (-20, SeekOrigin.Begin);
			Assert.AreEqual (0, stream.Position);
		}


		/////////////////////////////////////////////////////////////////////
		///
		/// 					Set Length Tests
		/// 
		////////////////////////////////////////////////////////////////////
		[Test()]
		public void SetLength_WhenLengthLessThanFirstStreamLength_StreamIsTruncated(){
			byte[] aInitialExpected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A);
			byte[] aFinalExpected = System.Text.Encoding.Unicode.GetBytes (
				TEST_STRING_A.Substring(0, TEST_STRING_A.Length / 2));


			MemoryStream a = new MemoryStream (aInitialExpected);
			MemoryStream b = new MemoryStream ();

			ConcatStream stream = new ConcatStream (a, b, aInitialExpected.Length);
			int streamLength = Convert.ToInt32(stream.Length);

			stream.SetLength (streamLength / 2);

			Assert.AreEqual (a.Length, streamLength / 2);

			stream.Seek (0, SeekOrigin.Begin);
			byte[] aFinalActual = new byte[stream.Length];
			int bytesRead = stream.Read (aFinalActual, 0, Convert.ToInt32(stream.Length));
			Assert.AreEqual (aFinalExpected.Length, bytesRead);
			Assert.AreEqual (aFinalExpected, aFinalActual);
		}


		// This test may take a while
		// depending on number of test cases
		[Test()]
		[Ignore()]
		public void SetLength_OnSetLengthAndWrite_DataIsCorrectlyWritten(){
			const int N_TEST_CASES = 1;
			const int MAX_STRING_LENGTH = int.MaxValue / 100;

			for (int i = 0; i < N_TEST_CASES; i++) {
				Console.WriteLine ("SetLength_OnSetLengthAndWrite_DataIsCorrectlyWritten i = {0}", i);

				String randomString = RandomString (r.Next (0, MAX_STRING_LENGTH));
				byte[] completeBuf = System.Text.Encoding.Unicode.GetBytes (randomString);                                                                                           

				// now create two streams
				int strALength = r.Next(0, randomString.Length);

				String strA = randomString.Substring (0, strALength);
				byte[] aBuf = System.Text.Encoding.Unicode.GetBytes (strA);

				String strB = randomString.Substring (strALength);
				byte[] bBuf = System.Text.Encoding.Unicode.GetBytes (strB);


				Stream A = new MemoryStream (aBuf);
				Stream B = new MemoryStream ();

				ConcatStream concat = new ConcatStream (A, B);

				concat.Seek (A.Length, SeekOrigin.Begin);

				int bytesToWrite = bBuf.Length;


				while (bytesToWrite > 0) {
					int nBytesToWrite = r.Next (0, bytesToWrite + 1);

					if (r.Next (1, int.MaxValue) % 2 == 0) {
						// change both streams position to messs wwith concatstream
						A.Position = r.Next (0, int.MaxValue);
						B.Position = r.Next (0, int.MaxValue);
					} else {
						A.Seek (r.Next (0, int.MaxValue), SeekOrigin.Begin);
						B.Seek (r.Next (0, int.MaxValue), SeekOrigin.Begin);
					}

					concat.Write (bBuf, bBuf.Length - bytesToWrite, nBytesToWrite);
					bytesToWrite -= nBytesToWrite;
				}

				concat.Seek (0, SeekOrigin.Begin);

				byte[] actual = new byte[concat.Length];

				if (r.Next (1, int.MaxValue) % 2 == 0) {
					// change both streams position to messs wwith concatstream
					A.Position = r.Next (0, int.MaxValue);
					B.Position = r.Next (0, int.MaxValue);
				} else {
					A.Seek (r.Next (0, int.MaxValue), SeekOrigin.Current);
					B.Seek (r.Next (0, int.MaxValue), SeekOrigin.Current);
				}

				int nBytesRead = concat.Read (actual, 0, actual.Length);


				Assert.AreEqual (completeBuf.Length, nBytesRead);
				Assert.AreEqual (completeBuf, actual);

			}
		}



		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[r.Next(s.Length)]).ToArray());
		}



		// Expand Tests
		[Test()]
		public void Write_ExpandsProperly(){
			String a = RandomString (1000);
			byte[] aBytes = bytes(a);


			MemoryStream First = new MemoryStream (aBytes);
			MemoryStream Second = new MemoryStream ();

			ConcatStream C = new ConcatStream (First, Second);

			First.Seek (20, SeekOrigin.End);
			Second.Seek (22, SeekOrigin.Begin);

			C.Write (aBytes, 0, aBytes.Length);

			Assert.AreEqual (aBytes.Length, C.Length);


			C.Seek(aBytes.Length - 1, SeekOrigin.Begin);

			String strToWrite = RandomString (1000);
			byte[] bytesToWrite = bytes (strToWrite);


		}

		// Expand Tests
		[Test()]
		public void Write_DoesNotExpandWhenSecondStreamIsNotExpandable(){
			String a = RandomString (1000);
			byte[] aBytes = bytes(a);

			String b = RandomString (1000);
			byte[] bBytes = bytes(b);

			String c = RandomString (4000);
			byte[] cBytes = bytes(c);

			MemoryStream First = new MemoryStream (aBytes);
			MemoryStream Second = new MemoryStream (bBytes);

			ConcatStream C = new ConcatStream (First, Second);

			// double original buffer and try to expand
			byte[] bytesToWrite = bytes(c);

			C.Write (bytesToWrite, 0, bytesToWrite.Length);

			byte[] actual = new byte[C.Length];
			C.Seek (0, SeekOrigin.Begin);
			C.Read (actual, 0, actual.Length);

			Assert.AreEqual (aBytes.Length + bBytes.Length, C.Length);
			Assert.AreEqual (bytes(c.Substring(0, 2000)), actual);
		}

		[Test()]
		public void Write_DoesExpandWhenSecondStreamIsExpandable(){
			String a = RandomString (1000);
			byte[] aBytes = bytes(a);

			String b = RandomString (1000);
			//byte[] bBytes = bytes();

			String c = RandomString (4000);
			byte[] cBytes = bytes(c);

			MemoryStream First = new MemoryStream (aBytes);
			MemoryStream Second = new MemoryStream ();

			ConcatStream C = new ConcatStream (First, Second);

			// double original buffer and try to expand
			byte[] bytesToWrite = bytes(c);

			C.Write (bytesToWrite, 0, bytesToWrite.Length);

			byte[] actual = new byte[C.Length];
			C.Seek (0, SeekOrigin.Begin);
			C.Read (actual, 0, actual.Length);

			Assert.AreEqual (cBytes.Length, C.Length);
			Assert.AreEqual (cBytes, actual);
		}



		public static byte[] bytes(string s){
			return System.Text.Encoding.Unicode.GetBytes (s);
		}
	}
}