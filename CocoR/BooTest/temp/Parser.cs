
using System;



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

	
	void Program() {
		statements as StatementSequence
		StmtSeq(statements);
	}

	void StmtSeq(ref statements as StatementSequence) {
		stmt as Statement
		Stmt(stmt);
		while (la.kind == 3) {
			Get();
			Stmt(stmt);
		}
	}

	void Stmt(ref stmt as Statement) {
		exp as Expression 
		switch (la.kind) {
		case 1: {
			Assign(stmt);
			break;
		}
		case 4: {
			Get();
			break;
		}
		case 8: {
			Block(stmt);
			break;
		}
		case 11: {
			If(stmt);
			break;
		}
		case 15: {
			While(stmt);
			break;
		}
		case 5: {
			Get();
			Expect(1);
			break;
		}
		case 6: {
			Get();
			Expr(exp);
			break;
		}
		default: SynErr(42); break;
		}
	}

	void Assign(ref assign as Statement) {
		Expect(1);
		Expect(10);
		Expr(exp);
	}

	void Block(ref block as Statement) {
		Expect(8);
		vars as (VariableDeclaration)
		if (la.kind == 7) {
			VarDec(vars);
		}
		statements as StatementSequence 
		StmtSeq(statements);
		Expect(9);
	}

	void If(ref ifStmt as Statement) {
		Expect(11);
		Expr(exp);
		Expect(12);
		StmtSeq(ifBranch);
		if (la.kind == 13) {
			Get();
			StmtSeq(elseBranch);
		}
		Expect(14);
	}

	void While(ref whileStmt as Statement) {
		Expect(15);
		Expr(exp);
		Expect(16);
		StmtSeq(whileBranch);
		Expect(17);
	}

	void Expr(ref exp as Expression) {
		LogicOr(exp);
	}

	void VarDec(ref vars as (VariableDeclaration)) {
		Expect(7);
		Expect(1);
		while (la.kind == 3) {
			Get();
			Expect(7);
			Expect(1);
		}
		Expect(3);
	}

	void LogicOr(ref exp as Expression) {
		LogicAnd(exp);
		while (la.kind == 18) {
			Get();
			LogicAnd(second);
		}
	}

	void LogicAnd(ref exp as Expression) {
		EqualComp(exp);
		while (la.kind == 19) {
			Get();
			EqualComp(second);
		}
	}

	void EqualComp(ref exp as Expression) {
		GreatOrEqual(exp);
		if (la.kind == 20 || la.kind == 21) {
			if (la.kind == 20) {
				Get();
			} else {
				Get();
			}
			GreatOrEqual(second);
		}
	}

	void GreatOrEqual(ref exp as Expression) {
		BitOr(exp);
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
			BitOr(second);
		}
	}

	void BitOr(ref exp as Expression) {
		BitXor(exp);
		while (la.kind == 26) {
			Get();
			BitXor(second);
		}
	}

	void BitXor(ref exp as Expression) {
		BitAnd(exp);
		while (la.kind == 27) {
			Get();
			BitAnd(second);
		}
	}

	void BitAnd(ref exp as Expression) {
		BitShift(exp);
		while (la.kind == 28) {
			Get();
			BitShift(exp);
		}
	}

	void BitShift(ref exp as Expression) {
		PlusMinus(exp);
		while (la.kind == 29 || la.kind == 30) {
			if (la.kind == 29) {
				Get();
			} else {
				Get();
			}
			PlusMinus(second);
		}
	}

	void PlusMinus(ref exp as Expression) {
		MulDivMod(exp);
		while (la.kind == 31 || la.kind == 32) {
			if (la.kind == 31) {
				Get();
			} else {
				Get();
			}
			MulDivMod(second);
		}
	}

	void MulDivMod(ref exp as Expression) {
		UnaryOperator(exp);
		while (la.kind == 33 || la.kind == 34 || la.kind == 35) {
			if (la.kind == 33) {
				Get();
			} else if (la.kind == 34) {
				Get();
			} else {
				Get();
			}
			UnaryOperator(second);
		}
	}

	void UnaryOperator(ref exp as Expression) {
		if (la.kind == 32 || la.kind == 36) {
			if (la.kind == 32) {
				Get();
			} else {
				Get();
			}
		}
		Terminal(exp);
	}

	void Terminal(ref exp as Expression) {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else if (la.kind == 37) {
			Get();
		} else if (la.kind == 38) {
			Get();
		} else if (la.kind == 39) {
			Get();
			Expr(exp);
			Expect(40);
		} else SynErr(43);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Program();

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
			case 43: s = "invalid Terminal"; break;

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

