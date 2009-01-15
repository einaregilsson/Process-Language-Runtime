
using System;



public class Parser {
	const int _EOF = 0;
	const int _ident = 1;
	const int _number = 2;
	const int maxT = 26;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Icelandic() {
		Stmt();
		while (StartOf(1)) {
			Stmt();
		}
	}

	void Stmt() {
		if (la.kind == 4) {
			VarDec();
		} else if (la.kind == 1) {
			Assign();
		} else if (la.kind == 10) {
			If();
		} else if (la.kind == 23) {
			While();
		} else SynErr(27);
		Expect(3);
	}

	void VarDec() {
		Expect(4);
		Expect(1);
		Expect(5);
		if (la.kind == 6) {
			Get();
		} else if (la.kind == 7) {
			Get();
		} else if (la.kind == 8) {
			Get();
		} else SynErr(28);
	}

	void Assign() {
		Expect(1);
		Expect(9);
		Expr();
	}

	void If() {
		Expect(10);
		Expr();
		Expect(11);
		Expect(12);
		if (la.kind == 13) {
			Get();
		} else if (la.kind == 14) {
			Get();
		} else SynErr(29);
		Expect(15);
		Icelandic();
		if (la.kind == 16) {
			Get();
			Expect(17);
			Expect(5);
			Expect(18);
			Expect(19);
		} else if (la.kind == 20) {
			Get();
			Expect(21);
			Expect(22);
		} else SynErr(30);
	}

	void While() {
		Expect(23);
		Expect(24);
		Expect(25);
	}

	void Expr() {
		Expect(1);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Icelandic();

    Expect(0);
	}
	
	bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
  public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
  
	public void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "\".\" expected"; break;
			case 4: s = "\"Breytan\" expected"; break;
			case 5: s = "\"er\" expected"; break;
			case 6: s = "\"heiltala\" expected"; break;
			case 7: s = "\"textastrengur\" expected"; break;
			case 8: s = "\"sannleiksgildi\" expected"; break;
			case 9: s = "\"ver\u00f0ur\" expected"; break;
			case 10: s = "\"Ef\" expected"; break;
			case 11: s = "\"\u00fe\u00e1\" expected"; break;
			case 12: s = "\"gerist\" expected"; break;
			case 13: s = "\"eftirfarandi\" expected"; break;
			case 14: s = "\"\u00feetta\" expected"; break;
			case 15: s = "\":\" expected"; break;
			case 16: s = "\"Og\" expected"; break;
			case 17: s = "\"\u00fearme\u00f0\" expected"; break;
			case 18: s = "\"skilyr\u00f0inu\" expected"; break;
			case 19: s = "\"loki\u00f0\" expected"; break;
			case 20: s = "\"H\u00e9r\" expected"; break;
			case 21: s = "\"endar\" expected"; break;
			case 22: s = "\"skilyr\u00f0i\u00f0\" expected"; break;
			case 23: s = "\"while\" expected"; break;
			case 24: s = "\"do\" expected"; break;
			case 25: s = "\"od\" expected"; break;
			case 26: s = "??? expected"; break;
			case 27: s = "invalid Stmt"; break;
			case 28: s = "invalid VarDec"; break;
			case 29: s = "invalid If"; break;
			case 30: s = "invalid If"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

