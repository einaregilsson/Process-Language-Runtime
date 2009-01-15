
using System;

namespace WC {



public class Parser {
	const int _EOF = 0;
	const int _ident = 1;
	const int _number = 2;
	const int maxT = 41;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

IWhileCompiler compiler; 


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

	
	void StmtSeq() {
		Stmt();
		while (la.kind == 3) {
			Get();
			Stmt();
		}
	}

	void Stmt() {
		switch (la.kind) {
		case 1: {
			Assign();
			break;
		}
		case 4: {
			Get();
			compiler.Skip(); 
			break;
		}
		case 8: {
			Block();
			break;
		}
		case 11: {
			If();
			break;
		}
		case 15: {
			While();
			break;
		}
		case 5: {
			Get();
			Expect(1);
			compiler.Read(t.val); 
			break;
		}
		case 6: {
			Get();
			AExpr();
			compiler.WriteArithmetic(); 
			break;
		}
		default: SynErr(42); break;
		}
	}

	void Assign() {
		Expect(1);
		string variable = t.val; 
		Expect(10);
		Expr();
		compiler.Assign(variable); 
	}

	void Block() {
		Expect(8);
		compiler.BlockBegin(); 
		if (la.kind == 7) {
			VarDec();
		}
		StmtSeq();
		Expect(9);
		compiler.BlockEnd(); 
	}

	void If() {
		Expect(11);
		Expr();
		Expect(12);
		StmtSeq();
		if (la.kind == 13) {
			Get();
			StmtSeq();
		}
		Expect(14);
	}

	void While() {
		Expect(15);
		Expr();
		Expect(16);
		StmtSeq();
		Expect(17);
	}

	void AExpr() {
		BitOr();
		while (la.kind == 26) {
			Get();
			string op = t.val; 
			BitOr();
			compiler.BinaryOp(op); 
		}
	}

	void VarDec() {
		Expect(7);
		Expect(1);
		compiler.VarDec(t.val); 
		Expect(3);
		if (la.kind == 7) {
			VarDec();
		}
	}

	void Expr() {
		LogicOr();
		while (la.kind == 18) {
			Get();
			string op = t.val; 
			LogicOr();
			compiler.BinaryOp(op); 
		}
	}

	void LogicOr() {
		LogicAnd();
		while (la.kind == 19) {
			Get();
			string op = t.val; 
			LogicAnd();
			compiler.BinaryOp(op); 
		}
	}

	void LogicAnd() {
		EqualComp();
		if (la.kind == 20 || la.kind == 21) {
			if (la.kind == 20) {
				Get();
			} else {
				Get();
			}
			EqualComp();
		}
	}

	void EqualComp() {
		AExpr();
		if (StartOf(1)) {
			if (la.kind == 22) {
				Get();
			} else if (la.kind == 23) {
				Get();
			} else if (la.kind == 24) {
				Get();
			} else {
				Get();
			}
			string op = t.val; 
			AExpr();
			compiler.BinaryOp(op); 
		}
	}

	void BitOr() {
		BitXor();
		while (la.kind == 27) {
			Get();
			string op = t.val; 
			BitXor();
			compiler.BinaryOp(op); 
		}
	}

	void BitXor() {
		BitAnd();
		while (la.kind == 28) {
			Get();
			string op = t.val; 
			BitAnd();
			compiler.BinaryOp(op); 
		}
	}

	void BitAnd() {
		BitShift();
		while (la.kind == 29 || la.kind == 30) {
			if (la.kind == 29) {
				Get();
			} else {
				Get();
			}
			string op = t.val; 
			BitShift();
			compiler.BinaryOp(op); 
		}
	}

	void BitShift() {
		PlusMinus();
		while (la.kind == 31 || la.kind == 32) {
			if (la.kind == 31) {
				Get();
			} else {
				Get();
			}
			string op = t.val; 
			PlusMinus();
			compiler.BinaryOp(op); 
		}
	}

	void PlusMinus() {
		MulDivMod();
		while (la.kind == 33 || la.kind == 34 || la.kind == 35) {
			if (la.kind == 33) {
				Get();
			} else if (la.kind == 34) {
				Get();
			} else {
				Get();
			}
			string op = t.val; 
			MulDivMod();
			compiler.BinaryOp(op); 
		}
	}

	void MulDivMod() {
		string unary = ""; 
		if (la.kind == 32 || la.kind == 36) {
			if (la.kind == 32) {
				Get();
			} else {
				Get();
				unary = t.val; 
			}
		}
		if (la.kind == 1) {
			Get();
			compiler.Ident(t.val); 
		} else if (la.kind == 2) {
			Get();
			compiler.Number(int.Parse(t.val)); 
		} else if (la.kind == 37) {
			Get();
		} else if (la.kind == 38) {
			Get();
		} else if (la.kind == 39) {
			Get();
			Expr();
			Expect(40);
		} else SynErr(43);
		if (unary != "") compiler.UnaryOp(unary); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		StmtSeq();

    Expect(0);
	}
	
	bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

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
			case 3: s = "\";\" expected"; break;
			case 4: s = "\"skip\" expected"; break;
			case 5: s = "\"read\" expected"; break;
			case 6: s = "\"write\" expected"; break;
			case 7: s = "\"var\" expected"; break;
			case 8: s = "\"begin\" expected"; break;
			case 9: s = "\"end\" expected"; break;
			case 10: s = "\":=\" expected"; break;
			case 11: s = "\"if\" expected"; break;
			case 12: s = "\"then\" expected"; break;
			case 13: s = "\"else\" expected"; break;
			case 14: s = "\"fi\" expected"; break;
			case 15: s = "\"while\" expected"; break;
			case 16: s = "\"do\" expected"; break;
			case 17: s = "\"od\" expected"; break;
			case 18: s = "\"||\" expected"; break;
			case 19: s = "\"&&\" expected"; break;
			case 20: s = "\"==\" expected"; break;
			case 21: s = "\"!=\" expected"; break;
			case 22: s = "\"<\" expected"; break;
			case 23: s = "\">\" expected"; break;
			case 24: s = "\"<=\" expected"; break;
			case 25: s = "\">=\" expected"; break;
			case 26: s = "\"|\" expected"; break;
			case 27: s = "\"^\" expected"; break;
			case 28: s = "\"&\" expected"; break;
			case 29: s = "\"<<\" expected"; break;
			case 30: s = "\">>\" expected"; break;
			case 31: s = "\"+\" expected"; break;
			case 32: s = "\"-\" expected"; break;
			case 33: s = "\"*\" expected"; break;
			case 34: s = "\"/\" expected"; break;
			case 35: s = "\"%\" expected"; break;
			case 36: s = "\"~\" expected"; break;
			case 37: s = "\"true\" expected"; break;
			case 38: s = "\"false\" expected"; break;
			case 39: s = "\"(\" expected"; break;
			case 40: s = "\")\" expected"; break;
			case 41: s = "??? expected"; break;
			case 42: s = "invalid Stmt"; break;
			case 43: s = "invalid MulDivMod"; break;

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

}