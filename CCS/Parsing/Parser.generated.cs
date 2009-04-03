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
        n.LexicalInfo.StartLine = t.line;
        n.LexicalInfo.StartColumn = t.col;
        n.LexicalInfo.EndLine = t.line;
        n.LexicalInfo.EndColumn = t.col+t.val.Length;
    }

    private void SetPos(Node n, Token start, Token end) {
        n.LexicalInfo.StartLine = start.line;
        n.LexicalInfo.StartColumn = start.col;
        n.LexicalInfo.EndLine = end.line;
        n.LexicalInfo.EndColumn = end.col+end.val.Length;
    }
    
    private void SetStartPos(Node n, Token t) {
        n.LexicalInfo.StartLine = t.line;
        n.LexicalInfo.StartColumn = t.col;
    }

    private void SetEndPos(Node n, Token t) {
        n.LexicalInfo.EndLine = t.line;
        n.LexicalInfo.EndColumn = t.col;
    }
    
    private void CopyPos(Node n, Node source, Token end) {
        n.LexicalInfo.StartLine = source.LexicalInfo.StartLine;
        n.LexicalInfo.StartColumn = source.LexicalInfo.StartColumn;
        n.LexicalInfo.EndLine = t.line;
        n.LexicalInfo.EndColumn = t.col;
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
		while (la.kind == 1) {
			ProcessDefinition(out proc, false);
			this.System.Add(proc); 
		}
		CopyPos(this.System, this.System[0], t); 
	}

	void ClassImport() {
		Expect(9);
		Expect(4);
		this.System.AddImport(t.val); 
	}

	void ProcessDefinition(out ProcessDefinition procdef, bool entryProc) {
		Process proc; string name; List<Variable> vars = new List<Variable>(); 
		Expect(1);
		name = t.val; Token startTok = t;
		if (la.kind == 10) {
			Get();
			Expect(3);
			vars.Add(new Variable(t.val)); 
			while (la.kind == 11) {
				Get();
				Expect(3);
				vars.Add(new Variable(t.val)); 
			}
			Expect(12);
		}
		Token endTok = t; 
		Expect(13);
		Token startProc = la; 
		Process(out proc);
		procdef = new ProcessDefinition(proc, name, entryProc); SetPos(procdef, startTok, endTok);
		foreach (Variable v in vars) { procdef.Variables.Add(v); }
		SetPos(proc, startProc, t);
		
	}

	void Process(out Process proc) {
		NonDeterministicChoice(out proc);
	}

	void NonDeterministicChoice(out Process proc) {
		Process pc; NonDeterministicChoice ndc = new NonDeterministicChoice();
		ParallelComposition(out pc);
		ndc.Add(pc); 
		while (la.kind == 14) {
			Get();
			ParallelComposition(out pc);
			ndc.Add(pc); 
		}
		if (ndc.Count == 1) {proc = ndc[0]; }else {proc = ndc; CopyPos(proc,ndc[0], t);}
	}

	void ParallelComposition(out Process proc) {
		Process ap; ParallelComposition pc = new ParallelComposition();
		ActionPrefix(out ap);
		pc.Add(ap); 
		while (la.kind == 15) {
			Get();
			ActionPrefix(out ap);
			pc.Add(ap); 
		}
		if (pc.Count == 1) proc = pc[0]; else {proc = pc; CopyPos(pc[0],proc,t);}
	}

	void ActionPrefix(out Process proc) {
		ActionPrefix ap = null; ActionPrefix prev = null; ActionPrefix first = null; Process nextProc = null; ProcessConstant pc; proc = null; Action act = null; 
		while (la.kind == 3 || la.kind == 5 || la.kind == 6) {
			Token startAction = la; 
			Action(out act);
			ap = new ActionPrefix(act, null); SetStartPos(ap,startAction); if (first == null) first = ap; if (prev != null) { prev.Process = ap;} prev = ap;
			SetPos(act, startAction, t); 
			Expect(16);
		}
		if (la.kind == 10) {
			Get();
			Process(out nextProc);
			Expect(12);
			nextProc.ParenCount++; 
		} else if (la.kind == 17) {
			Get();
			nextProc = new NilProcess(); SetPos(nextProc, t);
		} else if (la.kind == 1) {
			ProcessConstantInvoke(out pc);
			nextProc = pc; 
		} else SynErr(28);
		if (first == null) proc = nextProc; else {ap.Process = nextProc; proc = first;}; 
		if (la.kind == 18) {
			PreProcessActions ppa = null; 
			Relabelling(out ppa);
			nextProc.PreProcessActions = ppa; 
		}
		if (la.kind == 21) {
			ActionRestrictions ar = null; 
			Restriction(out ar);
			nextProc.ActionRestrictions = ar; 
		}
	}

	void Action(out Action act) {
		act = null; 
		if (la.kind == 3) {
			Get();
			InAction inAct = new InAction(t.val); 
			if (la.kind == 10) {
				Get();
				Expect(3);
				inAct.AddVariable(new Variable(t.val)); 
				while (la.kind == 11) {
					Get();
					Expect(3);
					inAct.AddVariable(new Variable(t.val)); 
				}
				Expect(12);
			}
			act = inAct; 
		} else if (la.kind == 5) {
			Get();
			OutAction outAct = new OutAction(t.val);  ArithmeticExpression exp; 
			if (la.kind == 10) {
				Get();
				ArithmeticExpression(out exp);
				outAct.AddExpression(exp); 
				while (la.kind == 11) {
					Get();
					ArithmeticExpression(out exp);
					outAct.AddExpression(exp); 
				}
				Expect(12);
			}
			act = outAct;  
		} else if (la.kind == 6) {
			Get();
			List<object> list = new List<object>(); Expression exp = null; string methodName = t.val.Replace(":","");  
			Expect(10);
			if (StartOf(1)) {
				CallParam(out exp);
				list.Add(exp); 
				while (la.kind == 11) {
					Get();
					CallParam(out exp);
					list.Add(exp); 
				}
			}
			Expect(12);
			act = new Call(new MethodCallExpression(methodName, list.ToArray())); 
		} else SynErr(29);
	}

	void ProcessConstantInvoke(out ProcessConstant pc) {
		pc = null; ArithmeticExpression exp = null;
		Expect(1);
		pc = new ProcessConstant(t.val); SetPos(pc, t); 
		if (la.kind == 10) {
			Get();
			ArithmeticExpression(out exp);
			pc.Expressions.Add(exp); 
			while (la.kind == 11) {
				Get();
				ArithmeticExpression(out exp);
				pc.Expressions.Add(exp); 
			}
			Expect(12);
		}
	}

	void Relabelling(out PreProcessActions preproc) {
		preproc = null; string relabelTo, relabelFrom; RelabelActions labels = new RelabelActions(); 
		Expect(18);
		Token first = t; 
		if (la.kind == 6) {
			Get();
			preproc = new CustomPreprocess(t.val.Replace(":","")); SetPos(preproc, first);
		} else if (la.kind == 3) {
			Get();
			relabelTo = t.val; SetPos(labels, first);
			Expect(19);
			Expect(3);
			relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			while (la.kind == 11) {
				Get();
				Expect(3);
				relabelTo = t.val; 
				Expect(19);
				Expect(3);
				relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			}
			preproc = labels; 
		} else SynErr(30);
		Expect(20);
	}

	void Restriction(out ActionRestrictions ar) {
		ar = null; ChannelRestrictions res = new ChannelRestrictions(); 
		Expect(21);
		if (la.kind == 3) {
			Get();
			res.Add(t.val); SetPos(res, t); res.ParenCount = 0; ar = res; 
		} else if (la.kind == 22) {
			Get();
			res.ParenCount = 1; 
			Expect(3);
			res.Add(t.val); SetPos(res, t); 
			while (la.kind == 11) {
				Get();
				Expect(3);
				res.Add(t.val); 
			}
			Expect(23);
			ar = res; 
		} else if (la.kind == 6) {
			Get();
			ar = new CustomRestrictions(t.val.Replace(":", "")); SetPos(ar, t); 
		} else SynErr(31);
	}

	void ArithmeticExpression(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		PlusMinusTerm(out left);
		aexp = left; 
		while (la.kind == 14 || la.kind == 24) {
			if (la.kind == 14) {
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
		} else SynErr(32);
	}

	void PlusMinusTerm(out ArithmeticExpression aexp) {
		ArithmeticBinOp op; ArithmeticExpression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 19 || la.kind == 25 || la.kind == 26) {
			if (la.kind == 25) {
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
			aexp = new ArithmeticBinOpExpression(aexp, right, op); CopyPos(((ArithmeticBinOpExpression)aexp).Left,aexp,t); 
		}
	}

	void UnaryMinusTerm(out ArithmeticExpression aexp) {
		bool isMinus = false; Token minusToken = null; aexp = null; 
		if (la.kind == 24) {
			Get();
			isMinus = true; minusToken = t; 
		}
		if (la.kind == 10) {
			Get();
			ArithmeticExpression(out aexp);
			Expect(12);
			aexp.ParenCount += 1; 
		} else if (la.kind == 7) {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t); 
		} else if (la.kind == 17) {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t);
		} else if (la.kind == 3) {
			Get();
			aexp = new Variable(t.val); SetPos(aexp, t); 
		} else SynErr(33);
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
		{x,x,x,T, x,x,x,T, T,x,T,x, x,x,x,x, x,T,x,x, x,x,x,x, T,x,x,x, x},
		{x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,x, x,T,x,x, x,x,x,x, T,x,x,x, x}

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
			case 10: s = "\"(\" expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\")\" expected"; break;
			case 13: s = "\"=\" expected"; break;
			case 14: s = "\"+\" expected"; break;
			case 15: s = "\"|\" expected"; break;
			case 16: s = "\".\" expected"; break;
			case 17: s = "\"0\" expected"; break;
			case 18: s = "\"[\" expected"; break;
			case 19: s = "\"/\" expected"; break;
			case 20: s = "\"]\" expected"; break;
			case 21: s = "\"\\\\\" expected"; break;
			case 22: s = "\"{\" expected"; break;
			case 23: s = "\"}\" expected"; break;
			case 24: s = "\"-\" expected"; break;
			case 25: s = "\"*\" expected"; break;
			case 26: s = "\"%\" expected"; break;
			case 27: s = "??? expected"; break;
			case 28: s = "invalid ActionPrefix"; break;
			case 29: s = "invalid Action"; break;
			case 30: s = "invalid Relabelling"; break;
			case 31: s = "invalid Restriction"; break;
			case 32: s = "invalid CallParam"; break;
			case 33: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}