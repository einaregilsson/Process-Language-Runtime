﻿using System;
using System.Collections;
using System.Text;
using PLR.AST;
using StringList = System.Collections.Generic.List<string>;
using PLR.AST.Expressions;
using PLR.AST.Processes;
using PLR.AST.Actions;
using Action = PLR.AST.Actions.Action;
using PLR.AST.ActionHandling;
namespace CCS.Formatters {

    public class SourceFormatter {

        #region Utility methods
        private int _highlightID = -1;
        public int HighlightID
        {
            get { return _highlightID; }
            set { _highlightID = value; }
        }
        protected String Join(string sep, IEnumerable items) {
            StringBuilder builder = new StringBuilder();
            foreach (object o in items) {
                if (o is Node) {
                    builder.Append(Format((Node)o));
                } else {
                    builder.Append(o.ToString());
                }
                builder.Append(sep);
            }
            string result = builder.ToString();
            if (result.EndsWith(sep)) {
                return result.Substring(0, result.Length - sep.Length);
            }
            return result;
        }

        protected static string SurroundWithParens(string s, int count) {
            return new String('(', count) + s + new String(')', count);
        }

        #endregion

        #region Format methods
        
        public virtual string Format(ProcessSystem sys) {
            return Join("\n", sys);
        }

        public virtual string Format(ProcessDefinition def) {
            return (def.EntryProc ? "->" : "") + def.Name + Format(def.Variables) + " = " + Format(def.Process);
        }

        public virtual string Format(ProcessConstant pc) {
                return SurroundWithParens(pc.Name+Format(pc.Expressions), pc.ParenCount) + Format(pc.PreProcessActions) + Format(pc.ActionRestrictions);
        }

        public virtual string Format(ExpressionList s) {
            if (s.Count == 0) {
                return "";
            } else {
                return "(" + Join(",", s) + ")";
            }
        }

        public virtual string Format(NonDeterministicChoice ndc) {
            return SurroundWithParens(Join("+", ndc), ndc.ParenCount) + Format(ndc.PreProcessActions) + Format(ndc.ActionRestrictions);
        }

        public virtual string Format(ParallelComposition pc) {
            return SurroundWithParens(Join(" | ", pc), pc.ParenCount) + Format(pc.PreProcessActions) + Format(pc.ActionRestrictions);
        }

        public virtual string Format(ActionPrefix ap) {
            return SurroundWithParens(Format(ap.Action) + "." + Format(ap.Process), ap.ParenCount);
        }

        public virtual string Format(NilProcess np) {
            return "0";
        }

        public virtual string Format(Action act) {
            string result = null;
            if (act is InAction) {
                result = Format((InAction)act);
            } else if (act is OutAction) {
                result = Format((OutAction)act);
            }
            else
            {
                return "ERROR: UNKNOWN ACTION";
            }
            return result;
            
        }

        public virtual string Format(InAction act) {
            return act.Name;
        }

        public virtual string Format(OutAction act) {
            return "_" + act.Name + "_";
        }

        public virtual string Format(PreProcessActions labels) {
            if (labels.Count == 0) {
                return "";
            } else {
                StringList temp = new StringList();
                foreach (object id in labels) {
                    temp.Add(id.ToString());
                }
                return "[" + Join(", ", temp) + "]";
            }
        }

        public virtual string Format(ActionRestrictions res) {
            if (res.HasParens) {
                return " \\{" + Join(", ", res) + '}';
            } else if (res.Count == 1 && !res.HasParens) {
                return " \\" + res[0];
            } else if (res.Count == 0) {
                return "";
            } else {
                throw new Exception("Should never happen!");
            }
        }

        public virtual string Format(Node node) {
            if (node is ProcessSystem) {
                return Format((ProcessSystem)node);
            } else if (node is Process) {
                return Format((Process)node);
            } else if (node is ProcessDefinition) {
                return Format((ProcessDefinition)node);
            } else if (node is PreProcessActions) {
                return Format((PreProcessActions)node);
            } else if (node is ActionRestrictions) {
                return Format((ActionRestrictions)node);
            } else if (node is ArithmeticExpression) {
                return Format((ArithmeticExpression)node);
            } else if (node is ExpressionList) {
                return Format((ExpressionList)node);
            }
            return "ERROR: UNKNOWN NODE";
        }

        public virtual string Format(Process p)
        {
            return Format(p, -1);
        }

        public virtual string Format(Process p, int highlightActionID) {
            if (highlightActionID != -1)
            {
                _highlightID = highlightActionID;
            }
            if (p is ActionPrefix) {
                return Format((ActionPrefix)p);
            } else if (p is NilProcess) {
                return Format((NilProcess)p);
            } else if (p is ProcessConstant) {
                return Format((ProcessConstant)p);
            } else if (p is NonDeterministicChoice) {
                return Format((NonDeterministicChoice)p);
            } else if (p is ParallelComposition) {
                return Format((ParallelComposition)p);
            }
            return "ERROR: UNKNOWN PROCESS";
        }

        public virtual string Format(ArithmeticExpression aexp) {
            if (aexp is ArithmeticBinOpExpression) {
                return Format((ArithmeticBinOpExpression)aexp);
            } else if (aexp is UnaryMinus) {
                return Format((UnaryMinus)aexp);
            } else if (aexp is Number) {
                return Format((Number)aexp);
            } else if (aexp is Variable) {
                return Format((Variable)aexp);
            }
            return "ERROR: UNKNOWN ARITHMETICEXPRESSION";
        }

        public virtual string Format(ArithmeticBinOpExpression exp) {
            char op = 'E';
            if (exp.Op == ArithmeticBinOp.Divide) {
                op = '/';
            } else if (exp.Op == ArithmeticBinOp.Minus) {
                op = '-';
            } else if (exp.Op == ArithmeticBinOp.Modulo) {
                op = '%';
            } else if (exp.Op == ArithmeticBinOp.Multiply) {
                op = '*';
            } else if (exp.Op == ArithmeticBinOp.Plus) {
                op = '+';
            }
            return SurroundWithParens(Format(exp.Left) + op + Format(exp.Right), exp.ParenCount);
        }

        public virtual string Format(UnaryMinus um) {
            return SurroundWithParens("-" + Format(um.Expression), um.ParenCount);
        }

        public virtual string Format(Number c) {
            return SurroundWithParens(c.ToString(), c.ParenCount);
        }

        public virtual string Format(Variable v) {
            return SurroundWithParens(v.Name, v.ParenCount);
        }
        #endregion
    }
}
