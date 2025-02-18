using NUnit.Framework;
using ObjectRegistryEditor.Helpers;
using ObjectRegistryEditor.SelectorWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ObjectRegistryEditor
{
    public class RegistryPopupData
    {
        public readonly string[] AllRegistryNames;
        public readonly List<IDataObjectRegistry> AllRegistryInProject;

        public RegistryPopupData(List<IDataObjectRegistry> allRegistryInProject)
        {
            AllRegistryInProject = allRegistryInProject;
            AllRegistryNames = AllRegistryInProject.Select(registry => registry.Name).ToArray();
        }
    }

    /// <summary>
    /// ObjectRegistry editor window.
    /// </summary>
    public class WindowDataRegistryEditor: EditorWindow
    {
        private const string FILE_PATH_KEY = "ObjectRegistryEditor.FilePath";
        /// <summary>Cell width.</summary>
        private int _cellWidth = 100;
        /// <summary>Cell height.</summary>
        private int _cellHeight = 140;
        private float _menuHeight = 30.0f;
        private int _currentPage = 0;
        private int _totalPages;
        private IDataObjectRegistry _editableRegistry;
        private IDataObject _selectedObject;
        private long _clickTime;
        private RegistryPopupData _popup;

        private void OnEnable()
        {
            UpdatePopup();
        }

        private void UpdatePopup()
        {
            var registries = AssetDatabase.GetAllAssetPaths()
                                      .Select(i => AssetDatabase.LoadAssetAtPath<ScriptableObject>(i))
                                      .OfType<IDataObjectRegistry>()
                                      .ToList();
            _popup = new RegistryPopupData(registries);
        }

        public void OnGUI()
        {
            DrawMenu();
            if (_editableRegistry == null)
            {
                string filePath = EditorPrefs.GetString(FILE_PATH_KEY);
                if(!string.IsNullOrEmpty(filePath))
                {
                    LoadFromFile(filePath);
                }

                if (_editableRegistry == null) return;
            }
            DrawWindowGrid();
        }

        public static WindowDataRegistryEditor OpenWindow()
        {
            var window = EditorWindow.GetWindow<WindowDataRegistryEditor>(false, "ObjectRegistry");
            window.minSize = new Vector2(500.0f, 320.0f);
            return window;
        }

        public void DrawMenu()
        {
            GUILayout.BeginHorizontal(RedactorStyle.Menu);
            GUILayout.Space(10.0f);
            //Button save>>>
            if (GUILayout.Button("", RedactorStyle.SaveOnDisk, GUILayout.Height(25), GUILayout.Width(25)))
            {
                Save();
            }
            //Button save<<<<
            GUILayout.Space(20.0f);
            //Button export>>>
            if (GUILayout.Button("", RedactorStyle.Export, GUILayout.Height(25), GUILayout.Width(25)))
            {
                bool error = false;
                try
                {
                    _editableRegistry.Export();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    error = true;
                }
                finally
                {
                    EditorUtility.DisplayDialog("Export", "Export completed " + ((error) ? "with errors" : "successfully"), "ok");
                }
            }
            //Button export<<<<
            GUILayout.Space(20.0f);
            //Button remove>>>>
            if (GUILayout.Button("", RedactorStyle.Delet, GUILayout.Height(25), GUILayout.Width(25)))
            {
                RemoveSelectObject();
            }
            //Button remove<<<<
            GUILayout.Space(40.0f);

            // Dropdown list of editable registries
            DrawDropdown();

            GUILayout.FlexibleSpace();
            //Pages>>>
            _currentPage = GUIHelper.DrawPagesSelector(_currentPage, _totalPages);
            //Pages<<<
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void DrawDropdown()
        {

            // Находим индекс текущего выбранного регистра
            int selectedIndex = _popup.AllRegistryInProject.IndexOf(_editableRegistry);

            // Отображаем выпадающий список
            int newIndex = EditorGUILayout.Popup(selectedIndex, _popup.AllRegistryNames, GUILayout.Height(25), GUILayout.Width(200));
            if (GUILayout.Button("", RedactorStyle.Refresh, GUILayout.Height(25), GUILayout.Width(25)))
            {
                UpdatePopup();
            }

            // Если выбранный индекс изменился, обновляем текущий регистр
            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < _popup.AllRegistryInProject.Count)
            {
                string assetPath = AssetDatabase.GetAssetPath((ScriptableObject)_popup.AllRegistryInProject[newIndex]);
                LoadFromFile(assetPath);
            }
        }

        private void DrawWindowGrid()
        {
            // Height of the window without the menu
            float height = position.height - _menuHeight;

            // Number of cells horizontally
            int horizontal_element = (int)position.width / _cellWidth;
            // Number of cells vertically
            int verctical_element = (int)height / _cellHeight;

            // The distance between the cells
            float horizontal_space = (position.width - (horizontal_element * _cellWidth)) / (horizontal_element - 1);
            float vertical_space = (height - (verctical_element * _cellHeight)) / (verctical_element - 1);

            // Number of cells on the page
            int elementOnPage = horizontal_element * verctical_element;
            // Total number of pages
            _totalPages = (_editableRegistry.Count + 1) / (elementOnPage);
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            // Index of the first element on the page in the registry
            int index = _currentPage * elementOnPage;
            int ver_i = 0;
            int hor_i = 0;
            for (ver_i = 0; ver_i < verctical_element; ver_i++)
            {
                for (hor_i = 0; hor_i < horizontal_element; hor_i++)
                {
                    // Draw the button to create a new object
                    if (_editableRegistry.Count <= index)
                    {
                        if (GUI.Button(new Rect(_cellWidth * hor_i + horizontal_space * hor_i + 18,
                                        _cellHeight * ver_i + vertical_space * ver_i + 38 + _menuHeight,
                                                     64, 64), "", RedactorStyle.Plus))
                        {
                            CreateObject();
                        }
                        return;//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    }
                    DrawItemArea(_editableRegistry[index++], ver_i, hor_i, horizontal_space, vertical_space);
                }
            }
        }

        /// <summary>
        /// Draw object area
        /// </summary>
        /// <param name="obj">Обьект для отрисовки</param>
        /// <param name="ver_i">Позиция ячейки по вертикали</param>
        /// <param name="hor_i">Позиция ячейки по горизонтали</param>
        /// <param name="horizontal_space"></param>
        /// <param name="vertical_space"></param>
        private void DrawItemArea(IDataObject obj, int ver_i, int hor_i, float horizontal_space, float vertical_space)
        {
            GUILayout.BeginArea(new Rect(_cellWidth * hor_i + horizontal_space * hor_i,
                                         _cellHeight * ver_i + vertical_space * ver_i + _menuHeight,
                                                     _cellWidth, _cellHeight),
                                                 (_selectedObject != null && obj.Equals(_selectedObject)) ? RedactorStyle.BackgraundSelectItem : RedactorStyle.BackgraundItem);

            DrawObject(obj);

            if (GUI.Button(new Rect(_cellWidth * hor_i + horizontal_space * hor_i,
                                    _cellHeight * ver_i + vertical_space * ver_i + _menuHeight,
                                                    _cellWidth, _cellHeight), "", RedactorStyle.Hide))
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _clickTime < 500 && _selectedObject == obj)
                {
                    // Double click
                    ScriptableObject scriptableObject = obj as ScriptableObject;
                    AssetDatabase.OpenAsset(scriptableObject);
                }
                _clickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                SelectObject(obj);
            }

        }

        /// <summary>
        /// Save changes to the registry
        /// </summary>
        private void Save() {
            if (_editableRegistry == null) return;
            AssetDatabase.Refresh();

            for(int i = 0; i < _editableRegistry.Count; i++)
            {
                EditorUtility.SetDirty((ScriptableObject)_editableRegistry[i]);
            }

            EditorUtility.SetDirty((ScriptableObject)_editableRegistry);
            AssetDatabase.SaveAssets();
        }

        public virtual void Export()
        {

        }

        /// <summary>
        /// Create new object
        /// </summary>
        private void CreateObject()
        {
            var findType = _editableRegistry.GetObjectType();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                               .SelectMany(i => i.GetTypes())
                                               .Where(i => findType.IsAssignableFrom(i) && i.IsClass && !i.IsAbstract)
                                               .ToList();

            if(types.Count == 1)
            {
                var obj = _editableRegistry.AddObjectOfType(types[0]);
                SelectObject(obj);
            }
            else ClassSelectorWindow.Display(_editableRegistry, types, obj => SelectObject(obj));
        }

        /// <summary>
        /// Select object to edit in inspector window
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void SelectObject(IDataObject obj)
        {
            _selectedObject = obj;
            Selection.activeObject = (ScriptableObject)_selectedObject;
        }

        /// <summary>
        /// Draw object preview
        /// </summary>
        /// <param name="obj"></param>
        private void DrawObject(IDataObject obj)
        {
            GUILayout.Label(obj.Preview, GUILayout.Width(90), GUILayout.Height(90));
            Color def = GUI.contentColor;
            GUI.contentColor = Color.black;
            GUILayout.Label("ID: " + obj.ID);
            GUILayout.Label(obj.Name);
            GUI.contentColor = def;
            GUILayout.EndArea();
        }

        /// <summary>
        /// Remove the selected object from the registry
        /// </summary>
        private void RemoveSelectObject()
        {
            if (_selectedObject != null)
            {
                if (EditorUtility.DisplayDialog("Delete Item", "Delete item ID: " + _selectedObject.ID + ", Name: " + _selectedObject.Name + "?", "Yes", "No"))
                {
                    _editableRegistry.RemoveObject((IDataObject)_selectedObject);
                    _selectedObject = null;
                    Selection.activeObject = null;
                }
            }
            else EditorUtility.DisplayDialog("Delete Item", "No item selected", "OK");
        }

        [OnOpenAsset(1)]
        public static bool OpenGameStateFlow(int instanceID, int line)
        {
            // Get the asset path and type
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

            // Check if the asset type is NodesContainer
            bool isObjectRegistry = typeof(IDataObjectRegistry).IsAssignableFrom(assetType);

            if (isObjectRegistry)
            {
                // Create and load the window if the asset is a NodesContainer
                WindowDataRegistryEditor window = OpenWindow();
                window.LoadFromFile(assetPath);
            }

            return isObjectRegistry;
        }

        private void LoadFromFile(string assetPath)
        {
            EditorPrefs.SetString(FILE_PATH_KEY, assetPath);
            _editableRegistry = (IDataObjectRegistry)AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        }
    }
}