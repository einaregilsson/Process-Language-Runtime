using PLR.AST;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using PLR.AST.Actions;
using PLR.AST.ActionHandling;
using Action = PLR.AST.Actions.Action;
using System.Collections.Generic;

using System;

namespace CCS.Parsing {



public partial class Parser {
	public const int _EOF = 0;
	public const int _PROCNAME = 1;
	public const int _PROCNAMESUB = 2;
	public const int _LCASEIDENT = 3;
	public const int _CLASSNAME = 4;
	public const int _OUTACTION = 5;
	public const int _METHOD = 6;
	public const int _NUMBER = 7;
	public const int _STRING = 8;
	public const int maxT = 27;


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
		while (la.kind == 9) {
			ClassImport();
		}
		ProcessDefinition proc; 
		ProcessDefinition(out proc, true);
		this.System.Add(proc); 
		while (la.kind == 1 || la.kind == 2) {
			ProcessDefinition(out proc, false);
			this.System.Add(proc); 
		}
		CopyPos(this.System[0],this.System, t); 
	}

	void ClassImport() {
		Expect(9);
		Expect(4);
		this.System.AddImport(t.val); 
	}

	void ProcessDefinition(out ProcessDefinition procdef, bool entryProc) {
		Process proc; ProcessConstant pc; 
		ProcessConstantDef(out pc);
		Expect(10);
		Process(out proc);
		procdef = new ProcessDefinition(pc, proc, entryProc); CopyPos(pc,procdef,t); 
	}

	void ProcessConstantDef(out ProcessConstant pc) {
		pc = null; 
		if (la.kind == 1) {
			Get();
			pc = new ProcessConstant(t.val); SetPos(pc, t);
		} else if (la.kind == 2) {
			Get();
			pc = new ProcessConstant(t.val.Replace("_",""));  SetPos(pc,t); ArithmeticExpression subscript; 
			Expect(11);
			Token startToken = t; 
			Subscript(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 12) {
				Get();
				Subscript(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(13);
			SetPos(pc.Subscript,startToken); pc.Subscript.Length = t.pos - pc.Subscript.Position; 
		} else SynErr(28);
	}

	void Process(out Process proc) {
		NonDeterministicChoice(out proc);
	}

	void Subscript(out ArithmeticExpression sub) {
		sub = null; 
		if (la.kind == 3) {
			Get();
			sub = new Variable(t.val); SetPos(sub, t); 
		} else if (la.kind == 7) {
			Get();
		} else if (la.kind == 14) {
			Get();
			sub = new Number(int.Parse(t.val)); SetPos(sub, t); 
		} else SynErr(29);
	}

	void NonDeterministicChoice(out Process proc) {
		Process pc; NonDeterministicChoice ndc = new NonDeterministicChoice();
		ParallelComposition(out pc);
		ndc.Add(pc); 
		while (la.kind == 15) {
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
		while (la.kind == 16) {
			Get();
			ActionPrefix(out ap);
			pc.Add(ap); 
		}
		if (pc.Count == 1) proc = pc[0]; else {proc = pc; CopyPos(pc[0],proc,t);}
	}

	void ActionPrefix(out Process proc) {
		ActionPrefix ap = null; ActionPrefix prev = null; ActionPrefix first = null; Process nextProc = null; ProcessConstant pc; proc = null; Action act = null; 
		while (la.kind == 3 || la.kind == 5 || la.kind == 6) {
			Action(out act);
			Expect(17);
			ap = new ActionPrefix(act, null); CopyPos(ap.Action,ap,t); if (first == null) first = ap; if (prev != null) { prev.Process = ap;} prev = ap;
		}
		if (la.kind == 18) {
			Get();
			Process(out nextProc);
			Expect(19);
			nextProc.ParenCount++; 
		} else if (la.kind == 14) {
			Get();
			nextProc = new NilProcess(); SetPos(nextProc, t);
		} else if (la.kind == 1 || la.kind == 2) {
			ProcessConstantInvoke(out pc);
			nextProc = pc; 
		} else SynErr(30);
		if (first == null) proc = nextProc; else {ap.Process = nextProc; proc = first;}; 
		if (la.kind == 20) {
			PreProcessActions ppa = null; 
			Relabelling(out ppa);
			nextProc.PreProcessActions = ppa; 
		}
		if (la.kind == 23) {
			ActionRestrictions ar = null; 
			Restriction(out ar);
			nextProc.ActionRestrictions = ar; 
		}
	}

	void Action(out Action act) {
		act = null; 
		if (la.kind == 3) {
			Get();
			act = new InAction(t.val); SetPos(act, t);
		} else if (la.kind == 5) {
			Get();
			act = new OutAction(t.val); SetPos(act, t);
		} else if (la.kind == 6) {
			Get();
			List<object> list = new List<object>(); Expression exp = null; string methodName = t.val.Replace(":",""); Token callStart = t; 
			Expect(18);
			if (StartOf(1)) {
				CallParam(out exp);
				list.Add(exp); 
				while (la.kind == 12) {
					Get();
					CallParam(out exp);
					list.Add(exp); 
				}
			}
			Expect(19);
			act = new Call(new MethodCallExpression(methodName, list.ToArray())); SetPos(act, t); 
		} else SynErr(31);
	}

	void ProcessConstantInvoke(out ProcessConstant pc) {
		pc = null; 
		if (la.kind == 1) {
			Get();
			pc = new ProcessConstant(t.val); SetPos(pc, t); 
		} else if (la.kind == 2) {
			Get();
			pc = new ProcessConstant(t.val.Replace("_",""));  SetPos(pc, t); ArithmeticExpression subscript; 
			Expect(11);
			ArithmeticExpression(out subscript);
			pc.Subscript.Add(subscript); 
			while (la.kind == 12) {
				Get();
				ArithmeticExpression(out subscript);
				pc.Subscript.Add(subscript); 
			}
			Expect(13);
		} else SynErr(32);
	}

	void Relabelling(out PreProcessActions preproc) {
		preproc = null; string relabelTo, relabelFrom; RelabelActions labels = new RelabelActions(); 
		Expect(20);
		Token first = t; 
		if (la.kind == 6) {
			Get();
			preproc = new CustomPreprocess(t.val.Replace(":","")); SetPos(preproc, first);
		} else if (la.kind == 3) {
			Get();
			relabelTo = t.val; SetPos(labels, first);
			Expect(21);
			Expect(3);
			relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			while (la.kind == 12) {
				Get();
				Expect(3);
				relabelTo = t.val; 
				Expect(21);
				Expect(3);
				relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			}
			preproc = labels; 
		} else SynErr(33);
		Expect(22);
	}

	void Restriction(out ActionRestrictions ar) {
		ar = null; ChannelRestrictions res = new ChannelRestrictions(); 
		Expect(23);
		if (la.kind == 3) {
			Get();
			res.Add(t.val); SetPos(res, t); res.ParenCount = 0; ar = res; 
		} else if (la.kind == 11) {
			Get();
			res.ParenCount = 1; 
			Expect(3);
			res.Add(t.val); SetPos(res, t); 
			while (la.kind == 12) {
				Get();
				Expect(3);
				res.Add(t.val); 
			}
			Expect(13);
			ar = res; 
		} else if (la.kind == 6) {
			Get();
			ar = new CustomRestrictions(t.val.Replace(":", "")); SetPos(ar, t); 
		} else SynErr(34);
	}

	void ArithmeticExpression(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		PlusMinusTerm(out left);
		aexp = left; 
		while (la.kind == 15 || la.kind == 24) {
			if (la.kind == 15) {
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

	void CallParam(out Expression exp) {
		ArithmeticExpression aexp = null; exp = null;
		if (StartOf(2)) {
			ArithmeticExpression(out aexp);
			exp = aexp; 
		} else if (la.kind == 8) {
			Get();
			exp = new PLRString(t.val.Substring(1, t.val.Length-2)); SetPos(exp, t);
		} else SynErr(35);
	}

	void PlusMinusTerm(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 21 || la.kind == 25 || la.kind == 26) {
			if (la.kind == 25) {
				Get();
				op = ArithmeticBinOp.Multiply; 
			} else if (la.kind == 21) {
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
		if (la.kind == 24) {
			Get();
			isMinus = true; minusToken = t; 
		}
		if (la.kind == 18) {
			Get();
			ArithmeticExpression(out aexp);
			Expect(19);
			aexp.ParenCount += 1; 
		} else if (la.kind == 7) {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t); 
		} else if (la.kind == 14) {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t);
		} else if (la.kind == 3) {
			Get();
			aexp = new Variable(t.val); SetPos(aexp, t); 
		} else SynErr(36);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,T, x,x,x,T, T,x,x,x, x,x,T,x, x,x,T,x, x,x,x,x, T,x,x,x, x},
		{x,x,x,T, x,x,x,T, x,x,x,x, x,x,T,x, x,x,T,x, x,x,x,x, T,x,x,x, x}

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
			case 4: s = "CLASSNAME expected"; break;
			case 5: s = "OUTACTION expected"; break;
			case 6: s = "METHOD expected"; break;
			case 7: s = "NUMBER expected"; break;
			case 8: s = "STRING expected"; break;
			case 9: s = "\"use\" expected"; break;
			case 10: s = "\"=\" expected"; break;
			case 11: s = "\"{\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "\"}\" expected"; break;
			case 14: s = "\"0\" expected"; break;
			case 15: s = "\"+\" expected"; break;
			case 16: s = "\"|\" expected"; break;
			case 17: s = "\".\" expected"; break;
			case 18: s = "\"(\" expected"; break;
			case 19: s = "\")\" expected"; break;
			case 20: s = "\"[\" expected"; break;
			case 21: s = "\"/\" expected"; break;
			case 22: s = "\"]\" expected"; break;
			case 23: s = "\"\\\\\" expected"; break;
			case 24: s = "\"-\" expected"; break;
			case 25: s = "\"*\" expected"; break;
			case 26: s = "\"%\" expected"; break;
			case 27: s = "??? expected"; break;
			case 28: s = "invalid ProcessConstantDef"; break;
			case 29: s = "invalid Subscript"; break;
			case 30: s = "invalid ActionPrefix"; break;
			case 31: s = "invalid Action"; break;
			case 32: s = "invalid ProcessConstantInvoke"; break;
			case 33: s = "invalid Relabelling"; break;
			case 34: s = "invalid Restriction"; break;
			case 35: s = "invalid CallParam"; break;
			case 36: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}