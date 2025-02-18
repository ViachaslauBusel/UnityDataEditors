using ObjectRegistryEditor.Helpers;
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
    public class DataObjectSelectorWindow : EditorWindow
    {
        private ScriptableObjectSelectorWindow _windowLogic;
        /// <summary>
        /// Open a window to select a ScriptableObject of type T
        /// </summary>
        public static void Display<T>(Action<T> action) where T : ScriptableObject, IDataObject
        {
            Display(typeof(T), (i) => action?.Invoke((T)i));
        }

        public static void Display(Type dataType, Action<IDataObject> action)
        {
            DataObjectSelectorWindow window = EditorWindow.GetWindow<DataObjectSelectorWindow>(true, "ADD");
            window.minSize = new Vector2(300.0f, 500.0f);
            ScriptableObjectSelectorWindow genericWindow = new ScriptableObjectSelectorWindow(dataType, action, window);
            window._windowLogic = genericWindow;

        }

        private void OnGUI()
        {
            _windowLogic?.OnGUI();
        }
    }


    public class ScriptableObjectSelectorWindow
    {
        private string _find = "";
        private int _currentPage = 0;
        private GUIStyle _iconBackground;
        private Action<IDataObject> _action;
        private EditorWindow _window;
        private List<IDataObject> _objects = new List<IDataObject>();

        public ScriptableObjectSelectorWindow(Type dataType, Action<IDataObject> action, EditorWindow window)
        {
            _action = action;
            _window = window;
            _iconBackground = BackgroundStyle.Get(new Color(0.15f, 0.15f, 0.15f));

            var assets = AssetDatabase.FindAssets($"t:{dataType.Name}");

            foreach (var asset in assets)
            {
                var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), dataType);
                if (obj is IDataObject editableObject)
                {
                    _objects.Add(editableObject);
                }
            }
        }

        public void OnGUI()
        {
            Func<IDataObject, bool> filter = _find.StartsWith("id:")
                                   ? (i => i.ID.ToString().Contains(_find.Substring(3)))
                                   : (i => i.Name.ToLower().Contains(_find)); // Check if the search string starts with "id:

            // Use IEnumerable to avoid unnecessary array conversion
            IEnumerable<IDataObject> filteredObjects = string.IsNullOrEmpty(_find)
                                                          ? _objects
                                                          : _objects.Where(filter); // Use the pre-lowered search string
            int totalObjects = filteredObjects.Count();
            float totalHeight = totalObjects * 60f;
            float pageHeight = _window.position.height - 30;
            int elementsPerPage = Mathf.FloorToInt(pageHeight / 30f);
            int maxPages = Mathf.FloorToInt(totalObjects / (float)elementsPerPage);
            _currentPage = Math.Clamp(_currentPage, 0, maxPages);

            EditorGUILayout.BeginHorizontal(BackgroundStyle.Get(Color.white));
            //Button left page
            _currentPage = GUIHelper.DrawPagesSelector(_currentPage, maxPages);
            GUILayout.Label("FIND:", GUILayout.Width(40));
            _find = EditorGUILayout.TextField(_find, GUILayout.Width(200));
            _find = _find.ToLower(); // Convert to lowercase once
            
            EditorGUILayout.EndHorizontal();

            filteredObjects = filteredObjects.Skip(_currentPage * elementsPerPage).Take(elementsPerPage);
            int index = 0;
            foreach (var obj in filteredObjects)
            {
                EditorGUILayout.BeginHorizontal((index % 2 == 0) ? BackgroundStyle.Get(Color.gray) : BackgroundStyle.Get(Color.blue));
                //Icon
                Texture icon = obj?.Preview != null ? obj.Preview : EditorGUIUtility.IconContent("BuildSettings.Broadcom").image;

                GUILayout.Box(icon, _iconBackground, GUILayout.Width(45), GUILayout.Height(45));
                
                GUILayout.Label(obj.Name);
                GUILayout.FlexibleSpace();
                GUILayout.Label($" ID: {obj.ID}", GUILayout.Width(100));
                if (GUILayout.Button("select", GUILayout.Width(60)))
                {
                    IDataObject castedObj = obj;
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
