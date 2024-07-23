using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [CustomPropertyDrawer(typeof(IDataObject), true)]
    public class DataObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = ReflectionUtility.FindProperty(parentType, property.propertyPath);

            DataObjectGUIRenderer.DrawDataObject(position, property, label, property.objectReferenceValue as IDataObject, fi.FieldType, (obj) => SelectObject(property, obj));
        }

        private void SelectObject(SerializedProperty property, IDataObject @object)
        {
            property.objectReferenceValue = (UnityEngine.Object)@object;
            property.serializedObject.ApplyModifiedProperties();
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
