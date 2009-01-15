using System.Collections.Generic;

namespace CCS.Nodes {
    public class Restrictions : ASTNode, IList<string>{
        private List<string> list = new List<string>();
        public bool HasParens { get; set; }

        #region IList<string> Members

        public int IndexOf(string item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, string item) {
            list.Insert(index, item);
        }

        public void RemoveAt(int index) {
            list.RemoveAt(index);
        }

        public string this[int index] {
            get {
                return list[index];
            }
            set {
                list[index] = value;
            }
        }

        #endregion

        #region ICollection<string> Members

        public void Add(string item) {
            list.Add(item);
        }

        public void Clear() {
            list.Clear();
        }

        public bool Contains(string item) {
            return list.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex) {
            list.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return list.Count; }
        }

        public bool IsReadOnly {
            get { return ((ICollection<string>)list).IsReadOnly;  }
        }

        public bool Remove(string item) {
            return list.Remove(item);
        }

        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator() {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }

        #endregion
    }
}
