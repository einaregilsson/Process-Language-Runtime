/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
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
	public const int maxT = 41;


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
		if (ndc.Processes.Count == 1) {proc = ndc.Processes[0]; }else {proc = ndc; CopyPos(proc,ndc.Processes[0], t);}
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
		if (pc.Processes.Count == 1) proc = pc.Processes[0]; else {proc = pc; CopyPos(pc.Processes[0],proc,t);}
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
		} else if (la.kind == 18) {
			BranchProcess(out nextProc);
		} else SynErr(42);
		if (first == null) proc = nextProc; else {ap.Process = nextProc; proc = first;}; 
		if (la.kind == 21) {
			PreProcessActions ppa = null; 
			Relabelling(out ppa);
			nextProc.PreProcessActions = ppa; 
		}
		if (la.kind == 24) {
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
			OutAction outAct = new OutAction(t.val);  Expression exp; 
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
		} else SynErr(43);
	}

	void ProcessConstantInvoke(out ProcessConstant pc) {
		pc = null; Expression exp = null;
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

	void BranchProcess(out Process proc) {
		Expression exp = null; Process ifProc = null; Process elseProc = null; 
		Expect(18);
		Expression(out exp);
		Expect(19);
		Process(out ifProc);
		Expect(20);
		Process(out elseProc);
		proc = new BranchProcess(exp, ifProc, elseProc); 
	}

	void Relabelling(out PreProcessActions preproc) {
		preproc = null; string relabelTo, relabelFrom; RelabelActions labels = new RelabelActions(); 
		Expect(21);
		Token first = t; 
		if (la.kind == 6) {
			Get();
			preproc = new CustomPreprocess(t.val.Replace(":","")); SetPos(preproc, first);
		} else if (la.kind == 3) {
			Get();
			relabelTo = t.val; SetPos(labels, first);
			Expect(22);
			Expect(3);
			relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			while (la.kind == 11) {
				Get();
				Expect(3);
				relabelTo = t.val; 
				Expect(22);
				Expect(3);
				relabelFrom = t.val; labels.Add(relabelFrom, relabelTo); 
			}
			preproc = labels; 
		} else SynErr(44);
		Expect(23);
	}

	void Restriction(out ActionRestrictions ar) {
		ar = null; ChannelRestrictions res = new ChannelRestrictions(); 
		Expect(24);
		if (la.kind == 3) {
			Get();
			res.Add(t.val); SetPos(res, t); res.ParenCount = 0; ar = res; 
		} else if (la.kind == 25) {
			Get();
			res.ParenCount = 1; 
			Expect(3);
			res.Add(t.val); SetPos(res, t); 
			while (la.kind == 11) {
				Get();
				Expect(3);
				res.Add(t.val); 
			}
			Expect(26);
			ar = res; 
		} else if (la.kind == 6) {
			Get();
			ar = new CustomRestrictions(t.val.Replace(":", "")); SetPos(ar, t); 
		} else SynErr(45);
	}

	void Expression(out Expression exp) {
		Expression right = null, left = null; 
		OrTerm(out left);
		exp = left; 
		while (la.kind == 27) {
			Get();
			OrTerm(out right);
			exp = new LogicalBinOpExpression(exp, right, LogicalBinOp.Or); CopyPos(exp, ((LogicalBinOpExpression)exp).Left,t);
		}
	}

	void ArithmeticExpression(out Expression aexp) {
		ArithmeticBinOp op; Expression right = null, left = null; 
		PlusMinusTerm(out left);
		aexp = left; 
		while (la.kind == 14 || la.kind == 36) {
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
		Expression aexp = null; exp = null;
		if (StartOf(2)) {
			ArithmeticExpression(out aexp);
			exp = aexp; 
		} else if (la.kind == 8) {
			Get();
			exp = new PLRString(t.val.Substring(1, t.val.Length-2)); SetPos(exp, t);
		} else SynErr(46);
	}

	void OrTerm(out Expression exp) {
		Expression right = null, left = null; 
		AndTerm(out left);
		exp = left; 
		while (la.kind == 28) {
			Get();
			AndTerm(out right);
			exp = new LogicalBinOpExpression(exp, right, LogicalBinOp.And); CopyPos(exp, ((LogicalBinOpExpression)exp).Left,t);
		}
	}

	void AndTerm(out Expression exp) {
		Expression right = null, left = null; 
		RelationalTerm(out left);
		exp = left; 
		while (la.kind == 29) {
			Get();
			RelationalTerm(out right);
			exp = new LogicalBinOpExpression(exp, right, LogicalBinOp.Xor); CopyPos(exp, ((LogicalBinOpExpression)exp).Left,t);
		}
	}

	void RelationalTerm(out Expression exp) {
		Expression right = null, left = null; RelationalBinOp op = RelationalBinOp.Equal;
		ArithmeticExpression(out left);
		exp = left; 
		if (StartOf(3)) {
			switch (la.kind) {
			case 30: {
				Get();
				op = RelationalBinOp.Equal; 
				break;
			}
			case 31: {
				Get();
				op = RelationalBinOp.NotEqual; 
				break;
			}
			case 32: {
				Get();
				op = RelationalBinOp.GreaterThan; 
				break;
			}
			case 33: {
				Get();
				op = RelationalBinOp.GreaterThanOrEqual; 
				break;
			}
			case 34: {
				Get();
				op = RelationalBinOp.LessThan; 
				break;
			}
			case 35: {
				Get();
				op = RelationalBinOp.LessThanOrEqual; 
				break;
			}
			}
			ArithmeticExpression(out right);
			exp = new RelationalBinOpExpression(exp, right, op); CopyPos(exp, ((RelationalBinOpExpression)exp).Left,t);
		}
	}

	void PlusMinusTerm(out Expression aexp) {
		ArithmeticBinOp op; Expression right = null, left = null; 
		UnaryMinusTerm(out left);
		aexp = left; 
		while (la.kind == 22 || la.kind == 37 || la.kind == 38) {
			if (la.kind == 37) {
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
		if (la.kind == 36) {
			Get();
			isMinus = true; minusToken = t; 
		}
		switch (la.kind) {
		case 10: {
			Get();
			ArithmeticExpression(out aexp);
			Expect(12);
			aexp.ParenCount += 1; 
			break;
		}
		case 7: {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t); 
			break;
		}
		case 17: {
			Get();
			aexp = new Number(int.Parse(t.val)); SetPos(aexp, t);
			break;
		}
		case 39: {
			Get();
			aexp = new Bool(true); 
			break;
		}
		case 40: {
			Get();
			aexp = new Bool(false); 
			break;
		}
		case 3: {
			Get();
			aexp = new Variable(t.val); SetPos(aexp, t); 
			break;
		}
		default: SynErr(47); break;
		}
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,T, x,x,x,T, T,x,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x},
		{x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x}

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
			case 18: s = "\"if\" expected"; break;
			case 19: s = "\"then\" expected"; break;
			case 20: s = "\"else\" expected"; break;
			case 21: s = "\"[\" expected"; break;
			case 22: s = "\"/\" expected"; break;
			case 23: s = "\"]\" expected"; break;
			case 24: s = "\"\\\\\" expected"; break;
			case 25: s = "\"{\" expected"; break;
			case 26: s = "\"}\" expected"; break;
			case 27: s = "\"or\" expected"; break;
			case 28: s = "\"and\" expected"; break;
			case 29: s = "\"xor\" expected"; break;
			case 30: s = "\"==\" expected"; break;
			case 31: s = "\"!=\" expected"; break;
			case 32: s = "\">\" expected"; break;
			case 33: s = "\">=\" expected"; break;
			case 34: s = "\"<\" expected"; break;
			case 35: s = "\"<=\" expected"; break;
			case 36: s = "\"-\" expected"; break;
			case 37: s = "\"*\" expected"; break;
			case 38: s = "\"%\" expected"; break;
			case 39: s = "\"true\" expected"; break;
			case 40: s = "\"false\" expected"; break;
			case 41: s = "??? expected"; break;
			case 42: s = "invalid ActionPrefix"; break;
			case 43: s = "invalid Action"; break;
			case 44: s = "invalid Relabelling"; break;
			case 45: s = "invalid Restriction"; break;
			case 46: s = "invalid CallParam"; break;
			case 47: s = "invalid UnaryMinusTerm"; break;

        }
        return s;
    }
} // Errors

}