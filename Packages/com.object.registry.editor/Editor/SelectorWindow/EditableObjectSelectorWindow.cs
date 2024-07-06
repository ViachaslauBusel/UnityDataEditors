using ObjectRegistryEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor.SelectorWindow
{
    public class EditableObjectSelectorWindow : EditorWindow
    {
        private IGenericWindow _genericWindow;
        /// <summary>
        /// Open a window to select a ScriptableObject of type T
        /// </summary>
        public static void Display<T>(Action<T> action) where T : ScriptableObject, IEditableObject
        {
            Display(typeof(T), (i) => action?.Invoke((T)i));
        }

        public static void Display(Type dataType, Action<IEditableObject> action)
        {
            EditableObjectSelectorWindow window = EditorWindow.GetWindow<EditableObjectSelectorWindow>(true, "ADD");
            window.minSize = new Vector2(300.0f, 500.0f);
            GenericScriptableObjectSelectorWindow genericWindow = new GenericScriptableObjectSelectorWindow(dataType, action, window);
            window._genericWindow = genericWindow;

        }

        private void OnGUI()
        {
            _genericWindow?.OnGUI();
        }
    }


    public class GenericScriptableObjectSelectorWindow : IGenericWindow
    {
        private string _find = "";
        private GUIStyle _iconBackground;
        private Action<IEditableObject> _action;
        private EditorWindow _window;
        private List<IEditableObject> _objects = new List<IEditableObject>();

        public GenericScriptableObjectSelectorWindow(Type dataType, Action<IEditableObject> action, EditorWindow window)
        {
            _action = action;
            _window = window;
            _iconBackground = BackgroundStyle.Get(new Color(0.15f, 0.15f, 0.15f));

            var assets = AssetDatabase.FindAssets($"t:{dataType.Name}");

            foreach (var asset in assets)
            {
                var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), dataType);
                if (obj is IEditableObject editableObject)
                {
                    _objects.Add(editableObject);
                }
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.white));
            _find = EditorGUILayout.TextField("FIND:", _find);
            _find = _find.ToLower(); // Convert to lowercase once
            EditorGUILayout.EndVertical();

            Func<IEditableObject, bool> filter = _find.StartsWith("id:")
                                               ? (i => i.ID.ToString().Contains(_find.Substring(3))) 
                                               : (i => i.Name.ToLower().Contains(_find)); // Check if the search string starts with "id:

            // Use IEnumerable to avoid unnecessary array conversion
            IEnumerable<IEditableObject> filteredObjects = string.IsNullOrEmpty(_find)
                                                          ? _objects
                                                          : _objects.Where(filter); // Use the pre-lowered search string

            int index = 0;
            foreach (var obj in filteredObjects)
            {
                EditorGUILayout.BeginHorizontal((index % 2 == 0) ? BackgroundStyle.Get(Color.gray) : BackgroundStyle.Get(Color.blue));
                //Icon
                Texture icon = obj?.Preview != null ? obj.Preview : EditorGUIUtility.IconContent("BuildSettings.Broadcom").image;

                GUILayout.Box(icon, _iconBackground, GUILayout.Width(30), GUILayout.Height(30));
                
                GUILayout.Label(obj.Name);
                GUILayout.Label($" ID: {obj.ID}");
                if (GUILayout.Button("select", GUILayout.Width(60)))
                {
                    IEditableObject castedObj = obj;
                    if (castedObj != null)
                    {
                        _action?.Invoke(castedObj);
                        _window.Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
                index++;
            }
        }
    }
}
