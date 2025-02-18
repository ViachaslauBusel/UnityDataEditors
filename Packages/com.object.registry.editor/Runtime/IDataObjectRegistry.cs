using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ObjectRegistryEditor
{
    public interface IDataObjectRegistry
    {
        string Name { get; }
        public IDataObject this[int index] { get; }
        int Count { get; }

        IDataObject AddObjectOfType(Type type);
        U AddObjectOfType<U>() where U : ScriptableObject, IDataObject;
        IDataObject GetObjectByID(int id);
        void RemoveObject(IDataObject oBJ);
        void Export();
        Type GetObjectType();
    }
}
