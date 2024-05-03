using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectRegistryEditor
{
    public interface IEditableObjectRegistry
    {
        public IEditableObject this[int index] { get; }
        int Count { get; }

        IEditableObject AddObject();
        void RemoveObject(IEditableObject oBJ);
        void Export();
    }
}
