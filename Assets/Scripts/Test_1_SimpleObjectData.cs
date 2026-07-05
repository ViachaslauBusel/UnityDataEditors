using ObjectRegistryEditor;
using System;
using UnityEngine;

namespace TestNamespace
{
    [Serializable]
    public class Test_1_SimpleObjectData : SimpleObjectData
    {
        [SerializeField]
        private int _someValue;
        [SerializeField]
        private string _someString;
        [SerializeField]
        private DataLink<TestData> _dataLink;
    }
}
