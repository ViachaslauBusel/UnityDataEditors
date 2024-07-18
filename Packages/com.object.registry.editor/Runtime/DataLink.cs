using System;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [Serializable]
    public struct DataLink<T> where T : IDataObject
    {
        [SerializeField]
        private int _id;

        public int ID => _id;

        public DataLink(int id)
        {
            _id = id;
        }
    }
}