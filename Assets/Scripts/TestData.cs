using ObjectRegistryEditor;
using ObjectRegistryEditor.SerializeReferenceList;
using UnityEngine;

namespace TestNamespace
{
    [CreateAssetMenu(fileName = "TestData", menuName = "ScriptableObjects/Test Data", order = 1)]
    [ItemDetails("Base class for test data.")]
    public class TestData : ScriptableObject, IDataObject
    {
        [SerializeField]
        private int _id;
        [SerializeField]
        private Texture _preview;
        [SerializeField]
        private string _name;
        [SerializeField]
        private DataLink<TestData> _dataLink;
        [SerializeField]
        private RefList<SimpleObjectData> _simpleObjectDataList = new RefList<SimpleObjectData>();

        public int ID => _id;
        public string Name => _name;
        public Texture Preview => _preview;
        public RefList<SimpleObjectData> SimpleObjectDataList => _simpleObjectDataList;

        public void Initialize(int iD)
        {
            _id = iD;
        }
    }
}