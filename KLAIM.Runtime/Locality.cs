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
using System.Threading;

namespace KLAIM.Runtime {

    public class Locality {

        private List<Tuple> _tuples = new List<Tuple>();
        private string _name;
        public string Name {
            get { return _name; }
        }

        public static Type Type { get { return typeof(Locality); } }

        public Locality(string name) {
            _name = name;
            _tuples = new List<Tuple>();
        }

        public List<Tuple> Tuples {
            get { return _tuples; }
        }

        public void Out(object[] items) {
            _tuples.Add(new Tuple(items));
        }

        public bool ContainsMatchingTuple(object[] items) {
            return _tuples.TrueForAll(delegate(Tuple t) {
                return !t.Matches(items);
            });
        }

        public Tuple In(object[] items) {
            while (true) {
                lock (this) {
                    Tuple t = GetRandomTuple(items);
                    if (t != null) {
                        _tuples.Remove(t);
                        return t;
                    }
                }
                Thread.Sleep(250);
            }
        }

        public Tuple Read(object[] items) {
            while (true) {
                lock (this) {
                    Tuple t = GetRandomTuple(items);
                    if (t != null) {
                        return t;
                    }
                }
                Console.WriteLine("BLOCKED!");
                Thread.Sleep(250);
            }
        }

        private Tuple GetRandomTuple(object[] items) {
            List<Tuple> candidates = _tuples.FindAll(delegate(Tuple t) {
                return t.Matches(items);
            });
            if (candidates.Count == 0) {
                return null;
            }
            return candidates[new Random().Next(candidates.Count)];
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            foreach (Tuple t in Tuples) {
                builder.Append("|| ");
                builder.Append(Name).Append("::");
                builder.Append(t).Append("\n");
            }
            return builder.ToString();
        }
    }
}
