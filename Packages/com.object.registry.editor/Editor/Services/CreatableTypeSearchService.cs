using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace ObjectRegistryEditor.Services
{
    [InitializeOnLoad]
    internal static class CreatableTypeSearchService
    {
        private static readonly Dictionary<Type, List<Type>> _cache = new Dictionary<Type, List<Type>>();

        static CreatableTypeSearchService()
        {
            AssemblyReloadEvents.afterAssemblyReload += ClearCache;
            EditorApplication.projectChanged += ClearCache;
        }

        public static IReadOnlyList<Type> GetCreatableTypes(Type baseType)
        {
            if (baseType == null)
            {
                return Array.Empty<Type>();
            }

            if (_cache.TryGetValue(baseType, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }

            List<Type> types = FindCreatableTypes(baseType);
            _cache[baseType] = types;
            return types;
        }

        public static IReadOnlyList<Type> Refresh(Type baseType)
        {
            if (baseType == null)
            {
                return Array.Empty<Type>();
            }

            List<Type> types = FindCreatableTypes(baseType);
            _cache[baseType] = types;
            return types;
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }

        private static List<Type> FindCreatableTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(type => baseType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToList();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }
    }
}
