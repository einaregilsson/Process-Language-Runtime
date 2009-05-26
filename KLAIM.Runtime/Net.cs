/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace KLAIM.Runtime {
    public static class Net {

        private static SortedList<string, Locality> _localities = new SortedList<string, Locality>();

        public static Locality GetLocality(string name) {
            if (!_localities.ContainsKey(name)) {
                AddLocality(name);
            }
            return _localities[name];
        }

        public static Locality AddLocality(string name) {
            Locality l = new Locality(name);
            _localities.Add(name, l);
            return l;
        }

        public static string Display() {
            StringBuilder sb = new StringBuilder();
            foreach (Locality loc in _localities.Values) {
                sb.Append(loc).Append("\n");
            }
            return sb.ToString();
        }
    }
}
