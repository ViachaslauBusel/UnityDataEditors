using System;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [Serializable]
    public struct DataLink<T> : IEquatable<DataLink<T>>, IEquatable<int> where T : IDataObject
    {
        [SerializeField]
        private int _id;

        public int ID => _id;

        public DataLink(int id)
        {
            _id = id;
        }

        public bool Equals(DataLink<T> other) => _id == other._id;
        public bool Equals(int other) => _id == other;

        public override bool Equals(object obj)
        {
            return obj switch
            {
                DataLink<T> link => Equals(link),
                int id => Equals(id),
                _ => false
            };
        }

        public override int GetHashCode() => _id;

        public static bool operator ==(DataLink<T> left, DataLink<T> right) => left.Equals(right);
        public static bool operator !=(DataLink<T> left, DataLink<T> right) => !left.Equals(right);

        public static bool operator ==(DataLink<T> left, int right) => left.Equals(right);
        public static bool operator !=(DataLink<T> left, int right) => !left.Equals(right);

        public static bool operator ==(int left, DataLink<T> right) => right.Equals(left);
        public static bool operator !=(int left, DataLink<T> right) => !right.Equals(left);

        public static implicit operator DataLink<T>(int id) => new DataLink<T>(id);
        public static implicit operator int(DataLink<T> link) => link._id;

        public override string ToString() => _id.ToString();
    }
}