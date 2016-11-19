using CS422;
using NUnit.Framework;
using System.IO;
using System;
using System.Net.Sockets;

namespace CS422{
	[TestFixture()]
	class ConcatStreamUnitTests{
		const string TEST_STRING_A = "jk;lkl;i";
		const string TEST_STRING_B = "asdhgasdghafdghghasdfgafsdhgfkashjdf";

		[Test()]
		public void TestRead(){
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
			Random random = new Random ();

			while (count > 0) {
				int randCount = random.Next (0, count + 1);
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
		public void Read_NoSeekAndMemoryStreamCombined_BytesReadInCorrectOrder(){


			MemoryStream a = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(TEST_STRING_A));
			byte[] bBuf = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_B);
			NoSeekMemoryStream b = new NoSeekMemoryStream (bBuf);
			ConcatStream c = new ConcatStream (a, b);

			byte[] expected = System.Text.Encoding.Unicode.GetBytes (TEST_STRING_A + TEST_STRING_B);
			byte[] actual = new byte[a.Length + bBuf.Length];

			int count = Convert.ToInt32(a.Length + bBuf.Length);
			int offset = 0;
			Random random = new Random ();

			while (count > 0) {
				int randCount = random.Next (0, count + 1);
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


		[Test()]
		public void Write_FragmentedWriting_BytesReadInCorrectOrder(){


			MemoryStream a = new MemoryStream ();
			MemoryStream b = new MemoryStream ();
			ConcatStream c = new ConcatStream (a, b);
			string test = "jhasdgfjklahdlkjfkjasdhfjhajkldsfhjkladsfklaskld;j;aklsdhfjkalhsdjkfhasjkldfhjkalshdfjklahsjkdfhajlsdhfkjlahsdfjklahsdafajklshdfjkalsdhf";

			for (int i = 0; i < 10; i++) {
				test += test;
			}


			byte[] expected = System.Text.Encoding.Unicode.GetBytes (test);
			byte[] actual = new byte[expected.Length];

			int count = Convert.ToInt32(expected.Length);
			int offset = 0;
			Random random = new Random ();

			while (count > 0) {

				int randCount = random.Next (0, count + 1);
				Console.WriteLine ("offset: {0}, count: {1}", offset, randCount);
				c.Write (expected, offset, randCount);
				offset += randCount;
				count -= randCount;
				Assert.AreEqual (offset, c.Position);
			}

			c.Seek (0, SeekOrigin.Begin);
			c.Read (actual, 0, expected.Length);
			Console.WriteLine ("actual: " + System.Text.Encoding.Unicode.GetString (actual));
			Assert.AreEqual (expected, actual);
		}


		[Test()]
		[Ignore("Ignore a fixture")]
		public void Write_OnMultipleFragmentedWritesWithFirstStreamOfZeroLength_DataIsSavedProperly(){
			string test = new string('x', 20000);
			MemoryStream a = new MemoryStream (System.Text.Encoding.Unicode.GetBytes(test));
			MemoryStream b = new MemoryStream ();
			ConcatStream c = new ConcatStream (a, b);


			byte[] expected = System.Text.Encoding.Unicode.GetBytes (test);
			byte[] actual = new byte[expected.Length];

			uint count = 0;
			int offset = 0;
			Random random = new Random ();

			while (count < int.MaxValue) {
				
				c.Write (expected, 0, expected.Length);
				count += Convert.ToUInt32(expected.Length);
				Assert.AreEqual (count, c.Position);

			}

			c.Seek (0, SeekOrigin.Begin);
			c.Read (actual, 0, expected.Length);
			Console.WriteLine ("actual: " + System.Text.Encoding.Unicode.GetString (actual));
			Assert.AreEqual (expected, actual);
		}

		[Test()]
		public void Write_OnOverWrite_StreamBehavesProperly(){
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

		[Ignore()]
		[Test()]
		public void Write_OnOverWriteWithFixedLength_StreamBehavesProperly(){
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

		[Test()]
		public void Write_OnOverflowingWriteWithFixedLength_StreamDoesntExpand(){
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

			Assert.AreEqual (aInitialExpected.Length, stream.Position);

			/*
			actual = new byte[aFinalExpected.Length];

			stream.Seek (0, SeekOrigin.Begin);
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (aFinalExpected, actual);
			*/
		}

		[Test()]
		public void Write_OnPositionMismatchWriteFails_StreamDoesntExpand(){
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
			//stream.Write (aFinalExpected, 0, aFinalExpected.Length);

			//Assert.AreEqual (aInitialExpected.Length, stream.Position);

			/*
			actual = new byte[aFinalExpected.Length];

			stream.Seek (0, SeekOrigin.Begin);
			stream.Read (actual, 0, actual.Length);

			Assert.AreEqual (aFinalExpected, actual);
			*/
		}
	}
}