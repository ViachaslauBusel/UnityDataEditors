using System;
using UnityEngine;

namespace TestNamespace
{
    [Serializable]
    public class Test_2_SimpleObjectData : SimpleObjectData
    {
        [SerializeField]
        private int _someValue;
        [SerializeField]
        private string _someString;
    }
}
