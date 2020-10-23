using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jonnidip
{
    public static class TypeHelper
    {
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        internal static Type FindType(string typeName, IEnumerable<Assembly> assemblies, bool searchInReferencedAssemblies = false)
        {
            var previouslyListed = new List<string>();

            foreach (var assembly in assemblies)
            {
                var cacheKey = $"{assembly.FullName}.{typeName}";
                if (FindTypeInCache(cacheKey, out var foundType))
                    return foundType;
            }

            foreach (var assembly in assemblies)
            {
                var type = FindType(typeName, assembly, false);
                if (type != null)
                    return type;

                previouslyListed.Add(assembly.FullName);

                if (searchInReferencedAssemblies)
                    foreach (var referencedAssembly in assembly.GetReferencedAssemblies()
                        .Where(w => !previouslyListed.Contains(w.FullName)))
                    {
                        type = FindType(typeName, Assembly.Load(referencedAssembly), false);
                        if (type != null)
                            return type;

                        previouslyListed.Add(referencedAssembly.FullName);
                    }
            }

            throw new Exception($"Cannot find type '{typeName}'");
        }

        internal static Type FindType(string typeName, Assembly assembly, bool checkCache = true)
        {
            var cacheKey = $"{assembly.FullName}.{typeName}";

            if (checkCache && FindTypeInCache(cacheKey, out var foundType))
                    return foundType;

            var type = assembly.GetTypes().FirstOrDefault(f => f.Name == typeName);

            if (type != null)
                lock (TypeCache)
                    TypeCache.Add(cacheKey, type);

            return type;
        }

        private static bool FindTypeInCache(string cacheKey, out Type foundType)
        {
            foundType = null;

            lock (TypeCache)
                if (TypeCache.ContainsKey(cacheKey))
                    foundType = TypeCache[cacheKey];

            return foundType != null;
        }

        internal static Type GetTypeDefinition(Type objectType)
            => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

        internal static void CheckEnumLiteral(Type type, string? literalName)
        {
            if (type.Name == "Enum")
                return;

            if (!type.IsEnum)
                throw new Exception($"Type '{type.Name}' is not an Enum");

            if (literalName == null)
                throw new Exception($"Literal cannot be null for enum: {type}");

            if (!Enum.IsDefined(type, literalName))
                throw new Exception($"Value '{literalName}' is not part of enum: {type}");
        }
    }
}