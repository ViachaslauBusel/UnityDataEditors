using ObjectRegistryEditor;
using ObjectRegistryEditor.SelectorWindow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestData", menuName = "ScriptableObjects/Test Data", order = 1)]
[ItemDetails("Base class for test data.")]
public class TestData : ScriptableObject, IEditableObject
{
    [SerializeField]
    private int _id;

    public int ID => _id;

    public string Name => "Test";

    public Texture Preview => null;

    public void Initialize(int iD)
    {
        _id = iD;
    }
}
