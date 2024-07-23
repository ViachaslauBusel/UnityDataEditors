using ObjectRegistryEditor.SelectorWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    internal static class DataObjectGUIRenderer
    {
        internal static void DrawDataObject(Rect position, SerializedProperty property, GUIContent label, IDataObject dataObject, Type objType, Action<IDataObject> action)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw a box around the entire content
            //GUI.Box(position, GUIContent.none);

            // Adjust the position to provide padding inside the box
            Rect innerPosition = new Rect(position.x + 2, position.y + 2, position.width - 4, position.height - 4);

            // Adjusted layout calculations for inner content
            float space = 5;
            float buttonSize = 25;
            float previewSize = 25; // Assuming a square preview
            float labelWidth = innerPosition.width - (previewSize + buttonSize + space * 4); // Adjusted for ID display
            Rect previewRect = new Rect(innerPosition.x, innerPosition.y, previewSize, previewSize);
            Rect nameRect = new Rect(innerPosition.x + previewSize + space, innerPosition.y, labelWidth * 0.75f, EditorGUIUtility.singleLineHeight); // Adjusted for ID display
            Rect idRect = new Rect(nameRect.xMax + space, innerPosition.y, labelWidth * 0.25f, EditorGUIUtility.singleLineHeight); // Positioned next to name
            Rect buttonRect = new Rect(innerPosition.x + innerPosition.width - buttonSize, innerPosition.y, buttonSize, buttonSize);

            // Display Preview Texture

            Texture icon = dataObject?.Preview != null ? dataObject.Preview : EditorGUIUtility.IconContent("BuildSettings.StandaloneGLESEmu").image;
            EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.DrawTexture(previewRect, icon);

            // Display Name
            EditorGUI.LabelField(nameRect, dataObject != null ? dataObject.Name : "null");

            // Display ID on the same line as Name
            EditorGUI.LabelField(idRect, $" ID: {dataObject?.ID ?? 0}");

            // Display Edit Button
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Prefab Icon"), GUI.skin.button))
            {
                DataObjectSelectorWindow.Display(objType, obj => action?.Invoke(obj));
            }

            EditorGUI.EndProperty();
        }
    }
}
