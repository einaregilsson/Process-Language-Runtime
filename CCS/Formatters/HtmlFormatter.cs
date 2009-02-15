using PLR.AST;
using PLR.AST.Actions;

namespace CCS.Formatters {

    public class HtmlFormatter : Formatter {

        #region Format methods

        public override string Format(ProcessSystem sys) {
            return "<table>\n\t<tr>" + Join("</tr>\n\t<tr>", sys) + "\n\t</tr>\n</table>";
        }

        public override string Format(ProcessDefinition def) {
            return "\n\t\t<td>" + Format(def.ProcessConstant) + "</td>\n\t\t<td>=</td>\n\t\t<td>" + Format(def.Process) + "</td>";
        }

        public override string Format(Subscript s) {
            if (s.Count == 0) {
                return "";
            } else {
                return "<sub>" + Join(",", s) + "</sub>";
            }
        }

        public override string Format(Relabellings labels) {
            string basestring = base.Format(labels);
            if (basestring == "") {
                return "";
            } else {
                return "<sub>" + basestring + "</sub>";
            }
        }

        public override string Format(OutAction act) {
            return "<span style=\"text-decoration:overline;\">" + act.Name + "</span>";
        }

        public override string Format(TauAction act) {
            return "&tau;";
        }

        #endregion
    }
}
