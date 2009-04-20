using System;
using System.Collections.Generic;
using System.Text;

namespace KlaimRuntime {
    public class Tuple {
        private List<object> _items = new List<object>();

        public Tuple(object[] items) {
            _items.AddRange(items);
        }

        public static Type Type { get { return typeof(Tuple); } }
        public bool Matches(object[] items) {
            if (items.Length != _items.Count) {
                return false;
            }

            for (int i = 0; i < _items.Count; i++) {
                if (items[i] != null && !items[i].Equals(_items[i])) {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() {
            string[] temp = new string[_items.Count];
            for (int i = 0; i < _items.Count; i++) {
                temp[i] = _items[i].ToString();
            }
            return "<" + String.Join(", ", temp) + ">";
        }
    }
}
