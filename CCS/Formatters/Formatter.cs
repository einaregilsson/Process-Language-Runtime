using System;
using System.Collections;
using System.Text;
using CCS.Nodes;
using Action = CCS.Nodes.Action;

namespace CCS.Formatters {

    public class Formatter {

        #region Utility methods
        
        protected String Join(string sep, IEnumerable items) {
            StringBuilder builder = new StringBuilder();
            foreach (object o in items) {
                if (o is ASTNode) {
                    builder.Append(Format((ASTNode)o));
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
        
        public virtual string Format(CCSSystem sys) {
            return Join("\n", sys);
        }

        public virtual string Format(ProcessDefinition def) {
            return (def.EntryProc ? "->" : "") + Format(def.ProcessConstant) + " = " + Format(def.Process);
        }

        public virtual string Format(ProcessConstant pc) {
                return SurroundWithParens(pc.Name+Format(pc.Subscript), pc.ParenCount) + Format(pc.Relabelling) + Format(pc.Restrictions);
        }

        public virtual string Format(Subscript s) {
            if (s.Count == 0) {
                return "";
            } else {
                return "_{" + Join(",", s) + "}";
            }
        }

        public virtual string Format(NonDeterministicChoice ndc) {
            return SurroundWithParens(Join("+", ndc), ndc.ParenCount) + Format(ndc.Relabelling) + Format(ndc.Restrictions);
        }

        public virtual string Format(ParallelComposition pc) {
            return SurroundWithParens(Join(" | ", pc), pc.ParenCount) + Format(pc.Relabelling) + Format(pc.Restrictions);
        }

        public virtual string Format(ActionPrefix ap) {
            return SurroundWithParens(Format(ap.Action) + "." + Format(ap.Process), ap.ParenCount);
        }

        public virtual string Format(NilProcess np) {
            return "0";
        }

        public virtual string Format(Action act) {
            if (act is InAction) {
                return Format((InAction)act);
            } else if (act is OutAction) {
                return Format((OutAction)act);
            } else if (act is TauAction) {
                return Format((TauAction)act);
            }
            return "ERROR: UNKNOWN ACTION";
        }

        public virtual string Format(InAction act) {
            return act.Name;
        }

        public virtual string Format(OutAction act) {
            return "_" + act.Name + "_";
        }

        public virtual string Format(TauAction act) {
            return "t";
        }
        

        public virtual string Format(Relabellings labels) {
            if (labels.Count == 0) {
                return "";
            } else {
                var temp = new System.Collections.Generic.List<string>();
                foreach (string key in labels.Keys) {
                    temp.Add(labels[key] + "/" + key);
                }
                return "[" + Join(", ", temp) + "]";
            }
        }

        public virtual string Format(Restrictions res) {
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

        public virtual string Format(ASTNode node) {
            if (node is CCSSystem) {
                return Format((CCSSystem)node);
            } else if (node is Process) {
                return Format((Process)node);
            } else if (node is ProcessDefinition) {
                return Format((ProcessDefinition)node);
            } else if (node is Relabellings) {
                return Format((Relabellings)node);
            } else if (node is Restrictions) {
                return Format((Restrictions)node);
            } else if (node is ArithmeticExpression) {
                return Format((ArithmeticExpression)node);
            } else if (node is Subscript) {
                return Format((Subscript)node);
            }
            return "ERROR: UNKNOWN NODE";
        }

        public virtual string Format(Process p) {
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
            } else if (aexp is Constant) {
                return Format((Constant)aexp);
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
            return SurroundWithParens("-" + Format(um.Term), um.ParenCount);
        }

        public virtual string Format(Constant c) {
            return SurroundWithParens(c.Value.ToString(), c.ParenCount);
        }

        public virtual string Format(Variable v) {
            return SurroundWithParens(v.Name, v.ParenCount);
        }
        #endregion
    }
}
