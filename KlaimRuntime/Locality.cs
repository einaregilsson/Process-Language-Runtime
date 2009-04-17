using System;
using System.Collections.Generic;
using System.Text;

namespace KlaimRuntime {

    public class Locality {

        private List<Tuple> _tuples = new List<Tuple>();
        private string _name;
        public string Name {
            get { return _name; }
        }

        public static Type Type { get { return typeof(Locality); } }

        public Locality(string name) {
            _name = name;
        }

        public List<Tuple> Tuples {
            get { return _tuples; }
        }

        public void AddTuple(object[] items) {
            _tuples.Add(new Tuple(items));
        }

        public bool ContainsMatchingTuple(object[] items) {
            return _tuples.TrueForAll(delegate(Tuple t) {
                return !t.Matches(items);
            });
        }

        public Tuple In(object[] items) {
            Tuple t = GetRandomTuple(items);
            _tuples.Remove(t);
            return t;
        }

        public Tuple Read(object[] items) {
            return GetRandomTuple(items);
        }

        private Tuple GetRandomTuple(object[] items) {
            List<Tuple> candidates = _tuples.FindAll(delegate(Tuple t) {
                return t.Matches(items);
            });
            if (candidates.Count == 0) {
                throw new KlaimException("No matching tuple found at " + _name);
            }
            return candidates[new Random().Next(candidates.Count)];
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name).Append("\n");
            foreach (Tuple t in Tuples) {
                builder.Append("\t").Append(t.ToString()).Append("\n");
            }
            return builder.ToString();
        }
    }
}
