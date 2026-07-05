using ObjectRegistryEditor.SelectorWindow;
using ObjectRegistryEditor.SerializeReferenceList;
using System;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    [CustomPropertyDrawer(typeof(RefList<>), true)]
    internal sealed class SerializeReferenceListDrawer : PropertyDrawer
    {
        private const float HeaderHeight = 22f;
        private const float CardHeaderHeight = 28f;
        private const float AddButtonHeight = 28f;

        private const float Padding = 8f;
        private const float Spacing = 6f;

        private const float HeaderButtonSize = 22f;
        private const float HeaderButtonSpacing = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = HeaderHeight + Spacing;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = itemsProperty.GetArrayElementAtIndex(i);

                height += GetElementCardHeight(elementProperty);
                height += Spacing;
            }

            height += AddButtonHeight;

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

            Rect headerRect = new Rect(
                position.x,
                position.y,
                position.width,
                HeaderHeight);

            DrawHeader(headerRect, itemsProperty, label);

            float currentY = headerRect.yMax + Spacing;

            int removeIndex = -1;
            int moveFromIndex = -1;
            int moveToIndex = -1;

            for (int i = 0; i < itemsProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = itemsProperty.GetArrayElementAtIndex(i);

                float cardHeight = GetElementCardHeight(elementProperty);

                Rect cardRect = new Rect(
                    position.x,
                    currentY,
                    position.width,
                    cardHeight);

                DrawElementCard(
                    cardRect,
                    itemsProperty,
                    elementProperty,
                    i,
                    ref removeIndex,
                    ref moveFromIndex,
                    ref moveToIndex);

                currentY = cardRect.yMax + Spacing;
            }

            Rect addRect = new Rect(
                position.x,
                currentY,
                position.width,
                AddButtonHeight);

            DrawAddButton(addRect, itemsProperty);

            if (moveFromIndex >= 0 && moveToIndex >= 0)
            {
                MoveElement(itemsProperty, moveFromIndex, moveToIndex);
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
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
            EditorGUI.LabelField(
                rect,
                $"{label.text} ({itemsProperty.arraySize})",
                EditorStyles.boldLabel);
        }

        private void DrawAddButton(Rect rect, SerializedProperty itemsProperty)
        {
            Rect buttonRect = new Rect(
                rect.x + 16f,
                rect.y,
                rect.width - 32f,
                rect.height);

            if (GUI.Button(buttonRect, "+  Add Module", EditorStyles.miniButton))
            {
                ShowAddTypeSelector(itemsProperty);
            }
        }

        private void DrawElementCard(
            Rect rect,
            SerializedProperty itemsProperty,
            SerializedProperty elementProperty,
            int index,
            ref int removeIndex,
            ref int moveFromIndex,
            ref int moveToIndex)
        {
            DrawCardBackground(rect);

            Rect headerRect = new Rect(
                rect.x + Padding,
                rect.y + Padding,
                rect.width - Padding * 2f,
                CardHeaderHeight);

            DrawElementHeader(
                headerRect,
                itemsProperty,
                elementProperty,
                index,
                ref removeIndex,
                ref moveFromIndex,
                ref moveToIndex);

            Rect lineRect = new Rect(
                rect.x + Padding,
                headerRect.yMax,
                rect.width - Padding * 2f,
                1f);

            EditorGUI.DrawRect(lineRect, new Color(0.28f, 0.28f, 0.28f, 1f));

            Rect contentRect = new Rect(
                rect.x + Padding,
                lineRect.yMax + Padding,
                rect.width - Padding * 2f,
                rect.height - CardHeaderHeight - Padding * 3f - 1f);

            DrawElementFields(contentRect, elementProperty);
        }

        private void DrawCardBackground(Rect rect)
        {
            Color oldColor = GUI.color;

            GUI.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            GUI.Box(rect, GUIContent.none);

            GUI.color = oldColor;
        }

        private void DrawElementHeader(
            Rect rect,
            SerializedProperty itemsProperty,
            SerializedProperty elementProperty,
            int index,
            ref int removeIndex,
            ref int moveFromIndex,
            ref int moveToIndex)
        {
            string typeName = GetReadableTypeName(elementProperty);
            string title = $"{index + 1}. {typeName}";

            float controlsWidth =
                HeaderButtonSize * 3f +
                HeaderButtonSpacing * 2f;

            Rect titleRect = new Rect(
                rect.x,
                rect.y,
                rect.width - controlsWidth - Spacing,
                rect.height);

            Rect upRect = new Rect(
                rect.xMax - controlsWidth,
                rect.y + 3f,
                HeaderButtonSize,
                HeaderButtonSize);

            Rect downRect = new Rect(
                upRect.xMax + HeaderButtonSpacing,
                rect.y + 3f,
                HeaderButtonSize,
                HeaderButtonSize);

            Rect removeRect = new Rect(
                downRect.xMax + HeaderButtonSpacing,
                rect.y + 3f,
                HeaderButtonSize,
                HeaderButtonSize);

            EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(index <= 0))
            {
                if (GUI.Button(upRect, new GUIContent("↑", "Move module up"), EditorStyles.miniButton))
                {
                    moveFromIndex = index;
                    moveToIndex = index - 1;
                }
            }

            using (new EditorGUI.DisabledScope(index >= itemsProperty.arraySize - 1))
            {
                if (GUI.Button(downRect, new GUIContent("↓", "Move module down"), EditorStyles.miniButton))
                {
                    moveFromIndex = index;
                    moveToIndex = index + 1;
                }
            }

            if (GUI.Button(removeRect, new GUIContent("×", "Remove module"), EditorStyles.miniButton))
            {
                removeIndex = index;
            }
        }

        private void DrawElementFields(Rect rect, SerializedProperty elementProperty)
        {
            if (elementProperty.managedReferenceValue == null)
            {
                EditorGUI.HelpBox(rect, "Null module reference", MessageType.Warning);
                return;
            }

            SerializedProperty iterator = elementProperty.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            float currentY = rect.y;

            EditorGUI.indentLevel++;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;

                float fieldHeight = EditorGUI.GetPropertyHeight(iterator, true);

                Rect fieldRect = new Rect(
                    rect.x,
                    currentY,
                    rect.width,
                    fieldHeight);

                EditorGUI.PropertyField(fieldRect, iterator, true);

                currentY += fieldHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        private float GetElementCardHeight(SerializedProperty elementProperty)
        {
            float contentHeight = GetElementFieldsHeight(elementProperty);

            return Padding
                   + CardHeaderHeight
                   + 1f
                   + Padding
                   + contentHeight
                   + Padding;
        }

        private float GetElementFieldsHeight(SerializedProperty elementProperty)
        {
            if (elementProperty.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight * 2f;
            }

            float height = 0f;

            SerializedProperty iterator = elementProperty.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;
            bool hasFields = false;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                hasFields = true;

                height += EditorGUI.GetPropertyHeight(iterator, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            if (!hasFields)
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        private string GetReadableTypeName(SerializedProperty elementProperty)
        {
            Type runtimeType = elementProperty.managedReferenceValue?.GetType();

            if (runtimeType == null)
            {
                return "Null Module";
            }

            string name = runtimeType.Name;

            if (name.EndsWith("ModuleData"))
            {
                name = name[..^"ModuleData".Length];
            }

            if (name.EndsWith("Data"))
            {
                name = name[..^"Data".Length];
            }

            return ObjectNames.NicifyVariableName(name);
        }

        private void MoveElement(SerializedProperty listProperty, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            if (fromIndex < 0 || fromIndex >= listProperty.arraySize)
            {
                return;
            }

            if (toIndex < 0 || toIndex >= listProperty.arraySize)
            {
                return;
            }

            listProperty.MoveArrayElement(fromIndex, toIndex);
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