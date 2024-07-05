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
    [SerializeField]
    private Texture _preview;
    [SerializeField]
    private string _name;

    public int ID => _id;

    public string Name => _name;

    public Texture Preview => _preview;

    public void Initialize(int iD)
    {
        _id = iD;
    }
}
