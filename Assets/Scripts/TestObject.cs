using ObjectRegistryEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestNamespace;

namespace Assets.Scripts
{
    [Serializable]
    public class TestObject
    {
        public DataLink<TestData> link;
    }
}
