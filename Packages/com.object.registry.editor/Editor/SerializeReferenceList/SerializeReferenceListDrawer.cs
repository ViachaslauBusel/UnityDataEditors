using ObjectRegistryEditor.SelectorWindow;
using System;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor.SerializeReferenceList
{
    [CustomPropertyDrawer(typeof(RefList<>), true)]
    internal sealed class SerializeReferenceListDrawer : PropertyDrawer
    {
        private const float AddButtonWidth = 70f;
        private const float RemoveButtonWidth = 70f;
        private const float BoxPadding = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = itemsProperty.GetArrayElementAtIndex(i);
                float elementHeight = EditorGUI.GetPropertyHeight(elementProperty, GetElementLabel(elementProperty, i), true);
                height += EditorGUIUtility.standardVerticalSpacing + elementHeight + BoxPadding * 2f;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Missing _items");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawHeader(headerRect, itemsProperty, label);

            float currentY = headerRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            int removeIndex = -1;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = itemsProperty.GetArrayElementAtIndex(i);
                GUIContent elementLabel = GetElementLabel(elementProperty, i);
                float elementHeight = EditorGUI.GetPropertyHeight(elementProperty, elementLabel, true);

                Rect boxRect = new Rect(position.x, currentY, position.width, elementHeight + BoxPadding * 2f);
                GUI.Box(boxRect, GUIContent.none);

                Rect removeRect = new Rect(
                    boxRect.xMax - RemoveButtonWidth - BoxPadding,
                    boxRect.y + BoxPadding,
                    RemoveButtonWidth,
                    EditorGUIUtility.singleLineHeight);

                Rect propertyRect = new Rect(
                    boxRect.x + BoxPadding,
                    boxRect.y + BoxPadding,
                    boxRect.width - RemoveButtonWidth - BoxPadding * 3f,
                    elementHeight);

                EditorGUI.PropertyField(propertyRect, elementProperty, elementLabel, true);

                if (GUI.Button(removeRect, "Remove"))
                {
                    removeIndex = i;
                }

                currentY = boxRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }

            if (removeIndex >= 0)
            {
                RemoveElementAt(itemsProperty, removeIndex);
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            EditorGUI.EndProperty();
        }

        private void DrawHeader(Rect rect, SerializedProperty itemsProperty, GUIContent label)
        {
            Rect labelRect = new Rect(rect.x, rect.y, rect.width - AddButtonWidth - 4f, rect.height);
            Rect addRect = new Rect(rect.xMax - AddButtonWidth, rect.y, AddButtonWidth, rect.height);

            EditorGUI.LabelField(labelRect, $"{label.text} ({itemsProperty.arraySize})");

            if (GUI.Button(addRect, "Add"))
            {
                ShowAddTypeSelector(itemsProperty);
            }
        }

        private void ShowAddTypeSelector(SerializedProperty itemsProperty)
        {
            Type elementType = fieldInfo.FieldType.GetGenericArguments()[0];
            UnityEngine.Object targetObject = itemsProperty.serializedObject.targetObject;
            string propertyPath = itemsProperty.propertyPath;

            ClassSelectorWindow.Display<object>(
                elementType,
                type => Activator.CreateInstance(type),
                createdObject =>
                {
                    if (createdObject == null || targetObject == null)
                    {
                        return;
                    }

                    SerializedObject serializedObject = new SerializedObject(targetObject);
                    SerializedProperty listProperty = serializedObject.FindProperty(propertyPath);
                    if (listProperty == null)
                    {
                        return;
                    }

                    serializedObject.Update();

                    int index = listProperty.arraySize;
                    listProperty.InsertArrayElementAtIndex(index);
                    listProperty.GetArrayElementAtIndex(index).managedReferenceValue = createdObject;

                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(targetObject);
                });
        }

        private GUIContent GetElementLabel(SerializedProperty elementProperty, int index)
        {
            Type runtimeType = elementProperty.managedReferenceValue?.GetType();
            string typeName = runtimeType != null ? runtimeType.Name : "Null";
            return new GUIContent($"Element {index}: {typeName}");
        }

        private void RemoveElementAt(SerializedProperty listProperty, int index)
        {
            SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(index);
            if (elementProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                elementProperty.managedReferenceValue = null;
            }

            listProperty.DeleteArrayElementAtIndex(index);
        }
    }
}