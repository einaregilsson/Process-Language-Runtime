/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections;
using System.Text;

namespace PLR {
    public static class Util {
        public static string Join(string sep, IEnumerable items) {
            StringBuilder sb = new StringBuilder();
            foreach (object o in items) {
                if (o == null) {
                    sb.Append("null");
                }else {
                    sb.Append(o.ToString());
                }
                sb.Append(sep);
            }
            if (sb.ToString() == "") {
                return "";
            } else {
                return sb.ToString().Substring(0, sb.ToString().Length - sep.Length);
            }
            
        }
    }
}
