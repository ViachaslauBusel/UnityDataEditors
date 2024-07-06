using System;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [Serializable]
    public struct DataLink<T> where T : IDataObject
    {
        [SerializeField, SerializeReference]
        private int _id;

        public int ID => _id;
    }
}