using PLR.AST;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using PLR.AST.ActionHandling;
using KLAIM.AST;
using Action = KLAIM.AST.Action;
using System.Collections.Generic;

using System;

namespace KLAIM.Parsing {



public partial class Parser {
	public const int _EOF = 0;
	public const int _LOCALITY = 1;
	public const int _VARIABLE = 2;
	public const int _VARBINDING = 3;
	public const int _NUMBER = 4;
	public const int maxT = 24;




	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void KLAIM() {
		LocatedItem();
		while (la.kind == 5) {
			Get();
			LocatedItem();
		}
	}

	void LocatedItem() {
		TupleInfo ti = null; Process p = null; 
		Expect(1);
		string locality = t.val; 
		Expect(6);
		if (la.kind == 7) {
			Tuple(out ti, locality);
			this.tuples.Add(ti); 
		} else if (StartOf(1)) {
			Process(out p, locality);
			this.Processes.Add(new ProcessDefinition(p, GetProcName(locality), true)); 
		} else SynErr(25);
	}

	void Tuple(out TupleInfo ti, string locality) {
		object val = null; List<object> items = new List<object>();
		Expect(7);
		Constant(out val);
		items.Add(val); 
		while (la.kind == 8) {
			Get();
			Constant(out val);
			items.Add(val); 
		}
		Expect(9);
		ti = new TupleInfo(){Items = items, Locality = locality}; 
	}

	void Process(out Process p, string locality) {
		p = null; bool replicated = false; int startAction = _actionNr; Token replTok = null; 
		if (la.kind == 10) {
			Get();
			replicated = true; replTok = t;
		}
		NonDeterministicChoice(out p, locality);
		if (replicated) { 
		       if (!(p is ActionPrefix) || (p is ActionPrefix && !(((ActionPrefix)p).Action is InAction))) {
		           errors.SemErr(replTok.line, replTok.col, "Replicated processes are required to start with an in action!");
		        }
		       p = new ReplicatedProcess(p); 
		  }
	}

	void Constant(out object val) {
		val = null; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 4) {
			Get();
			val = int.Parse(t.val); 
		} else SynErr(26);
	}

	void NonDeterministicChoice(out Process proc, string locality) {
		NonDeterministicChoice ndc = new NonDeterministicChoice(); Process pc = null; 
		ParallelComposition(out pc, locality);
		ndc.Add(pc); 
		while (la.kind == 11) {
			Get();
			ParallelComposition(out pc, locality);
			ndc.Add(pc); 
		}
		if (ndc.Processes.Count == 1) {proc = ndc.Processes[0]; } else {proc = ndc; CopyPos(proc,ndc.Processes[0], t);}
	}

	void ParallelComposition(out Process proc, string locality) {
		Process ap; ParallelComposition pc = new ParallelComposition();
		ActionPrefix(out ap, locality);
		pc.Add(ap); 
		while (la.kind == 12) {
			Get();
			ActionPrefix(out ap, locality);
			pc.Add(ap); 
		}
		if (pc.Processes.Count == 1) proc = pc.Processes[0]; else {proc = pc; CopyPos(pc.Processes[0],proc,t);}
	}

	void ActionPrefix(out Process proc, string locality) {
		Process nextproc = new NilProcess(); proc = null; Action action = null;
		if (la.kind == 17 || la.kind == 19 || la.kind == 20) {
			Action(out action, locality);
			if (la.kind == 13) {
				Get();
				ActionPrefix(out nextproc, locality);
			}
			proc = new ActionPrefix(action, nextproc); CopyPos(proc, action, t);
		} else if (la.kind == 14) {
			Get();
			Process(out proc, locality);
			Expect(15);
		} else if (la.kind == 16) {
			Get();
			proc = new NilProcess(); SetPos(proc, t); 
		} else SynErr(27);
	}

	void Action(out Action action, string locality) {
		action = null; Token start = la;
		if (la.kind == 17) {
			OutAction(out action, locality);
		} else if (la.kind == 19 || la.kind == 20) {
			InOrReadAction(out action, locality);
		} else SynErr(28);
		action.Nr = _actionNr++; 
		Token end = t; SetPos(action, start, end); 
	}

	void OutAction(out Action action, string locality) {
		Expression exp = null; action = new OutAction(); action.Locality = locality;  
		Expect(17);
		Expect(14);
		OutParam(out exp);
		action.AddExpression(exp); 
		while (la.kind == 8) {
			Get();
			OutParam(out exp);
			action.AddExpression(exp); 
		}
		Expect(15);
		Expect(18);
		if (la.kind == 1) {
			Get();
			action.At = new PLRString(t.val); 
		} else if (la.kind == 2) {
			Get();
			action.At = new Variable(t.val); 
		} else SynErr(29);
	}

	void InOrReadAction(out Action action, string locality) {
		Expression exp = null;   action = null; 
		if (la.kind == 19) {
			Get();
			action = new InAction(); action.Locality = locality;
		} else if (la.kind == 20) {
			Get();
			action = new ReadAction();  action.Locality = locality;
		} else SynErr(30);
		Expect(14);
		InOrReadParam(out exp);
		action.AddExpression(exp); 
		while (la.kind == 8) {
			Get();
			InOrReadParam(out exp);
			action.AddExpression(exp); 
		}
		Expect(15);
		Expect(18);
		if (la.kind == 1) {
			Get();
			action.At = new PLRString(t.val); SetPos(action.At, t); 
		} else if (la.kind == 2) {
			Get();
			action.At = new Variable(t.val); SetPos(action.At, t);
		} else SynErr(31);
	}

	void OutParam(out Expression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new PLRString(t.val); 
		} else if (StartOf(2)) {
			Expression(out exp);
		} else SynErr(32);
	}

	void Expression(out Expression aexp) {
		ArithmeticBinOp op; Expression right = null, left = null; 
		PlusMinusTerm(out left);
		aexp = left; 
		while (la.kind == 11 || la.kind == 21) {
			if (la.kind == 11) {
				Get();
				op = ArithmeticBinOp.Plus; 
			} else {
				Get();
				op = ArithmeticBinOp.Minus; 
			}
			PlusMinusTerm(out right);
			aexp = new ArithmeticBinOpExpression(aexp, right, op); CopyPos(((ArithmeticBinOpExpression)aexp).Left,aexp,t);
		}
	}

	void InOrReadParam(out Expression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new PLRString(t.val); SetPos(exp, t);
		} else if (la.kind == 3) {
			Get();
			exp = new VariableBinding(t.val.Replace("!", "")); SetPos(exp, t);
		} else if (StartOf(2)) {
			Expression(out exp);
		} else SynErr(33);
	}

	void PlusMinusTerm(out Expression aexp) {
		ArithmeticBinOp op; Expression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 10 || la.kind == 22 || la.kind == 23) {
			if (la.kind == 10) {
				Get();
				op = ArithmeticBinOp.Multiply; 
			} else if (la.kind == 22) {
				Get();
				op = ArithmeticBinOp.Divide; 
			} else {
				Get();
				op = ArithmeticBinOp.Modulo; 
			}
			UnaryMinusTerm(out right);
			aexp = new ArithmeticBinOpExpression(aexp, right, op); CopyPos(((ArithmeticBinOpExpression)aexp).Left,aexp,t); 
		}
	}

	void UnaryMinusTerm(out Expression aexp) {
		bool isMinus = false; Token minusToken = null; aexp = null; 
		if (la.kind == 21) {
			Get();
			isMinus = true; minusToken = t; 
		}
		if (la.kind == 14) {
			Get();
			Expression(out aexp);
			Expect(15);
			aexp.ParenCount += 1; 
		} else if (la.kind == 4) {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t); 
		} else if (la.kind == 2) {
			Get();
			aexp = new Variable(t.val); SetPos(aexp, t); 
		} else SynErr(34);
		if (isMinus) {aexp = new UnaryMinus(aexp); SetPos(aexp, minusToken);} 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		KLAIM();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,x, T,T,x,T, T,x,x,x, x,x},
		{x,x,T,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x,x}

	};
} // end Parser


public partial class Errors {
    private string GetErrorMessage(int n) {
        string s = null;
        switch(n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "LOCALITY expected"; break;
			case 2: s = "VARIABLE expected"; break;
			case 3: s = "VARBINDING expected"; break;
			case 4: s = "NUMBER expected"; break;
			case 5: s = "\"||\" expected"; break;
			case 6: s = "\"::\" expected"; break;
			case 7: s = "\"<\" expected"; break;
			case 8: s = "\",\" expected"; break;
			case 9: s = "\">\" expected"; break;
			case 10: s = "\"*\" expected"; break;
			case 11: s = "\"+\" expected"; break;
			case 12: s = "\"|\" expected"; break;
			case 13: s = "\".\" expected"; break;
			case 14: s = "\"(\" expected"; break;
			case 15: s = "\")\" expected"; break;
			case 16: s = "\"nil\" expected"; break;
			case 17: s = "\"out\" expected"; break;
			case 18: s = "\"@\" expected"; break;
			case 19: s = "\"in\" expected"; break;
			case 20: s = "\"read\" expected"; break;
			case 21: s = "\"-\" expected"; break;
			case 22: s = "\"/\" expected"; break;
			case 23: s = "\"%\" expected"; break;
			case 24: s = "??? expected"; break;
			case 25: s = "invalid LocatedItem"; break;
			case 26: s = "invalid Constant"; break;
			case 27: s = "invalid ActionPrefix"; break;
			case 28: s = "invalid Action"; break;
			case 29: s = "invalid OutAction"; break;
			case 30: s = "invalid InOrReadAction"; break;
			case 31: s = "invalid InOrReadAction"; break;
			case 32: s = "invalid OutParam"; break;
			case 33: s = "invalid InOrReadParam"; break;
			case 34: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}