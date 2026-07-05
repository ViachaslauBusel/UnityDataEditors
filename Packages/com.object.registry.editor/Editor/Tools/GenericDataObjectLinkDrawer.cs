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

            _dataType = ResolveDataObjectType();
            if (_dataType == null)
            {
                _editableObjectLinksCache[propertyID] = null;
                return;
            }

            var allEditableObjects = AssetDatabase.FindAssets("t:" + _dataType.Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), _dataType) as IDataObject)
                .Where(obj => obj != null)
                .ToList();

            _editableObjectLinksCache[propertyID] = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
        }

        private Type ResolveDataObjectType()
        {
            if (fieldInfo == null)
            {
                return null;
            }

            Type fieldType = fieldInfo.FieldType;

            if (TryGetDataLinkTargetType(fieldType, out Type dataType))
            {
                return dataType;
            }

            if (fieldType.IsArray)
            {
                Type elementType = fieldType.GetElementType();
                if (TryGetDataLinkTargetType(elementType, out dataType))
                {
                    return dataType;
                }
            }

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listElementType = fieldType.GetGenericArguments()[0];
                if (TryGetDataLinkTargetType(listElementType, out dataType))
                {
                    return dataType;
                }
            }

            return null;
        }

        private bool TryGetDataLinkTargetType(Type type, out Type dataType)
        {
            dataType = null;

            if (type == null || !type.IsGenericType)
            {
                return false;
            }

            if (type.GetGenericTypeDefinition() != typeof(DataLink<>))
            {
                return false;
            }

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                return false;
            }

            dataType = genericArguments[0];
            return true;
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
            return defaultHeight + 15;
        }
    }
}