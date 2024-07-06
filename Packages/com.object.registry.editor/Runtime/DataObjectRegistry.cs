using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ObjectRegistryEditor
{
    /// <summary>
    /// Container for storing editable objects.
    /// </summary>
    /// <typeparam name="T">Type of the editable objects.</typeparam>
    public abstract class DataObjectRegistry<T> : ScriptableObject, IDataObjectRegistry where T : ScriptableObject, IDataObject, new()
    {
        [SerializeField]
        private List<T> _objects = new List<T>();
        [SerializeField]
        private int _generatorID = 0;

        /// <summary>
        /// Gets the list of editable objects.
        /// </summary>
        public List<T> Objects => _objects;

        /// <summary>
        /// Adds a new editable object with a unique ID.
        /// </summary>
        /// <returns>The added editable object.</returns>
        public void AddObject(Action<IDataObject> action)
        {
#if UNITY_EDITOR

            //Find all the types that implement the T type
            Type[] objectTypes = AppDomain.CurrentDomain.GetAssemblies()
                                                        .SelectMany(i => i.GetTypes())
                                                        .Where(i => typeof(T).IsAssignableFrom(i) && i.IsClass && !i.IsAbstract)
                                                        .ToArray();
            if (objectTypes.Length <= 1)
            {
                T obj = ScriptableObject.CreateInstance<T>();
                SaveObject(obj);
                action?.Invoke(obj);
            }
            else
            {
                //Create a menu to select the type of object to create
                GenericMenu menu = new GenericMenu();
                foreach (Type type in objectTypes)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        T obj = ScriptableObject.CreateInstance(type) as T;
                        SaveObject(obj);
                        action?.Invoke(obj);
                    });
                }
                menu.ShowAsContext();
            }
#else
            action?.Invoke(null);
#endif
        }

        public IDataObject AddObjectOfType(Type typeObject)
        {
            T obj = ScriptableObject.CreateInstance(typeObject) as T;
            SaveObject(obj);
            return obj;
        }

        public U AddObjectOfType<U>() where U : ScriptableObject, IDataObject
        {
            return AddObjectOfType(typeof(U)) as U;
        }

        private void SaveObject(T obj)
        {
#if UNITY_EDITOR
            int id = GetUniqueID(++_generatorID);
            obj.Initialize(id);

            //Save the scriptable object to the folder with the same name as the registry
            string path = GetPathToEditableObjects();

            path = Path.Combine(path, $"{obj.GetType().Name}_{obj.ID}.asset");
            AssetDatabase.CreateAsset(obj, path);
            _objects.Add(obj);
#endif
        }

        private int GetUniqueID(int id)
        {
            while (ContainsKey(id))
            {
                id = ++_generatorID;
            }
            return id;
        }

        /// <summary>
        /// Gets the number of editable objects.
        /// </summary>
        public int Count => _objects.Count;

        /// <summary>
        /// Gets the editable object at the specified index.
        /// </summary>
        public IDataObject this[int index] => _objects[index];

        /// <summary>
        /// Removes the specified editable object.
        /// </summary>
        /// <param name="obj">The editable object to remove.</param>
        public void RemoveObject(IDataObject obj)
        {
#if UNITY_EDITOR
            if (obj != null)
            {
                _objects.Remove((T)obj);
                string path = AssetDatabase.GetAssetPath((ScriptableObject)obj);
                AssetDatabase.DeleteAsset(path);
            }
#endif
        }

        /// <summary>
        /// Determines whether an editable object with the specified ID exists.
        /// </summary>
        /// <param name="id">The ID to check.</param>
        /// <returns>true if an object with the ID exists; otherwise, false.</returns>
        public bool ContainsKey(int id) => _objects.Any(i => i.ID == id);

        /// <summary>
        /// Finds an editable object by its ID.
        /// </summary>
        /// <param name="id">The ID of the object to find.</param>
        /// <returns>The object with the specified ID, or null if not found.</returns>
        public T GetObjectByID(int id) => _objects.Find(i => i.ID == id);

        public virtual void Export() { }
#if UNITY_EDITOR
        private string GetPathToEditableObjects()
        {
            string path = AssetDatabase.GetAssetPath(this);
            path = Path.GetDirectoryName(path);
            string folder = name;
            path = Path.Combine(path, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
#endif

            private void OnValidate()
        {
            Validate();
        }

        private void Validate()
        {
#if UNITY_EDITOR
            //Get all assets in the folder
            string[] assets = AssetDatabase.FindAssets($"t:{typeof(T)}", new[] { GetPathToEditableObjects() });

            //Check if the asset is in the list
            foreach (string guid in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T obj = AssetDatabase.LoadAssetAtPath<T>(path);
                if (_objects.Contains(obj) == false)
                {
                    int id = GetUniqueID(obj.ID);
                  
                    if(id != obj.ID)
                    {
                        obj.Initialize(id);
                        EditorUtility.SetDirty(obj);
                    }
                    _objects.Add(obj);
                    EditorUtility.SetDirty(this);
                }
                if(obj.ID == 0)
                {
                    int id = GetUniqueID(++_generatorID);
                    obj.Initialize(id); 
                    EditorUtility.SetDirty(obj);
                }
                //Check if Oject has unique ID
                if(_objects.Where(i => i.ID == obj.ID).Count() > 1)
                {
                    int id = GetUniqueID(++_generatorID);
                    obj.Initialize(id);
                    EditorUtility.SetDirty(obj);
                }
            }
#endif
        }

        public Type GetObjectType() => typeof(T);
    }
}