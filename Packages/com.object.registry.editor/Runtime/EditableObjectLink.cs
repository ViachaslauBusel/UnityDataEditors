using System;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [Serializable]
    public struct EditableObjectLink<T> where T : IEditableObject
    {
        [SerializeField]
        private int _id;

        public int ID => _id;
    }
}