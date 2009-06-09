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
using PLR.AST.ActionHandling;
using System.Collections.Generic;
using System.Text;

namespace PLR.AST.Formatters {

    public class HtmlFormatter : BaseFormatter {

        public override void Visit(ProcessSystem sys) {
            Return("<table>\n\t<tr>" + Join("</tr>\n\t<tr>", PopChildren()) + "\n\t</tr>\n</table>");
        }

        public override void Visit(ProcessDefinition node) {
            List<string> children = PopChildren();
            Return("\n\t\t<td><b>"+node.Name + children[0] + "</b></td><td>=</td><td>" + children[1]+"</td>");
        }

        public override void Visit(RelabelActions actions) {
            PopChildren();
            StringBuilder sb = new StringBuilder();
            foreach (string key in actions.Mapping.Keys) {
                sb.Append(actions.Mapping[key] + "/" + key + ", ");
            }
            sb.Remove(sb.Length - 2, 2);
            Return("<sub>[" + sb.ToString() + "]</sub>");
        }

        public override void Visit(CustomPreprocess preprocess) {
            PopChildren();
            Return("<sub>[:" + preprocess.MethodName + "]</sub>");
        }

        public override void Visit(OutAction act) {
            List<string> children = PopChildren();
            string text = act.Name;
            if (children.Count > 0) {
                text += "(" + Join(", ", children) + ")";
            }
            Return("<span style=\"text-decoration:overline;\">" + text + "</span>");
        }
    }
}
