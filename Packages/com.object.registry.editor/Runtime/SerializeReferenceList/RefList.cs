using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectRegistryEditor.SerializeReferenceList
{
    [Serializable]
    public class RefList<T> : IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T> where T : class
    {
        [UnityEngine.SerializeReference]
        private List<T> _items = new List<T>();

        public int Count => _items.Count;
        public T this[int index] => _items[index];
        public IReadOnlyList<T> Items => _items;

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
