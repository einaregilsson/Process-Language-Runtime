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
    public class Tuple {
        private List<object> _items = new List<object>();

        //These are needed to notify when a tuple is removed from a tuple space...
        public int GeneratingActionNr { get; set; }
        public IActionSubscriber Subscriber { get; set; }

        public Tuple(object[] items) {
            _items.AddRange(items);
        }

        /// <summary>
        /// Returns true if the tuple matches the items given.
        /// Null matches everything and can be used for things like
        /// variable bindings.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
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

        public object GetValueAt(int index) {
            return _items[index];               
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
