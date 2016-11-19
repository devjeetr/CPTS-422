

## ConcatStream
### Constructors:
* When the ConcatStream is constructed (using either constructor) its position must be at 0,
 regardless of the position of the first stream when it was passed to the constructor. 

* If the second stream does not support seeking, assume it’s at position 0 when passed into the constructor. 
	* It should have
	 been implied from requirements listed prior, but it’s worth pointing out that you’ll need a member
	 variable in the ConcatStream class to keep track of the position within the stream.


### Writing:
* The writing functionality will be such that it **never expands the length of the first stream, but it
can overwrite existing contents in that stream**. 

	As an example, suppose you have 2 writable streams, the
	first of which is 20 bytes in size and currently at position 10 (meaning the ConcatStream’s position is also
	at 10), and you write 30 bytes from a buffer. Since there are only 10 bytes left in the first stream and you
	cannot expand that stream, you write the first 10 of 30 bytes to it. Then you write the remaining 20 to
	the second stream. 

* The ConcatStream must also be able to expand provided the following two things
are true:
	1. The second of the two streams supports expanding
	2. The 2-parameter constructor was used to instantiate the ConcatStream (i.e. the stream is not
	fixed length due to the use of the 3-parameter constructor)
* **Do NOT let the stream expand if the 3-parameter constructor was used,*even if the second of the two
streams supports expanding**.

* If a write call cannot be completed then throw an exception, but **only do so if the action cannot
possibly be completed**. For example, if your ConcatStream position is 32 and the first stream has a
length of 20, then you could only write correctly to the second stream if its current position was exactly
12. In the case when the second stream supports seeking, it’s easy enough to ensure that, so just seek to
the appropriate spot and write in those circumstances. But if the second stream doesn’t support
seeking, then only complete the write if you are at the exact correct position. Otherwise throw an
exception

### Seek:
`public override long Position`

`public override long Seek(long offset, SeekOrigin origin)`

* Your stream must support seeking properly through use of both the Position property and Seek
function. 

* Make sure your unit tests include use of both of these. 

* You may assume that the position will
not be set to a negative value, but that’s the only assumption you may make. 

* You need to read the
documentation to determine how to properly implement all other cases of seeking.

### Length

`public override long Length`

* Your stream must support querying length in all cases where it can be accurately determined. 
	* If
	you construct a ConcatStream with the 3-parmeter constructor, it is of fixed length and the length
	passe dto that constructor must always be returned by the Length property for that instance. 

	* If the 2-
	parameter constructor was used, then you must return the combined length of the two streams. 

	* Should
	either one throw an exception when you query its length in this case, it’s fine to let that exception
	bubble-up to outside of that function