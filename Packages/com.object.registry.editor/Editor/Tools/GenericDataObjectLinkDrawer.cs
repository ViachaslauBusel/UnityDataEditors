using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [CustomPropertyDrawer(typeof(DataLink<>), true)]
    public class GenericDataObjectLinkDrawer : PropertyDrawer
    {
        protected Dictionary<string, IDataObject> _editableObjectLinksCache = new Dictionary<string, IDataObject>();
        protected Type _dataType;

        protected virtual void InitializeEditableObject(SerializedProperty property, string propertyID)
        {
            var idProperty = property.FindPropertyRelative("_id");
            Type parentType = property.serializedObject.targetObject.GetType();
            Type fieldType =  ReflectionUtility.FindPropertyType(parentType, property.propertyPath);
            _dataType = fieldType.GetGenericArguments()[0];

            var allEditableObjects = AssetDatabase.FindAssets("t:" + _dataType.Name)
                                                  .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), _dataType) as IDataObject)
                                                  .ToList();

            _editableObjectLinksCache[propertyID] = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var DataIdfield = property.FindPropertyRelative("_id");
            var datId = DataIdfield.intValue;
            string propertyId = property.propertyPath;
            if (!_editableObjectLinksCache.ContainsKey(propertyId) || (_editableObjectLinksCache[propertyId] != null && _editableObjectLinksCache[propertyId].ID != datId))
            {
                InitializeEditableObject(property, propertyId);
            }

            DataObjectGUIRenderer.DrawDataObject(position, property, label, _editableObjectLinksCache[propertyId], _dataType, (obj) => SelectObject(obj, property, propertyId, DataIdfield));
        }

        private void SelectObject(IDataObject dataObject, SerializedProperty property, string propertyId, SerializedProperty DataIdfield)
        {
            _editableObjectLinksCache[propertyId] = dataObject;
            DataIdfield.intValue = dataObject?.ID ?? 0;
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