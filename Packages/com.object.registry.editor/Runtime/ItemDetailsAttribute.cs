using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectRegistryEditor
{
    public class ItemDetailsAttribute : Attribute
    {
        private string _description;

        public string Description => _description;

        public ItemDetailsAttribute(string description)
        {
            _description = description;
        }
    }
}
