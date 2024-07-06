using ObjectRegistryEditor;
using System.Collections;
using System.Collections.Generic;
using TestNamespace;
using UnityEngine;

[CreateAssetMenu(fileName = "TestRegistry", menuName = "DATA/TestRegistry Data", order = 51)]
public class TestRegistry : EditableObjectRegistry<TestData>
{
}
