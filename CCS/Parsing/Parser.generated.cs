using PLR.AST;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using Action = PLR.AST.Actions.Action;

using System;

namespace CCS.Parsing {



public partial class Parser {
	public const int _EOF = 0;
	public const int _PROCNAME = 1;
	public const int _PROCNAMESUB = 2;
	public const int _LCASEIDENT = 3;
	public const int _OUTACTION = 4;
	public const int _NUMBER = 5;
	public const int _ENTRYPROC = 6;
	public const int maxT = 25;


s
    
    private ProcessSystem system = new ProcessSystem();
    public ProcessSystem System {get { return this.system;}}
    


	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void CCS() {
		ProcessDefinition proc; 
		ProcessDefinition(out proc);
		this.System.Add(proc); 
		while (la.kind == 1 || la.kind == 2 || la.kind == 6) {
			ProcessDefinition(out proc);
			this.System.Add(proc); 
		}
		this.System.CopyPos(this.System[0],t); 
	}

	void ProcessDefinition(out ProcessDefinition procdef) {
		procdef = new ProcessDefinition(); Process proc; ProcessConstant pc; 
		if (la.kind == 6) {
			Get();
			procdef.EntryProc = true; 
		}
		ProcessConstantDef(out pc);
		procdef.ProcessConstant = pc; 
		Expect(7);
		Process(out proc);
		procdef.Process = proc; procdef.CopyPos(pc,t); 
	}

	void ProcessConstantDef(out ProcessConstant pc) {
		pc = new ProcessConstant(); 
		if (la.kind == 1) {
			Get();
			pc.Name = t.val; pc.SetPos(t);
		} else if (la.kind == 2) {
			Get();
			pc.Name = t.val.Replace("_","");  pc.SetPos(t); ArithmeticExpression subscript; 
			Expect(8);
			Token startToken = t; 
			Subscript(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 9) {
				Get();
				Subscript(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(10);
			pc.Subscript.SetPos(startToken); pc.Subscript.Length = t.pos - pc.Subscript.Pos; 
		} else SynErr(26);
	}

	void Process(out Process proc) {
		NonDeterministicChoice(out proc);
	}

	void Subscript(out ArithmeticExpression sub) {
		sub = null; 
		if (la.kind == 3) {
			Get();
			sub = new Variable(t.val); sub.SetPos(t); 
		} else if (la.kind == 5) {
			Get();
		} else if (la.kind == 11) {
			Get();
			sub = new Constant(int.Parse(t.val)); sub.SetPos(t); 
		} else SynErr(27);
	}

	void NonDeterministicChoice(out Process proc) {
		Process pc; NonDeterministicChoice ndc = new NonDeterministicChoice();
		ParallelComposition(out pc);
		ndc.Add(pc); 
		while (la.kind == 12) {
			Get();
			ParallelComposition(out pc);
			ndc.Add(pc); 
		}
		if (ndc.Count == 1) {proc = ndc[0]; }else {proc = ndc; proc.CopyPos(ndc[0],t);}
	}

	void ParallelComposition(out Process proc) {
		Process ap; ParallelComposition pc = new ParallelComposition();
		ActionPrefix(out ap);
		pc.Add(ap); 
		while (la.kind == 13) {
			Get();
			ActionPrefix(out ap);
			pc.Add(ap); 
		}
		if (pc.Count == 1) proc = pc[0]; else {proc = pc; proc.CopyPos(pc[0],t);}
	}

	void ActionPrefix(out Process proc) {
		ActionPrefix ap = null; ActionPrefix prev = null; ActionPrefix first = null; Process nextProc = null; ProcessConstant pc; proc = null; Action act = null; 
		while (la.kind == 3 || la.kind == 4 || la.kind == 17) {
			Action(out act);
			Expect(14);
			ap = new ActionPrefix(act); ap.CopyPos(ap.Action,t); if (first == null) first = ap; if (prev != null) { prev.Process = ap;} prev = ap;
		}
		if (la.kind == 15) {
			Get();
			Process(out nextProc);
			Expect(16);
			nextProc.ParenCount++; 
		} else if (la.kind == 11) {
			Get();
			nextProc = new NilProcess(); nextProc.SetPos(t);
		} else if (la.kind == 1 || la.kind == 2) {
			ProcessConstantInvoke(out pc);
			nextProc = pc; 
		} else SynErr(28);
		if (first == null) proc = nextProc; else {ap.Process = nextProc; proc = first;}; 
		if (la.kind == 18) {
			Relabelling(nextProc.Relabelling);
		}
		if (la.kind == 21) {
			Restriction(nextProc.Restrictions);
		}
	}

	void Action(out Action act) {
		act = null; 
		if (la.kind == 17) {
			Get();
			act = new TauAction(); act.SetPos(t); 
		} else if (la.kind == 3) {
			Get();
			act = new InAction(t.val); act.SetPos(t);
		} else if (la.kind == 4) {
			Get();
			if (t.val == "_t_") SemErr("Tau actions cannot be output actions!"); act = new OutAction(t.val); act.SetPos(t);
		} else SynErr(29);
	}

	void ProcessConstantInvoke(out ProcessConstant pc) {
		pc = new ProcessConstant(); 
		if (la.kind == 1) {
			Get();
			pc.Name = t.val; pc.SetPos(t); 
		} else if (la.kind == 2) {
			Get();
			pc.Name = t.val.Replace("_","");  pc.SetPos(t); ArithmeticExpression subscript; 
			Expect(8);
			ArithmeticExpression(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 9) {
				Get();
				ArithmeticExpression(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(10);
		} else SynErr(30);
	}

	void Relabelling(Relabellings labels) {
		string relabelTo, relabelFrom; 
		Expect(18);
		labels.SetPos(t); 
		Expect(3);
		relabelTo = t.val; 
		Expect(19);
		Expect(3);
		relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
		while (la.kind == 9) {
			Get();
			Expect(3);
			relabelTo = t.val; 
			Expect(19);
			Expect(3);
			relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
		}
		Expect(20);
	}

	void Restriction(Restrictions res) {
		Expect(21);
		if (la.kind == 3) {
			Get();
			res.Add(t.val); res.SetPos(t); res.HasParens = false; 
		} else if (la.kind == 8) {
			Get();
			res.HasParens = true; 
			Expect(3);
			res.Add(t.val); res.SetPos(t); 
			while (la.kind == 9) {
				Get();
				Expect(3);
				res.Add(t.val); 
			}
			Expect(10);
		} else SynErr(31);
	}

	void ArithmeticExpression(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		PlusMinusTerm(out left);
		aexp = left; 
		while (la.kind == 12 || la.kind == 22) {
			if (la.kind == 12) {
				Get();
				op = ArithmeticBinOp.Plus; 
			} else {
				Get();
				op = ArithmeticBinOp.Minus; 
			}
			PlusMinusTerm(out right);
			aexp = new ArithmeticBinOpExpression(aexp, right, op); aexp.CopyPos(((ArithmeticBinOpExpression)aexp).Left,t);
		}
	}

	void PlusMinusTerm(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 19 || la.kind == 23 || la.kind == 24) {
			if (la.kind == 23) {
				Get();
				op = ArithmeticBinOp.Multiply; 
			} else if (la.kind == 19) {
				Get();
				op = ArithmeticBinOp.Divide; 
			} else {
				Get();
				op = ArithmeticBinOp.Modulo; 
			}
			UnaryMinusTerm(out right);
			aexp = new ArithmeticBinOpExpression(aexp, right, op); aexp.CopyPos(((ArithmeticBinOpExpression)aexp).Left,t); 
		}
	}

	void UnaryMinusTerm(out ArithmeticExpression aexp) {
		bool isMinus = false; Token minusToken = null; aexp = null; 
		if (la.kind == 22) {
			Get();
			isMinus = true; minusToken = t; 
		}
		if (la.kind == 15) {
			Get();
			ArithmeticExpression(out aexp);
			Expect(16);
			aexp.ParenCount += 1; 
		} else if (la.kind == 5) {
			Get();
			aexp = new Constant(int.Parse(t.val)); aexp.SetPos(t); 
		} else if (la.kind == 11) {
			Get();
			aexp = new Constant(int.Parse(t.val)); aexp.SetPos(t);
		} else if (la.kind == 3) {
			Get();
			aexp = new Variable(t.val); aexp.SetPos(t); 
		} else SynErr(32);
		if (isMinus) {aexp = new UnaryMinus(aexp); aexp.SetPos(minusToken);} 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		CCS();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

	};
} // end Parser


public partial class Errors {
    private string GetErrorMessage(int n) {
        string s = null;
        switch(n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "PROCNAME expected"; break;
			case 2: s = "PROCNAMESUB expected"; break;
			case 3: s = "LCASEIDENT expected"; break;
			case 4: s = "OUTACTION expected"; break;
			case 5: s = "NUMBER expected"; break;
			case 6: s = "ENTRYPROC expected"; break;
			case 7: s = "\"=\" expected"; break;
			case 8: s = "\"{\" expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\"}\" expected"; break;
			case 11: s = "\"0\" expected"; break;
			case 12: s = "\"+\" expected"; break;
			case 13: s = "\"|\" expected"; break;
			case 14: s = "\".\" expected"; break;
			case 15: s = "\"(\" expected"; break;
			case 16: s = "\")\" expected"; break;
			case 17: s = "\"t\" expected"; break;
			case 18: s = "\"[\" expected"; break;
			case 19: s = "\"/\" expected"; break;
			case 20: s = "\"]\" expected"; break;
			case 21: s = "\"\\\\\" expected"; break;
			case 22: s = "\"-\" expected"; break;
			case 23: s = "\"*\" expected"; break;
			case 24: s = "\"%\" expected"; break;
			case 25: s = "??? expected"; break;
			case 26: s = "invalid ProcessConstantDef"; break;
			case 27: s = "invalid Subscript"; break;
			case 28: s = "invalid ActionPrefix"; break;
			case 29: s = "invalid Action"; break;
			case 30: s = "invalid ProcessConstantInvoke"; break;
			case 31: s = "invalid Restriction"; break;
			case 32: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}