using CCS.Nodes;

namespace CCS.Formatters {

    public class LaTeXFormatter : Formatter {

        #region Format methods

        public override string Format(CCSSystem sys) {
            return
@"\documentclass[a4paper,11pt]{article}
\usepackage{amsmath}

%<CCS commands>
\newcommand{\defeq}{\ \stackrel{\mathrm{def}}{=}\ }
\newcommand{\out}[1]{\overline{#1}}
\newcommand{\proc}[1]{\mathrm{#1}}
\newcommand{\paral}{\ |\ }
%</CCS commands>

\begin{document}
  \begin{align*}
"
            +
            Join(" \\\\\n", sys)
            +
@"
  \end{align*}
\end{document}
";

        }

        public override string Format(ProcessDefinition def) {
            return "    \\mathrm{" + Format(def.ProcessConstant) + "} \\defeq & \\mathrm{" + Format(def.Process) + "}";
        }

        public override string Format(ParallelComposition pc) {
            return SurroundWithParens(Join(" \\paral ", pc), pc.ParenCount) + Format(pc.Relabelling) + Format(pc.Restrictions);
        }

        public override string Format(OutAction act) {
            return "\\out{" + act.Name + "}";
        }

        public override string Format(TauAction act) {
            return "\\tau";
        }

        public override string Format(Relabellings labels) {
            string basestring = base.Format(labels);
            if (basestring == "") {
                return "";
            } else {
                return "_{" + basestring + "}";
            }
        }

        #endregion
    }
}