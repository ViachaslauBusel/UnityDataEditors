using ObjectRegistryEditor.SelectorWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestSelectObject))]
internal class TestSelectObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //if (GUILayout.Button("Select Object"))
        //{
        //    EditableObjectSelectorWindow.Display<TestData>((data) =>
        //    {
        //        Debug.Log(data.Name);
        //        ((TestSelectObject)target).data = data;
        //    });
        //}
    }
}
