using While.AST;
using While.AST.Statements;
using While.AST.Expressions;
using System.Collections.Generic;

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
		StatementSequence statements = null; 
		StmtSeq(out statements);
	}

	void StmtSeq(out StatementSequence statements) {
		var statementList = new List<Statement>(); Statement stmt = null;
		Stmt(out stmt);
		statementList.Add(stmt); 
		while (la.kind == 3) {
			Get();
			Stmt(out stmt);
			statementList.Add(stmt); 
		}
		statements = new StatementSequence(statementList); 
	}

	void Stmt(out Statement stmt) {
		Expression exp = null; stmt = null; 
		switch (la.kind) {
		case 1: {
			Assign(out stmt);
			break;
		}
		case 4: {
			Get();
			stmt = new Skip(); 
			break;
		}
		case 8: {
			Block(out stmt);
			break;
		}
		case 11: {
			If(out stmt);
			break;
		}
		case 15: {
			While(out stmt);
			break;
		}
		case 5: {
			Get();
			Expect(1);
			stmt = new Read(new Variable(t.val)); 
			break;
		}
		case 6: {
			Get();
			Expr(out exp);
			stmt = new Write(exp); 
			break;
		}
		default: SynErr(42); break;
		}
	}

	void Assign(out Statement assign) {
		Expression exp = null; 
		Expect(1);
		Variable v = new Variable(t.val); 
		Expect(10);
		Expr(out exp);
		assign = new Assign(v, exp); 
	}

	void Block(out Statement block) {
		StatementSequence statements = null; VariableDeclaration[] vars = null; 
		Expect(8);
		if (la.kind == 7) {
			VarDec(out vars);
		}
		StmtSeq(out statements);
		block = new Block(vars, statements); 
		Expect(9);
	}

	void If(out Statement ifStmt) {
		Expression exp = null; StatementSequence ifBranch = null, elseBranch = null; 
		Expect(11);
		Expr(out exp);
		Expect(12);
		StmtSeq(out ifBranch);
		if (la.kind == 13) {
			Get();
			StmtSeq(out elseBranch);
		}
		Expect(14);
		ifStmt = new If(exp, ifBranch, elseBranch); 
	}

	void While(out Statement whileStmt) {
		Expression exp = null; StatementSequence whileBranch = null; 
		Expect(15);
		Expr(out exp);
		Expect(16);
		StmtSeq(out whileBranch);
		Expect(17);
		whileStmt = new While.AST.Statements.While(exp, whileBranch); 
	}

	void Expr(out Expression exp) {
		LogicOr(out exp);
	}

	void VarDec(out VariableDeclaration[] vars) {
		var varList = new List<VariableDeclaration>(); 
		Expect(7);
		Expect(1);
		varList.Add(new VariableDeclaration(t.val)); 
		while (la.kind == 3) {
			Get();
			Expect(7);
			Expect(1);
			varList.Add(new VariableDeclaration(t.val)); 
		}
		Expect(3);
		vars = varList.ToArray(); 
	}

	void LogicOr(out Expression exp) {
		Expression second = null; 
		LogicAnd(out exp);
		while (la.kind == 18) {
			Get();
			LogicAnd(out second);
			exp = new BinaryOp(BinaryOp.LogicOr, exp, second); 
		}
	}

	void LogicAnd(out Expression exp) {
		Expression second = null; 
		EqualComp(out exp);
		while (la.kind == 19) {
			Get();
			EqualComp(out second);
			exp = new BinaryOp(BinaryOp.LogicAnd, exp, second); 
		}
	}

	void EqualComp(out Expression exp) {
		Expression second = null; string op;
		GreatOrEqual(out exp);
		if (la.kind == 20 || la.kind == 21) {
			if (la.kind == 20) {
				Get();
				op = BinaryOp.Equal; 
			} else {
				Get();
				op = BinaryOp.NotEquals; 
			}
			GreatOrEqual(out second);
			exp = new BinaryOp(op, exp, second); 
		}
	}

	void GreatOrEqual(out Expression exp) {
		Expression second = null; string op; 
		BitOr(out exp);
		if (StartOf(1)) {
			if (la.kind == 22) {
				Get();
				op = BinaryOp.LessThan; 
			} else if (la.kind == 23) {
				Get();
				op = BinaryOp.GreaterThan; 
			} else if (la.kind == 24) {
				Get();
				op = BinaryOp.LessThanOrEqual; 
			} else {
				Get();
				op = BinaryOp.GreaterThanOrEqual; 
			}
			BitOr(out second);
			exp = new BinaryOp(op, exp, second); 
		}
	}

	void BitOr(out Expression exp) {
		Expression second = null; 
		BitXor(out exp);
		while (la.kind == 26) {
			Get();
			BitXor(out second);
			exp = new BinaryOp(BinaryOp.BitOr, exp, second); 
		}
	}

	void BitXor(out Expression exp) {
		Expression second = null; 
		BitAnd(out exp);
		while (la.kind == 27) {
			Get();
			BitAnd(out second);
			exp = new BinaryOp(BinaryOp.BitXor, exp, second); 
		}
	}

	void BitAnd(out Expression exp) {
		Expression second = null; 
		BitShift(out exp);
		while (la.kind == 28) {
			Get();
			BitShift(out exp);
			exp = new BinaryOp(BinaryOp.BitAnd, exp, second); 
		}
	}

	void BitShift(out Expression exp) {
		Expression second = null; string op; 
		PlusMinus(out exp);
		while (la.kind == 29 || la.kind == 30) {
			if (la.kind == 29) {
				Get();
				op = BinaryOp.BitShiftLeft; 
			} else {
				Get();
				op = BinaryOp.BitShiftRight; 
			}
			PlusMinus(out second);
			exp = new BinaryOp(op, exp, second); 
		}
	}

	void PlusMinus(out Expression exp) {
		Expression second = null; string op; 
		MulDivMod(out exp);
		while (la.kind == 31 || la.kind == 32) {
			if (la.kind == 31) {
				Get();
				op = BinaryOp.Plus; 
			} else {
				Get();
				op = BinaryOp.Minus; 
			}
			MulDivMod(out second);
			exp = new BinaryOp(op, exp, second); 
		}
	}

	void MulDivMod(out Expression exp) {
		Expression second = null; string op; 
		UnaryOperator(out exp);
		while (la.kind == 33 || la.kind == 34 || la.kind == 35) {
			if (la.kind == 33) {
				Get();
				op = BinaryOp.Multiplication; 
			} else if (la.kind == 34) {
				Get();
				op = BinaryOp.Division; 
			} else {
				Get();
				op = BinaryOp.Modulo; 
			}
			UnaryOperator(out second);
			exp = new BinaryOp(op, exp, second); 
		}
	}

	void UnaryOperator(out Expression exp) {
		string op = null; bool isUnary = false; 
		if (la.kind == 32 || la.kind == 36) {
			if (la.kind == 32) {
				isUnary = true; 
				Get();
				op = UnaryOp.Minus; 
			} else {
				Get();
				op = UnaryOp.BitNegate; 
			}
		}
		Terminal(out exp);
		if (isUnary) exp = new UnaryOp(op, exp); 
	}

	void Terminal(out Expression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new Variable(t.val); 
		} else if (la.kind == 2) {
			Get();
			exp = new Number(int.Parse(t.val)); 
		} else if (la.kind == 37) {
			Get();
			exp = new Bool(true); 
		} else if (la.kind == 38) {
			Get();
			exp = new Bool(false); 
		} else if (la.kind == 39) {
			Get();
			Expr(out exp);
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

}