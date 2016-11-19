using System;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace CS422
{
	class MainClass
	{
		static Random r = new Random();

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[r.Next(s.Length)]).ToArray());
		}
		public static void Main (string[] args)
		{
			int nTestCases = 1;

			for (int i = 0; i < nTestCases; i++) {
				Console.WriteLine ("SetLength_OnSetLengthAndWrite_DataIsCorrectlyWritten i = {0}", i);
				String randomString = RandomString (r.Next (0, int.MaxValue / 50));
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

					concat.Write (bBuf, bBuf.Length - bytesToWrite, nBytesToWrite);
					Console.WriteLine ("Writing {0}", bytesToWrite);
					bytesToWrite -= nBytesToWrite;
				}
				Console.WriteLine ("Done");

				concat.Seek (0, SeekOrigin.Begin);
				byte[] actual = new byte[concat.Length];

				int nBytesRead = concat.Read (actual, 0, actual.Length);
				Assert.AreEqual (completeBuf.Length, nBytesRead);

				Assert.AreEqual (completeBuf, actual);

			}
		}
	}
}
