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
            SerializedProperty idProperty = property.FindPropertyRelative("_id");
            if (idProperty == null)
            {
                _dataType = null;
                _editableObjectLinksCache[propertyID] = null;
                return;
            }

            if (fieldInfo == null || !fieldInfo.FieldType.IsGenericType)
            {
                _dataType = null;
                _editableObjectLinksCache[propertyID] = null;
                return;
            }

            Type[] genericArguments = fieldInfo.FieldType.GetGenericArguments();
            if (genericArguments == null || genericArguments.Length == 0)
            {
                _dataType = null;
                _editableObjectLinksCache[propertyID] = null;
                return;
            }

            _dataType = genericArguments[0];

            var allEditableObjects = AssetDatabase.FindAssets("t:" + _dataType.Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), _dataType) as IDataObject)
                .ToList();

            _editableObjectLinksCache[propertyID] = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty dataIdField = property.FindPropertyRelative("_id");
            string propertyId = property.propertyPath;

            if (!_editableObjectLinksCache.ContainsKey(propertyId) ||
                (_editableObjectLinksCache[propertyId] != null && dataIdField != null && _editableObjectLinksCache[propertyId].ID != dataIdField.intValue))
            {
                InitializeEditableObject(property, propertyId);
            }

            if (_dataType == null)
            {
                EditorGUI.LabelField(position, label.text, "Unsupported field");
                return;
            }

            DataObjectGUIRenderer.DrawDataObject(
                position,
                property,
                label,
                _editableObjectLinksCache[propertyId],
                _dataType,
                obj => SelectObject(obj, property, propertyId, dataIdField));
        }

        private void SelectObject(IDataObject dataObject, SerializedProperty property, string propertyId, SerializedProperty dataIdField)
        {
            _editableObjectLinksCache[propertyId] = dataObject;
            if (dataIdField != null)
            {
                dataIdField.intValue = dataObject?.ID ?? 0;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float defaultHeight = base.GetPropertyHeight(property, label);
            float extraSpace = 15;
            return defaultHeight + extraSpace;
        }
    }
}