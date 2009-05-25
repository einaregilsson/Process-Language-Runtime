
using System;
using System.IO;
using System.Collections;

namespace KLAIM.Parsing {

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public partial class Scanner {

	const int maxT = 24;
	const int noSym = 24;


	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 2;
		for (int i = 48; i <= 57; ++i) start[i] = 5;
		start[33] = 3; 
		start[124] = 21; 
		start[58] = 7; 
		start[60] = 9; 
		start[44] = 10; 
		start[62] = 11; 
		start[43] = 12; 
		start[46] = 13; 
		start[40] = 14; 
		start[41] = 15; 
		start[64] = 16; 
		start[45] = 17; 
		start[42] = 18; 
		start[47] = 19; 
		start[37] = 20; 
		start[Buffer.EOF] = -1;

	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read(); col++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}



	bool Comment0() {
		int level = 1, pos0 = pos, line0 = line, col0 = col;
		NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
	}


	void CheckLiteral() {
		switch (t.val) {
			case "nil": t.kind = 15; break;
			case "out": t.kind = 16; break;
			case "in": t.kind = 18; break;
			case "read": t.kind = 19; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();
		if (ch == '#' && Comment0()) return NextToken();
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; 
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: { t.kind = noSym; break; }   // NextCh already done
			case 1:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; break;}
			case 2:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 2;}
				else {t.kind = 2; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 3:
				if (ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = noSym; break;}
			case 4:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = 3; break;}
			case 5:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 5;}
				else {t.kind = 4; break;}
			case 6:
				{t.kind = 5; break;}
			case 7:
				if (ch == ':') {AddCh(); goto case 8;}
				else {t.kind = noSym; break;}
			case 8:
				{t.kind = 6; break;}
			case 9:
				{t.kind = 7; break;}
			case 10:
				{t.kind = 8; break;}
			case 11:
				{t.kind = 9; break;}
			case 12:
				{t.kind = 10; break;}
			case 13:
				{t.kind = 12; break;}
			case 14:
				{t.kind = 13; break;}
			case 15:
				{t.kind = 14; break;}
			case 16:
				{t.kind = 17; break;}
			case 17:
				{t.kind = 20; break;}
			case 18:
				{t.kind = 21; break;}
			case 19:
				{t.kind = 22; break;}
			case 20:
				{t.kind = 23; break;}
			case 21:
				if (ch == '|') {AddCh(); goto case 6;}
				else {t.kind = 11; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
} // end Scanner

}