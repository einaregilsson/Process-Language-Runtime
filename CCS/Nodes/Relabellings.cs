using System.Collections.Generic;

namespace CCS.Nodes {
    public class Relabellings : ASTNode, IDictionary<string, string> {
        private Dictionary<string, string> dict = new Dictionary<string, string>();
        private ICollection<KeyValuePair<string, string>> coll;
        public Relabellings() {
            coll = (ICollection<KeyValuePair<string, string>>)dict;
        }

        #region IDictionary<string,string> Members

        public void Add(string key, string value) {
            dict.Add(key, value);
        }

        public bool ContainsKey(string key) {
            return dict.ContainsKey(key);
        }

        public ICollection<string> Keys {
            get { return dict.Keys; }
        }

        public bool Remove(string key) {
            return dict.Remove(key);
        }

        public bool TryGetValue(string key, out string value) {
            return dict.TryGetValue(key, out value);
        }

        public ICollection<string> Values {
            get { return dict.Values; }
        }

        public string this[string key] {
            get {
                return dict[key];
            }
            set {
                dict[key] = key;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,string>> Members

        public void Add(KeyValuePair<string, string> item) {
            coll.Add(item);
        }

        public void Clear() {
            coll.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item) {
            return coll.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
            coll.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return dict.Count; }
        }

        public bool IsReadOnly {
            get { return coll.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, string> item) {
            return coll.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return ((IEnumerable<KeyValuePair<string, string>>)dict).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return dict.GetEnumerator();
        }

        #endregion
    }
}
