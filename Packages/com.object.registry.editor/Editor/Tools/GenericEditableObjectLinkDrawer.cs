using ObjectRegistryEditor.SelectorWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    // Base drawer that can handle any EditableObjectLink<T>
    [CustomPropertyDrawer(typeof(EditableObjectLink<>), true)]
    public class GenericEditableObjectLinkDrawer : PropertyDrawer
    {
        private IEditableObject _editableObject;
        private bool _isInitialized = false;
        private Type _dataType;

        private void InitializeEditableObject(SerializedProperty property)
        {
            var idProperty = property.FindPropertyRelative("_id");
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = FindProperty(parentType, property.propertyPath);
            _dataType = fi.FieldType.GetGenericArguments()[0];

            var allEditableObjects = AssetDatabase.FindAssets("t:" + _dataType.Name)
                                                  .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), _dataType) as IEditableObject)
                                                  .ToList();
            _editableObject = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
        }

        public FieldInfo FindProperty(Type parentType, string propertyPath)
        {
            string[] properties = propertyPath.Split('.');
            FieldInfo fi = null;
            foreach (var property in properties)
            {
                if (property.Contains("Array")) continue; // Skip array properties
                // Handling for array/list elements, which are denoted by property names like "Array.data[x]"
                if (property.Contains("data["))
                {
                    // Extract the index within the brackets (though not used here, might be useful for future enhancements)
                    var indexString = property.Substring(property.IndexOf('[') + 1, property.IndexOf(']') - property.IndexOf('[') - 1);
                    int index;
                    if (int.TryParse(indexString, out index))
                    {
                        // Assuming the current parentType is an array or a list, we get the element type
                        if (parentType.IsArray)
                        {
                            parentType = parentType.GetElementType(); // For arrays
                        }
                        else if (parentType.IsGenericType && parentType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            parentType = parentType.GetGenericArguments()[0]; // For generic lists
                        }
                        continue; // Skip further processing in this iteration
                    }
                }
                else
                {
                    // Use BindingFlags to search for both public and non-public instance fields
                    fi = parentType.GetField(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi == null)
                    {
                        return null; // Field not found
                    }
                    parentType = fi.FieldType;
                }
            }

            return fi;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_isInitialized)
            {
                InitializeEditableObject(property);
                _isInitialized = true;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Draw a box around the entire content
            //GUI.Box(position, GUIContent.none);

            // Adjust the position to provide padding inside the box
            Rect innerPosition = new Rect(position.x + 2, position.y + 2, position.width - 4, position.height - 4);

            var idProperty = property.FindPropertyRelative("_id");
            var id = idProperty.intValue;

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

            Texture icon = _editableObject?.Preview != null ? _editableObject.Preview : EditorGUIUtility.IconContent("BuildSettings.StandaloneGLESEmu").image;
            EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.DrawTexture(previewRect, icon);

            // Display Name
            EditorGUI.LabelField(nameRect, _editableObject != null ? _editableObject.Name : "null");

            // Display ID on the same line as Name
            EditorGUI.LabelField(idRect, $" ID: {id}");

            // Display Edit Button
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Prefab Icon"), GUI.skin.button))
            {
                EditableObjectSelectorWindow.Display(_dataType, selectedObject =>
                {
                    _editableObject = selectedObject;
                    idProperty.intValue = selectedObject?.ID ?? 0;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate the default property height
            float defaultHeight = base.GetPropertyHeight(property, label);

            // Add extra space
            float extraSpace = 15; // Adjust this value as needed

            return defaultHeight + extraSpace;
        }
    }
}