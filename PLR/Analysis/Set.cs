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

namespace PLR.Analysis {

    public class Set : IEnumerable {
        #region Members
        //Only reason it's sorted is so the ToString will look nicer...
        private Hashtable _items = new Hashtable();
        #endregion

        #region Constructors
        public Set() {
        }

        public void Clear() {
            _items.Clear();
        }

        public Set(IEnumerable range) {
            AddRange(range);
        }
        #endregion

        #region Add/Remove operations
        public void Add(Object obj) {
            if (!_items.ContainsKey(obj)) {
                _items.Add(obj, null);
            }
        }

        public void AddRange(IEnumerable range) {
            foreach (object o in range) {
                Add(o);
            }
        }

        public void Remove(object obj) {
            if (_items.ContainsKey(obj)) {
                _items.Remove(obj);
            }
        }

        public void RemoveRange(IEnumerable range) {
            foreach (object o in range) {
                Remove(o);
            }
        }

        #endregion

        #region Queries
        public bool IsEmpty {
            get { return this.Count == 0; }
        }

        public int Count {
            get { return _items.Count; }
        }

        public bool Contains(object obj) {
            return _items.ContainsKey(obj);
        }
        #endregion

        #region Set operations
        public Set Union(Set set) {
            Set result = new Set();
            result.AddRange(this);
            result.AddRange(set);
            return result;
        }

        public Set Intersection(Set set) {
            Set result = new Set();
            foreach (object o in this) {
                if (set.Contains(o)) {
                    result.Add(o);
                }
            }
            return result;
        }

        public Set Difference(Set set) {
            Set result = new Set();
            foreach (object o in this) {
                if (!set.Contains(o)) {
                    result.Add(o);
                }
            }
            return result;
        }

        public bool IsSubsetOf(Set set) {
            foreach (object o in this) {
                if (!set.Contains(o)) {
                    return false;
                }
            }
            return true;
        }

        public bool IsSupersetOf(Set set) {
            return set.IsSubsetOf(this);
        }
        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() {
            return this._items.Keys.GetEnumerator();
        }
        #endregion

        #region Overrides
        public override string ToString() {
            if (Count == 0) {
                return "{}";
            }

            SortedList s = new SortedList(this._items);
            return "{" + Util.Join(", ", s.Keys) + "}";
        }

        public override bool Equals(object obj) {
            if (!(obj is Set)) {
                return false;
            }

            Set set = (Set)obj;

            return set.Count == this.Count && this.Difference(set).Count == 0;
        }

        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }
        #endregion
    }
}
