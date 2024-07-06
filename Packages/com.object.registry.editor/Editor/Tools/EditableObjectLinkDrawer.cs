using ObjectRegistryEditor.SelectorWindow;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ObjectRegistryEditor
{
    public class EditableObjectLinkDrawer<T> : GenericEditableObjectLinkDrawer where T : ScriptableObject, IDataObject
    {
        protected override void InitializeEditableObject(SerializedProperty property)
        {
            var idProperty = property.FindPropertyRelative("_id");
            var allEditableObjects = AssetDatabase.FindAssets("t:" + typeof(T).Name)
                                                  .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                                                  .ToList();
            _editableObject = allEditableObjects.FirstOrDefault(x => x.ID == idProperty.intValue);
            _dataType = typeof(T);
        }
    }
}
