using ObjectRegistryEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEditor.Progress;
using UnityEditor;
using UnityEngine;
using TestNamespace;

namespace Assets.Scripts
{
    [CustomPropertyDrawer(typeof(TestDataLink))]
    public class TestDataLinkDrawer : DataObjectLinkDrawer<TestData>
    {
    }
}
