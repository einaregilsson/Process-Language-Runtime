using PLR.AST;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using PLR.AST.Actions;
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
	public const int maxT = 24;


private ProcessSystem system = new ProcessSystem();
    public ProcessSystem System {get { return this.system;}}
    
    private void SetPos(Node n, Token t) {
		n.SetPos(t.line, t.col, t.val.Length, t.pos);
    }
    private void CopyPos(Node from, Node to, Token end) {
		to.SetPos(from.Line, from.Column, t.pos-from.Position, from.Position);
    }
    


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
		ProcessDefinition(out proc, true);
		this.System.Add(proc); 
		while (la.kind == 1 || la.kind == 2) {
			ProcessDefinition(out proc, false);
			this.System.Add(proc); 
		}
		CopyPos(this.System[0],this.System, t); 
	}

	void ProcessDefinition(out ProcessDefinition procdef, bool entryProc) {
		Process proc; ProcessConstant pc; 
		ProcessConstantDef(out pc);
		Expect(6);
		Process(out proc);
		procdef = new ProcessDefinition(pc, proc, entryProc); //CopyPos(pc,procdef,t); 
	}

	void ProcessConstantDef(out ProcessConstant pc) {
		pc = null; 
		if (la.kind == 1) {
			Get();
			pc = new ProcessConstant(t.val); SetPos(pc, t);
		} else if (la.kind == 2) {
			Get();
			pc = new ProcessConstant(t.val.Replace("_",""));  SetPos(pc,t); ArithmeticExpression subscript; 
			Expect(7);
			Token startToken = t; 
			Subscript(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 8) {
				Get();
				Subscript(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(9);
			SetPos(pc.Subscript,startToken); pc.Subscript.Length = t.pos - pc.Subscript.Position; 
		} else SynErr(25);
	}

	void Process(out Process proc) {
		NonDeterministicChoice(out proc);
	}

	void Subscript(out ArithmeticExpression sub) {
		sub = null; 
		if (la.kind == 3) {
			Get();
			sub = new Variable(t.val); SetPos(sub, t); 
		} else if (la.kind == 5) {
			Get();
		} else if (la.kind == 10) {
			Get();
			sub = new Constant(int.Parse(t.val)); SetPos(sub, t); 
		} else SynErr(26);
	}

	void NonDeterministicChoice(out Process proc) {
		Process pc; NonDeterministicChoice ndc = new NonDeterministicChoice();
		ParallelComposition(out pc);
		ndc.Add(pc); 
		while (la.kind == 11) {
			Get();
			ParallelComposition(out pc);
			ndc.Add(pc); 
		}
		if (ndc.Count == 1) {proc = ndc[0]; }else {proc = ndc; CopyPos(ndc[0],proc, t);}
	}

	void ParallelComposition(out Process proc) {
		Process ap; ParallelComposition pc = new ParallelComposition();
		ActionPrefix(out ap);
		pc.Add(ap); 
		while (la.kind == 12) {
			Get();
			ActionPrefix(out ap);
			pc.Add(ap); 
		}
		if (pc.Count == 1) proc = pc[0]; else {proc = pc; CopyPos(pc[0],proc,t);}
	}

	void ActionPrefix(out Process proc) {
		ActionPrefix ap = null; ActionPrefix prev = null; ActionPrefix first = null; Process nextProc = null; ProcessConstant pc; proc = null; Action act = null; 
		while (la.kind == 3 || la.kind == 4 || la.kind == 16) {
			Action(out act);
			Expect(13);
			ap = new ActionPrefix(act, null); CopyPos(ap.Action,ap,t); if (first == null) first = ap; if (prev != null) { prev.Process = ap;} prev = ap;
		}
		if (la.kind == 14) {
			Get();
			Process(out nextProc);
			Expect(15);
			nextProc.ParenCount++; 
		} else if (la.kind == 10) {
			Get();
			nextProc = new NilProcess(); SetPos(nextProc, t);
		} else if (la.kind == 1 || la.kind == 2) {
			ProcessConstantInvoke(out pc);
			nextProc = pc; 
		} else SynErr(27);
		if (first == null) proc = nextProc; else {ap.Process = nextProc; proc = first;}; 
		if (la.kind == 17) {
			Relabelling(nextProc.Relabelling);
		}
		if (la.kind == 20) {
			Restriction(nextProc.Restrictions);
		}
	}

	void Action(out Action act) {
		act = null; 
		if (la.kind == 16) {
			Get();
			act = new TauAction(); SetPos(act, t); 
		} else if (la.kind == 3) {
			Get();
			act = new InAction(t.val); SetPos(act, t);
		} else if (la.kind == 4) {
			Get();
			if (t.val == "_t_") SemErr("Tau actions cannot be output actions!"); act = new OutAction(t.val); SetPos(act, t);
		} else SynErr(28);
	}

	void ProcessConstantInvoke(out ProcessConstant pc) {
		pc = null; 
		if (la.kind == 1) {
			Get();
			pc = new ProcessConstant(t.val); SetPos(pc, t); 
		} else if (la.kind == 2) {
			Get();
			pc = new ProcessConstant(t.val.Replace("_",""));  SetPos(pc, t); ArithmeticExpression subscript; 
			Expect(7);
			ArithmeticExpression(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 8) {
				Get();
				ArithmeticExpression(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(9);
		} else SynErr(29);
	}

	void Relabelling(Relabellings labels) {
		string relabelTo, relabelFrom; 
		Expect(17);
		SetPos(labels, t); 
		Expect(3);
		relabelTo = t.val; 
		Expect(18);
		Expect(3);
		relabelFrom = t.val; labels.Add(new ActionID(relabelFrom), new ActionID(relabelTo)); 
		while (la.kind == 8) {
			Get();
			Expect(3);
			relabelTo = t.val; 
			Expect(18);
			Expect(3);
			relabelFrom = t.val; labels.Add(new ActionID(relabelFrom), new ActionID(relabelTo)); 
		}
		Expect(19);
	}

	void Restriction(Restrictions res) {
		Expect(20);
		if (la.kind == 3) {
			Get();
			res.Add(new ActionID(t.val)); SetPos(res, t); res.ParenCount = 0; 
		} else if (la.kind == 7) {
			Get();
			res.ParenCount = 1; 
			Expect(3);
			res.Add(new ActionID(t.val)); SetPos(res, t); 
			while (la.kind == 8) {
				Get();
				Expect(3);
				res.Add(new ActionID(t.val)); 
			}
			Expect(9);
		} else SynErr(30);
	}

	void ArithmeticExpression(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
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

	void PlusMinusTerm(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 18 || la.kind == 22 || la.kind == 23) {
			if (la.kind == 22) {
				Get();
				op = ArithmeticBinOp.Multiply; 
			} else if (la.kind == 18) {
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

	void UnaryMinusTerm(out ArithmeticExpression aexp) {
		bool isMinus = false; Token minusToken = null; aexp = null; 
		if (la.kind == 21) {
			Get();
			isMinus = true; minusToken = t; 
		}
		if (la.kind == 14) {
			Get();
			ArithmeticExpression(out aexp);
			Expect(15);
			aexp.ParenCount += 1; 
		} else if (la.kind == 5) {
			Get();
			aexp = new Constant(int.Parse(t.val)); SetPos(aexp, t); 
		} else if (la.kind == 10) {
			Get();
			aexp = new Constant(int.Parse(t.val)); SetPos(aexp, t);
		} else if (la.kind == 3) {
			Get();
			aexp = new Variable(t.val); SetPos(aexp, t); 
		} else SynErr(31);
		if (isMinus) {aexp = new UnaryMinus(aexp); SetPos(aexp, minusToken);} 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		CCS();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

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
			case 6: s = "\"=\" expected"; break;
			case 7: s = "\"{\" expected"; break;
			case 8: s = "\",\" expected"; break;
			case 9: s = "\"}\" expected"; break;
			case 10: s = "\"0\" expected"; break;
			case 11: s = "\"+\" expected"; break;
			case 12: s = "\"|\" expected"; break;
			case 13: s = "\".\" expected"; break;
			case 14: s = "\"(\" expected"; break;
			case 15: s = "\")\" expected"; break;
			case 16: s = "\"t\" expected"; break;
			case 17: s = "\"[\" expected"; break;
			case 18: s = "\"/\" expected"; break;
			case 19: s = "\"]\" expected"; break;
			case 20: s = "\"\\\\\" expected"; break;
			case 21: s = "\"-\" expected"; break;
			case 22: s = "\"*\" expected"; break;
			case 23: s = "\"%\" expected"; break;
			case 24: s = "??? expected"; break;
			case 25: s = "invalid ProcessConstantDef"; break;
			case 26: s = "invalid Subscript"; break;
			case 27: s = "invalid ActionPrefix"; break;
			case 28: s = "invalid Action"; break;
			case 29: s = "invalid ProcessConstantInvoke"; break;
			case 30: s = "invalid Restriction"; break;
			case 31: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}