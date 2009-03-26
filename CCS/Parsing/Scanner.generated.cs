
using System;
using System.IO;
using System.Collections;

namespace CCS.Parsing {

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public partial class Scanner {

	const int maxT = 26;
	const int noSym = 26;


	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 13;
		for (int i = 97; i <= 122; ++i) start[i] = 2;
		for (int i = 95; i <= 95; ++i) start[i] = 7;
		for (int i = 48; i <= 57; ++i) start[i] = 12;
		start[58] = 10; 
		start[61] = 15; 
		start[123] = 16; 
		start[44] = 17; 
		start[125] = 18; 
		start[43] = 19; 
		start[124] = 20; 
		start[46] = 21; 
		start[40] = 22; 
		start[41] = 23; 
		start[91] = 24; 
		start[47] = 25; 
		start[93] = 26; 
		start[92] = 27; 
		start[45] = 28; 
		start[42] = 29; 
		start[37] = 30; 
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
			case "use": t.kind = 8; break;
			case "0": t.kind = 13; break;
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
				{t.kind = 2; break;}
			case 2:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 2;}
				else {t.kind = 3; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 3:
				if (ch >= 'A' && ch <= 'Z') {AddCh(); goto case 4;}
				else {t.kind = noSym; break;}
			case 4:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else if (ch == '.') {AddCh(); goto case 5;}
				else {t.kind = 4; break;}
			case 5:
				if (ch >= 'A' && ch <= 'Z') {AddCh(); goto case 6;}
				else {t.kind = noSym; break;}
			case 6:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 6;}
				else if (ch == '.') {AddCh(); goto case 5;}
				else {t.kind = 4; break;}
			case 7:
				if (ch >= 'a' && ch <= 'z') {AddCh(); goto case 8;}
				else {t.kind = noSym; break;}
			case 8:
				if (ch == '_') {AddCh(); goto case 9;}
				else if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 8;}
				else {t.kind = noSym; break;}
			case 9:
				{t.kind = 5; break;}
			case 10:
				if (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 11;}
				else {t.kind = noSym; break;}
			case 11:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 11;}
				else {t.kind = 6; break;}
			case 12:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 12;}
				else {t.kind = 7; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 13:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 13;}
				else if (ch == 39) {AddCh(); goto case 14;}
				else if (ch == '_') {AddCh(); goto case 1;}
				else if (ch == '.') {AddCh(); goto case 3;}
				else {t.kind = 1; break;}
			case 14:
				if (ch == 39) {AddCh(); goto case 14;}
				else if (ch == '_') {AddCh(); goto case 1;}
				else {t.kind = 1; break;}
			case 15:
				{t.kind = 9; break;}
			case 16:
				{t.kind = 10; break;}
			case 17:
				{t.kind = 11; break;}
			case 18:
				{t.kind = 12; break;}
			case 19:
				{t.kind = 14; break;}
			case 20:
				{t.kind = 15; break;}
			case 21:
				{t.kind = 16; break;}
			case 22:
				{t.kind = 17; break;}
			case 23:
				{t.kind = 18; break;}
			case 24:
				{t.kind = 19; break;}
			case 25:
				{t.kind = 20; break;}
			case 26:
				{t.kind = 21; break;}
			case 27:
				{t.kind = 22; break;}
			case 28:
				{t.kind = 23; break;}
			case 29:
				{t.kind = 24; break;}
			case 30:
				{t.kind = 25; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
} // end Scanner

}