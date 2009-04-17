using System;
using System.Collections.Generic;
using System.Text;

namespace KlaimRuntime {
    public static class Net {

        private static List<Locality> _localities = new List<Locality>();

        public static Locality GetLocality(string name) {
            return _localities.Find(delegate(Locality l) { return l.Name == name; });
        }

        public static Type Type { get { return typeof(Net); } }

        public static Locality AddLocality(string name) {
            Locality l = new Locality(name);
            _localities.Add(l);
            return l;
        }
    }
}
