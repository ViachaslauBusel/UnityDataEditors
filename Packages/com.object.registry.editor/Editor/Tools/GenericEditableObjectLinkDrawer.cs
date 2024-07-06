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
            FieldInfo fi = parentType.GetField(property.propertyPath);
            _dataType = fi.FieldType.GetGenericArguments()[0];

            var allEditableObjects = AssetDatabase.FindAssets("t:" + _dataType.Name)
                                                  .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), _dataType) as IEditableObject)
                                                  .ToList();
            _editableObject = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
        }

        public class TypeNameExtractor
        {
            public static string ExtractTypeName(string fullTypeName)
            {
                // Регулярное выражение для поиска имени типа
                var regex = new Regex(@"\[\[(.+?),");
                var match = regex.Match(fullTypeName);

                if (match.Success)
                {
                    // Возвращаем первую группу, содержащую имя типа
                    return match.Groups[1].Value;
                }

                return null;
            }
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