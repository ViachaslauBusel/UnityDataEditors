using ObjectRegistryEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace ObjectRegistryEditor.SelectorWindow
{
    public class ClassSelectorWindow : EditorWindow
    {
        private IEditableObjectRegistry _editableObjectRegistry;
        private Action<IEditableObject> _actionOnAdd;
        private List<Type> _types;
        private string _find = "";
        private Vector2 _scrollPosition;

        /// <summary>
        /// Open a window to select an class that implements the IEditableObject interface and add it to the registry.
        /// </summary>
        public static void Display(IEditableObjectRegistry editableObjectRegistry, List<Type> types, Action<IEditableObject> actionOnAdd)
        {
            ClassSelectorWindow window = EditorWindow.GetWindow<ClassSelectorWindow>(true, "ADD");
            window.Initialize(editableObjectRegistry, actionOnAdd, types);
            window.minSize = new Vector2(300.0f, 500.0f);
        }

        private void Initialize(IEditableObjectRegistry editableObjectRegistry, Action<IEditableObject> actionOnAdd, List<Type> types)
        {
            _editableObjectRegistry = editableObjectRegistry;
            _actionOnAdd = actionOnAdd;
            _types = types;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.white));
            _find = EditorGUILayout.TextField("FIND:", _find);
            string lowerFind = _find.ToLower(); // Convert to lowercase once
            EditorGUILayout.EndVertical();

            IEnumerable<Type> filteredObjects = string.IsNullOrEmpty(lowerFind)
                ? _types
                : _types.Where(i => i.Name.ToLower().Contains(lowerFind));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            int index = 0;
            foreach (var obj in filteredObjects)
            {
                EditorGUILayout.BeginVertical((index % 2 == 0) ? BackgroundStyle.Get(Color.gray) : BackgroundStyle.Get(Color.blue));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(obj.Name);
                if (GUILayout.Button("add", GUILayout.Width(60)))
                {
                    var addedObj = _editableObjectRegistry.AddObjectOfType(obj);
                    _actionOnAdd?.Invoke(addedObj);
                    Close();
                }
                EditorGUILayout.EndHorizontal();

                // If class contains attributes ItemDetailsAttribute, display them
                var attributes = obj.GetCustomAttributes(typeof(ItemDetailsAttribute), false);
                if (attributes.Length > 0)
                {
                    var attribute = (ItemDetailsAttribute)attributes[0]; // Safe to do because we checked the length
                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.black } };
                    GUILayout.Label(attribute.Description, labelStyle);
                }

                EditorGUILayout.EndVertical();

                index++;
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
