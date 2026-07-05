using ObjectRegistryEditor.Services;
using ObjectRegistryEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor.SelectorWindow
{
    public class ClassSelectorWindow : EditorWindow
    {
        private Type _baseType;
        private Func<Type, object> _createInstance;
        private Action<object> _actionOnCreate;
        private IReadOnlyList<Type> _types = Array.Empty<Type>();
        private string _find = string.Empty;
        private Vector2 _scrollPosition;

        public static void Display<TBase>(Type baseType, Func<Type, TBase> createInstance, Action<TBase> actionOnCreate = null)
        {
            ClassSelectorWindow window = GetWindow<ClassSelectorWindow>(true, $"ADD {baseType?.Name}");
            window.Initialize(
                baseType,
                type => createInstance(type),
                createdObject =>
                {
                    if (createdObject is TBase typedObject)
                    {
                        actionOnCreate?.Invoke(typedObject);
                    }
                });

            window.minSize = new Vector2(300.0f, 500.0f);
        }

        private void Initialize(Type baseType, Func<Type, object> createInstance, Action<object> actionOnCreate)
        {
            _baseType = baseType;
            _createInstance = createInstance;
            _actionOnCreate = actionOnCreate;
            RefreshTypes(false);
        }

        private void RefreshTypes(bool forceRefresh)
        {
            _types = forceRefresh ? CreatableTypeSearchService.Refresh(_baseType)
                                  : CreatableTypeSearchService.GetCreatableTypes(_baseType);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_types.Count == 0)
            {
                EditorGUILayout.HelpBox($"Types derived from {_baseType?.Name} were not found.", MessageType.Info);
                return;
            }

            IEnumerable<Type> filteredTypes = string.IsNullOrWhiteSpace(_find)
                ? _types
                : _types.Where(type => type.Name.IndexOf(_find, StringComparison.OrdinalIgnoreCase) >= 0);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            int index = 0;

            foreach (Type type in filteredTypes)
            {
                EditorGUILayout.BeginVertical((index % 2 == 0) ? BackgroundStyle.Get(Color.gray) : BackgroundStyle.Get(Color.blue));
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(type.Name);

                if (GUILayout.Button("add", GUILayout.Width(60)))
                {
                    object createdObject = _createInstance?.Invoke(type);
                    _actionOnCreate?.Invoke(createdObject);
                    Close();
                }

                EditorGUILayout.EndHorizontal();

                object[] attributes = type.GetCustomAttributes(typeof(ItemDetailsAttribute), false);
                if (attributes.Length > 0)
                {
                    ItemDetailsAttribute attribute = (ItemDetailsAttribute)attributes[0];
                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        normal = { textColor = Color.black }
                    };

                    GUILayout.Label(attribute.Description, labelStyle);
                }

                EditorGUILayout.EndVertical();
                index++;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.white));
            _find = EditorGUILayout.TextField("FIND:", _find);

            if (GUILayout.Button(new GUIContent("", "Refresh"), RedactorStyle.Refresh, GUILayout.Width(25), GUILayout.Height(25)))
            {
                RefreshTypes(true);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
