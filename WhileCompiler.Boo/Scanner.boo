#
#
#namespace WC
#
#import System
#import System.IO
#import System.Collections.Generic
#
#
#public class Token:
#
#	public kind as int
#
#	// token kind
#	public pos as int
#
#	// token position in the source text (starting at 0)
#	public col as int
#
#	// token column (starting at 1)
#	public line as int
#
#	// token line (starting at 1)
#	public val as string
#
#	// token value
#	public next as Token
#	// ML 2005-03-11 Tokens are kept in linked list
#
#
#//-----------------------------------------------------------------------------------
#// Buffer
#//-----------------------------------------------------------------------------------
#public class Buffer:
#
#	// This Buffer supports the following cases:
#	// 1) seekable stream (file)
#	//    a) whole stream in buffer
#	//    b) part of stream in buffer
#	// 2) non seekable stream (network, console)
#	
#	public static final EOF as int = (char.MaxValue + 1)
#
#	private static final MIN_BUFFER_LENGTH = 1024
#
#	// 1KB
#	private static final MAX_BUFFER_LENGTH as int = (MIN_BUFFER_LENGTH * 64)
#
#	// 64KB
#	private buf as (byte)
#
#	// input buffer
#	private bufStart as int
#
#	// position of first byte in buffer relative to input stream
#	private bufLen as int
#
#	// length of buffer
#	private fileLen as int
#
#	// length of input stream (may change if the stream is no file)
#	private bufPos as int
#
#	// current position in buffer
#	private stream as Stream
#
#	// input stream (seekable)
#	private isUserStream as bool
#
#	// was the stream opened by the user?
#	public def constructor(s as Stream, isUserStream as bool):
#		stream = s
#		self.isUserStream = isUserStream
#		
#		if stream.CanSeek:
#			fileLen = cast(int, stream.Length)
#			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH)
#			bufStart = Int32.MaxValue
#		else:
#			// nothing in the buffer so far
#			fileLen = (bufLen = (bufStart = 0))
#		
#		buf = array(byte, (bufLen if (bufLen > 0) else MIN_BUFFER_LENGTH))
#		if fileLen > 0:
#			Pos = 0
#		else:
#			// setup buffer to position 0 (start)
#			bufPos = 0
#		// index 0 is already after the file, thus Pos = 0 is invalid
#		if (bufLen == fileLen) and stream.CanSeek:
#			Close()
#
#	
#	protected def constructor(b as Buffer):
#		// called in UTF8Buffer constructor
#		buf = b.buf
#		bufStart = b.bufStart
#		bufLen = b.bufLen
#		fileLen = b.fileLen
#		bufPos = b.bufPos
#		stream = b.stream
#		// keep destructor from closing the stream
#		b.stream = null
#		isUserStream = b.isUserStream
#
#	def destructor():
#		
#		Close()
#
#	
#	protected def Close():
#		if (not isUserStream) and (stream is not null):
#			stream.Close()
#			stream = null
#
#	
#	public virtual def Read() as int:
#		if bufPos < bufLen:
#			return buf[(bufPos++)]
#		elif Pos < fileLen:
#			Pos = Pos
#			// shift buffer start to Pos
#			return buf[(bufPos++)]
#		elif ((stream is not null) and (not stream.CanSeek)) and (ReadNextStreamChunk() > 0):
#			return buf[(bufPos++)]
#		else:
#			return EOF
#
#	
#	public def Peek() as int:
#		curPos as int = Pos
#		ch as int = Read()
#		Pos = curPos
#		return ch
#
#	
#	public def GetString(beg as int, end as int) as string:
#		len as int = (end - beg)
#		buf as (char) = array(char, len)
#		oldPos as int = Pos
#		Pos = beg
#		for i in range(0, len):
#			buf[i] = cast(char, Read())
#		Pos = oldPos
#		return String(buf)
#
#	
#	public Pos as int:
#		get:
#			return (bufPos + bufStart)
#		set:
#			if ((value >= fileLen) and (stream is not null)) and (not stream.CanSeek):
#				// Wanted position is after buffer and the stream
#				// is not seek-able e.g. network or console,
#				// thus we have to read the stream manually till
#				// the wanted position is in sight.
#				while (value >= fileLen) and (ReadNextStreamChunk() > 0):
#					pass
#			
#			if (value < 0) or (value > fileLen):
#				raise FatalError(('buffer out of bounds access, position: ' + value))
#			
#			if (value >= bufStart) and (value < (bufStart + bufLen)):
#				// already in buffer
#				bufPos = (value - bufStart)
#			elif stream is not null:
#				// must be swapped in
#				stream.Seek(value, SeekOrigin.Begin)
#				bufLen = stream.Read(buf, 0, buf.Length)
#				bufStart = value
#				bufPos = 0
#			else:
#				// set the position to the end of the file, Pos will return fileLen.
#				bufPos = (fileLen - bufStart)
#
#	
#	// Read the next chunk of bytes from the stream, increases the buffer
#	// if needed and updates the fields fileLen and bufLen.
#	// Returns the number of bytes read.
#	private def ReadNextStreamChunk() as int:
#		free as int = (buf.Length - bufLen)
#		if free == 0:
#			// in the case of a growing input stream
#			// we can neither seek in the stream, nor can we
#			// foresee the maximum length, thus we must adapt
#			// the buffer size on demand.
#			newBuf as (byte) = array(byte, (bufLen * 2))
#			Array.Copy(buf, newBuf, bufLen)
#			buf = newBuf
#			free = bufLen
#		read as int = stream.Read(buf, bufLen, free)
#		if read > 0:
#			fileLen = (bufLen = (bufLen + read))
#			return read
#		// end of stream reached
#		return 0
#
#
#//-----------------------------------------------------------------------------------
#// UTF8Buffer
#//-----------------------------------------------------------------------------------
#public class UTF8Buffer(Buffer):
#
#	public def constructor(b as Buffer):
#		super(b)
#
#	
#	public override def Read() as int:
#		// until we find a uft8 start (0xxxxxxx or 11xxxxxx)
#		// nothing to do, first 127 chars are the same in ascii and utf8
#		// 0xxxxxxx or end of file character
#		// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
#		c2 as int
#		c1 as int
#		c3 as int
#		ch as int
#		while true:
#			ch = super.Read()
#			break  unless (((ch >= 128) and ((ch & 192) != 192)) and (ch != EOF))
#		if (ch < 128) or (ch == EOF):
#			pass
#		elif (ch & 240) == 240:
#			c1 = (ch & 7)
#			ch = super.Read()
#			c2 = (ch & 63)
#			ch = super.Read()
#			c3 = (ch & 63)
#			ch = super.Read()
#			c4 as int = (ch & 63)
#			ch = ((((((c1 << 6) | c2) << 6) | c3) << 6) | c4)
#		elif (ch & 224) == 224:
#			// 1110xxxx 10xxxxxx 10xxxxxx
#			c1 = (ch & 15)
#			ch = super.Read()
#			c2 = (ch & 63)
#			ch = super.Read()
#			c3 = (ch & 63)
#			ch = ((((c1 << 6) | c2) << 6) | c3)
#		elif (ch & 192) == 192:
#			// 110xxxxx 10xxxxxx
#			c1 = (ch & 31)
#			ch = super.Read()
#			c2 = (ch & 63)
#			ch = ((c1 << 6) | c2)
#		return ch
#
#
#//-----------------------------------------------------------------------------------
#// Scanner
#//-----------------------------------------------------------------------------------
#public class Scanner:
#
#	private static final EOL = char('\n')
#
#	private static final eofSym = 0
#
#	/* pdt */
#	private static final maxT = 41
#
#	private static final noSym = 41
#
#	
#	
#	public buffer as Buffer
#
#	// scanner buffer
#	private t as Token
#
#	// current token
#	private ch as int
#
#	// current input character
#	private pos as int
#
#	// byte position of current character
#	private col as int
#
#	// column number of current character
#	private line as int
#
#	// line number of current character
#	private oldEols as int
#
#	// EOLs that appeared in a comment;
#	private start as Dictionary[of int, int]
#
#	// maps first token character to start state
#	private tokens as Token
#
#	// list of tokens already peeked (first token is a dummy)
#	private pt as Token
#
#	// current peek token
#	private tval as (char) = array(char, 128)
#
#	// text of current token
#	private tlen as int
#
#	// length of current token
#	public def constructor(fileName as string):
#		try:
#			stream as Stream = FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
#			buffer = Buffer(stream, false)
#			Init()
#		except converterGeneratedName1 as IOException:
#			raise FatalError(('Cannot open file ' + fileName))
#
#	
#	public def constructor(s as Stream):
#		buffer = Buffer(s, true)
#		Init()
#
#	
#	private def Init():
#		pos = (-1)
#		line = 1
#		col = 0
#		oldEols = 0
#		NextCh()
#		if ch == 239:
#			// check optional byte order mark for UTF-8
#			NextCh()
#			ch1 as int = ch
#			NextCh()
#			ch2 as int = ch
#			if (ch1 != 187) or (ch2 != 191):
#				raise FatalError(String.Format('illegal byte order mark: EF {0,2:X} {1,2:X}', ch1, ch2))
#			buffer = UTF8Buffer(buffer)
#			col = 0
#			NextCh()
#		start = Dictionary[of int, int](128)
#		for i in range(65, 91):
#			start[i] = 1
#		for i in range(95, 96):
#			start[i] = 1
#		for i in range(97, 123):
#			start[i] = 1
#		for i in range(48, 58):
#			start[i] = 2
#		start[59] = 3
#		start[58] = 4
#		start[124] = 25
#		start[38] = 26
#		start[61] = 8
#		start[33] = 10
#		start[60] = 27
#		start[62] = 28
#		start[94] = 14
#		start[43] = 17
#		start[45] = 18
#		start[42] = 19
#		start[47] = 20
#		start[37] = 21
#		start[126] = 22
#		start[40] = 23
#		start[41] = 24
#		start[Buffer.EOF] = (-1)
#		
#		pt = (tokens = Token())
#		// first token is a dummy
#
#	
#	private def NextCh():
#		if oldEols > 0:
#			ch = EOL
#			oldEols -= 1
#		else:
#			pos = buffer.Pos
#			ch = buffer.Read()
#			col += 1
#			// replace isolated '\r' by '\n' in order to make
#			// eol handling uniform across Windows, Unix and Mac
#			if (ch == char('\r')) and (buffer.Peek() != char('\n')):
#				ch = EOL
#			if ch == EOL:
#				line += 1
#				col = 0
#		
#
#	
#	private def AddCh():
#		if tlen >= tval.Length:
#			newBuf as (char) = array(char, (2 * tval.Length))
#			Array.Copy(tval, 0, newBuf, 0, tval.Length)
#			tval = newBuf
#		tval[(tlen++)] = cast(char, ch)
#		NextCh()
#
#	
#	
#	
#	private def Comment0() as bool:
#		level = 1
#		pos0 as int = pos
#		line0 as int = line
#		col0 as int = col
#		NextCh()
#		goto converterGeneratedName2
#		while true:
#			:converterGeneratedName2
#			break  unless 
#			if ch == 10:
#				level -= 1
#				if level == 0:
#					oldEols = (line - line0)
#					NextCh()
#					return true
#				NextCh()
#			elif ch == Buffer.EOF:
#				return false
#			else:
#				NextCh()
#
#	
#	private def Comment1() as bool:
#		level = 1
#		pos0 as int = pos
#		line0 as int = line
#		col0 as int = col
#		NextCh()
#		if ch == char('/'):
#			NextCh()
#			goto converterGeneratedName3
#			while true:
#				:converterGeneratedName3
#				break  unless 
#				if ch == 10:
#					level -= 1
#					if level == 0:
#						oldEols = (line - line0)
#						NextCh()
#						return true
#					NextCh()
#				elif ch == Buffer.EOF:
#					return false
#				else:
#					NextCh()
#		else:
#			buffer.Pos = pos0
#			NextCh()
#			line = line0
#			col = col0
#		return false
#
#	
#	private def Comment2() as bool:
#		level = 1
#		pos0 as int = pos
#		line0 as int = line
#		col0 as int = col
#		NextCh()
#		if ch == char('*'):
#			NextCh()
#			goto converterGeneratedName4
#			while true:
#				:converterGeneratedName4
#				break  unless 
#				if ch == char('*'):
#					NextCh()
#					if ch == char('/'):
#						level -= 1
#						if level == 0:
#							oldEols = (line - line0)
#							NextCh()
#							return true
#						NextCh()
#				elif ch == char('/'):
#					NextCh()
#					if ch == char('*'):
#						level += 1
#						NextCh()
#				elif ch == Buffer.EOF:
#					return false
#				else:
#					NextCh()
#		else:
#			buffer.Pos = pos0
#			NextCh()
#			line = line0
#			col = col0
#		return false
#
#	
#	
#	private def CheckLiteral():
#		converterGeneratedName5 = t.val
#		if converterGeneratedName5 == 'skip':
#			t.kind = 4
#		elif converterGeneratedName5 == 'read':
#			t.kind = 5
#		elif converterGeneratedName5 == 'write':
#			t.kind = 6
#		elif converterGeneratedName5 == 'var':
#			t.kind = 7
#		elif converterGeneratedName5 == 'begin':
#			t.kind = 8
#		elif converterGeneratedName5 == 'end':
#			t.kind = 9
#		elif converterGeneratedName5 == 'if':
#			t.kind = 11
#		elif converterGeneratedName5 == 'then':
#			t.kind = 12
#		elif converterGeneratedName5 == 'else':
#			t.kind = 13
#		elif converterGeneratedName5 == 'fi':
#			t.kind = 14
#		elif converterGeneratedName5 == 'while':
#			t.kind = 15
#		elif converterGeneratedName5 == 'do':
#			t.kind = 16
#		elif converterGeneratedName5 == 'od':
#			t.kind = 17
#		elif converterGeneratedName5 == 'true':
#			t.kind = 37
#		elif converterGeneratedName5 == 'false':
#			t.kind = 38
#		else:
#			pass
#
#	
#	private def NextToken() as Token:
#		while ((ch == char(' ')) or ((ch >= 9) and (ch <= 10))) or (ch == 13):
#			NextCh()
#		if (((ch == char('#')) and Comment0()) or ((ch == char('/')) and Comment1())) or ((ch == char('/')) and Comment2()):
#			return NextToken()
#		t = Token()
#		t.pos = pos
#		t.col = col
#		t.line = line
#		state as int
#		try:
#			state = start[ch]
#		except converterGeneratedName6 as KeyNotFoundException:
#			state = 0
#		tlen = 0
#		AddCh()
#		converterGeneratedName7 = state
#		
#		if converterGeneratedName7 == (-1):
#			t.kind = eofSym
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 0:
#		// NextCh already done
#			t.kind = noSym
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 1:
#		// NextCh already done
#			:converterGeneratedName7_-842352753
#			if ((((ch >= char('0')) and (ch <= char('9'))) or ((ch >= char('A')) and (ch <= char('Z')))) or (ch == char('_'))) or ((ch >= char('a')) and (ch <= char('z'))):
#				AddCh()
#				goto converterGeneratedName7_-842352753
#			else:
#				t.kind = 1
#				t.val = String(tval, 0, tlen)
#				CheckLiteral()
#				return t
#		elif converterGeneratedName7 == 2:
#			:converterGeneratedName7_-842352754
#			if (ch >= char('0')) and (ch <= char('9')):
#				AddCh()
#				goto converterGeneratedName7_-842352754
#			else:
#				t.kind = 2
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 3:
#			t.kind = 3
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 4:
#			if ch == char('='):
#				AddCh()
#				goto converterGeneratedName7_-842352757
#			else:
#				t.kind = noSym
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 5:
#			:converterGeneratedName7_-842352757
#			t.kind = 10
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 6:
#			:converterGeneratedName7_-842352758
#			t.kind = 18
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 7:
#			:converterGeneratedName7_-842352759
#			t.kind = 19
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 8:
#			if ch == char('='):
#				AddCh()
#				goto converterGeneratedName7_-842352745
#			else:
#				t.kind = noSym
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 9:
#			:converterGeneratedName7_-842352745
#			t.kind = 20
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 10:
#			if ch == char('='):
#				AddCh()
#				goto converterGeneratedName7_-843466865
#			else:
#				t.kind = noSym
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 11:
#			:converterGeneratedName7_-843466865
#			t.kind = 21
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 12:
#			:converterGeneratedName7_-843532401
#			t.kind = 24
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 13:
#			:converterGeneratedName7_-843597937
#			t.kind = 25
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 14:
#			t.kind = 27
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 15:
#			:converterGeneratedName7_-843729009
#			t.kind = 29
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 16:
#			:converterGeneratedName7_-843794545
#			t.kind = 30
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 17:
#			t.kind = 31
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 18:
#			t.kind = 32
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 19:
#			t.kind = 33
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 20:
#			t.kind = 34
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 21:
#			t.kind = 35
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 22:
#			t.kind = 36
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 23:
#			t.kind = 39
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 24:
#			t.kind = 40
#			goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 25:
#			if ch == char('|'):
#				AddCh()
#				goto converterGeneratedName7_-842352758
#			else:
#				t.kind = 26
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 26:
#			if ch == char('&'):
#				AddCh()
#				goto converterGeneratedName7_-842352759
#			else:
#				t.kind = 28
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 27:
#			if ch == char('='):
#				AddCh()
#				goto converterGeneratedName7_-843532401
#			elif ch == char('<'):
#				AddCh()
#				goto converterGeneratedName7_-843729009
#			else:
#				t.kind = 22
#				goto converterGeneratedName7_end
#		elif converterGeneratedName7 == 28:
#			if ch == char('='):
#				AddCh()
#				goto converterGeneratedName7_-843597937
#			elif ch == char('>'):
#				AddCh()
#				goto converterGeneratedName7_-843794545
#			else:
#				t.kind = 23
#				goto converterGeneratedName7_end
#		:converterGeneratedName7_end
#		
#		t.val = String(tval, 0, tlen)
#		return t
#
#	
#	// get the next token (possibly a token already seen during peeking)
#	public def Scan() as Token:
#		if tokens.next is null:
#			return NextToken()
#		else:
#			pt = (tokens = tokens.next)
#			return tokens
#
#	
#	// peek for the next token, ignore pragmas
#	public def Peek() as Token:
#		if pt.next is null:
#			while true:
#				pt = (pt.next = NextToken())
#				break  unless (pt.kind > maxT)
#		else:
#			// skip pragmas
#			while true:
#				pt = pt.next
#				break  unless (pt.kind > maxT)
#		return pt
#
#	
#	// make sure that peeking starts at the current scan position
#	public def ResetPeek():
#		pt = tokens
#	
#
#// end Scanner
