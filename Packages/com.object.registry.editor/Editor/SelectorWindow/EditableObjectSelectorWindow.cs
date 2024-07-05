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

            EditableObjectSelectorWindow window = EditorWindow.GetWindow<EditableObjectSelectorWindow>(true, "ADD");
            window.minSize = new Vector2(300.0f, 500.0f);
            GenericScriptableObjectSelectorWindow<T> genericWindow = new GenericScriptableObjectSelectorWindow<T>(action, window);
            window._genericWindow = genericWindow;

        }
        private void OnGUI()
        {
            _genericWindow?.OnGUI();
        }
    }


    public class GenericScriptableObjectSelectorWindow<T> : IGenericWindow where T : ScriptableObject, IEditableObject
    {
        private string m_find = "";
        private Action<T> m_action;
        private EditorWindow m_window;
        private List<IEditableObject> m_objects = new List<IEditableObject>();

        public GenericScriptableObjectSelectorWindow(Action<T> action, EditorWindow window)
        {
            m_action = action;
            m_window = window;

            var assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (var asset in assets)
            {
                var obj = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(asset));
                if (obj is IEditableObject editableObject)
                {
                    m_objects.Add(editableObject);
                }
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(Color.white));
            m_find = EditorGUILayout.TextField("FIND:", m_find);
            string lowerFind = m_find.ToLower(); // Convert to lowercase once
            EditorGUILayout.EndVertical();

            // Use IEnumerable to avoid unnecessary array conversion
            IEnumerable<IEditableObject> filteredObjects = string.IsNullOrEmpty(m_find)
                ? m_objects
                : m_objects.Where(i => i.Name.ToLower().Contains(lowerFind)); // Use the pre-lowered search string

            int index = 0;
            foreach (var obj in filteredObjects)
            {
                EditorGUILayout.BeginHorizontal((index % 2 == 0) ? BackgroundStyle.Get(Color.gray) : BackgroundStyle.Get(Color.blue));
                //Icon
                Texture icon = obj?.Preview != null ? obj.Preview : EditorGUIUtility.IconContent("BuildSettings.StandaloneGLESEmu").image;

                GUILayout.Box(icon, GUILayout.Width(30), GUILayout.Height(30));
                
                GUILayout.Label(obj.Name);
                GUILayout.Label($" ID: {obj.ID}");
                if (GUILayout.Button("select", GUILayout.Width(60)))
                {
                    T castedObj = obj as T;
                    if (castedObj != null)
                    {
                        m_action?.Invoke(castedObj);
                        m_window.Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
                index++;
            }
        }
    }
}
