using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectRegistryEditor
{
    internal static class ReflectionUtility
    {
        internal static FieldInfo FindProperty(Type parentType, string propertyPath)
        {
            string[] properties = propertyPath.Split('.');
            FieldInfo fi = null;
            foreach (var property in properties)
            {
                if (property.Contains("Array")) continue; // Skip array properties
                // Handling for array/list elements, which are denoted by property names like "Array.data[x]"
                if (property.Contains("data["))
                {
                    // Extract the index within the brackets (though not used here, might be useful for future enhancements)
                    var indexString = property.Substring(property.IndexOf('[') + 1, property.IndexOf(']') - property.IndexOf('[') - 1);
                    int index;
                    if (int.TryParse(indexString, out index))
                    {
                        // Assuming the current parentType is an array or a list, we get the element type
                        if (parentType.IsArray)
                        {
                            parentType = parentType.GetElementType(); // For arrays
                        }
                        else if (parentType.IsGenericType && parentType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            parentType = parentType.GetGenericArguments()[0]; // For generic lists
                        }
                        continue; // Skip further processing in this iteration
                    }
                }
                else
                {
                    // Use BindingFlags to search for both public and non-public instance fields
                    fi = parentType.GetField(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi == null)
                    {
                        return null; // Field not found
                    }
                    parentType = fi.FieldType;
                }
            }

            return fi;
        }


    }
}
