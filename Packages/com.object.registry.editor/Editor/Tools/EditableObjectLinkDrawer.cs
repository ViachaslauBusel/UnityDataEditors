using ObjectRegistryEditor.SelectorWindow;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ObjectRegistryEditor
{
    public class EditableObjectLinkDrawer<T> : PropertyDrawer where T : ScriptableObject, IEditableObject
    {
        private T _editableObject;
        private bool _isInitialized = false;

        private void InitializeEditableObject(SerializedProperty property)
        {
            var idProperty = property.FindPropertyRelative("_id");
            var allEditableObjects = AssetDatabase.FindAssets("t:" + typeof(T).Name)
                                                  .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                                                  .ToList();
            _editableObject = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
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

            Texture icon = _editableObject.Preview != null ? _editableObject.Preview : EditorGUIUtility.IconContent("BuildSettings.StandaloneGLESEmu").image;

            EditorGUI.DrawPreviewTexture(previewRect, icon);


            // Display Name
            EditorGUI.LabelField(nameRect, _editableObject != null ? _editableObject.Name : "null");

            // Display ID on the same line as Name
            EditorGUI.LabelField(idRect, $" ID: {id}");

            // Display Edit Button
            if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("Prefab Icon"), GUI.skin.button))
            {
                EditableObjectSelectorWindow.Display<T>(selectedObject =>
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
            float extraSpace = 10; // Adjust this value as needed

            return defaultHeight + extraSpace;
        }
    }
}
