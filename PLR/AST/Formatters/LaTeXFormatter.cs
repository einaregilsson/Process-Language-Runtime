/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using PLR.AST;
using PLR.AST.Actions;
using PLR.AST.Processes;
using PLR.AST.ActionHandling;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST.Formatters {

    public class LaTeXFormatter : BaseFormatter {

        #region Format methods

        public override void Visit(ProcessSystem sys) {
            List<string> children = PopChildren();
            Return(
@"\documentclass[a4paper,11pt]{article}
\usepackage{amsmath}

%<CCS commands>
\newcommand{\defeq}{\ \stackrel{\mathrm{def}}{=}\ }
\newcommand{\proc}[1]{\mathrm{#1}}
\newcommand{\ccsdot}{\ \textbf{.}\ }
%</CCS commands>

\begin{document}
  \begin{align*}
"
            +
            Join(" \\\\\n", children)
            +
@"
  \end{align*}
\end{document}
");

        }


        public override void Visit(ProcessDefinition node) {
            List<string> children = PopChildren();
            Return("     \\mathrm{"+node.Name + children[0] + "} \\defeq & \\mathrm{" + children[1]+ "}");
        }


        public override void Visit(ParallelComposition node) {
            List<string> children = PopChildren();
            List<string> procChildren = new List<string>(children);
            procChildren.RemoveRange(0, 2);
            Return(CheckProc(Join(@" \mid\ ", procChildren), children));
        }

        public override void Visit(OutAction act) {
            List<string> children = PopChildren();
            string text = act.Name;
            if (children.Count > 0) {
                text += "(" + Join(", ", children) + ")";
            }
            Return(@"\overline{" + text + "}");
        }

        public override void Visit(ActionPrefix node) {
            List<string> children = PopChildren();
            Return(CheckProc(children[2] + @" \ccsdot\ " + children[3], children));
        }

        public override void Visit(ChannelRestrictions restrictions) {
            PopChildren();
            if (restrictions.ChannelNames.Count == 0) {
                Return("");
            } else {
                Return(@" \backslash\{" + Join(", ", restrictions.ChannelNames) + @"\}");
            }
        }

        public override void Visit(RelabelActions actions) {
            PopChildren();
            StringBuilder sb = new StringBuilder();
            foreach (string key in actions.Mapping.Keys) {
                sb.Append(actions.Mapping[key] + "/" + key + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            Return("_{[" + sb.ToString() + "]}");
        }

        public override void Visit(CustomPreprocess preprocess) {
            PopChildren();
            Return("_{[:" + preprocess.MethodName + "]}");
        }

        #endregion
    }
}