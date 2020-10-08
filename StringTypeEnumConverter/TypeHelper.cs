﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jonnidip
{
    public static class TypeHelper
    {
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        public static Type FindType(string typeName, IEnumerable<Assembly> assemblies, bool searchInReferencedAssemblies = false)
        {
            if (TypeCache.ContainsKey(typeName))
                return TypeCache[typeName];

            var previouslyListed = new List<string>();

            foreach (var assembly in assemblies)
            {
                var type = FindType(typeName, assembly);
                if (type != null)
                    return type;

                previouslyListed.Add(assembly.FullName);

                if (searchInReferencedAssemblies)
                    foreach (var referencedAssembly in assembly.GetReferencedAssemblies()
                        .Where(w => !previouslyListed.Contains(w.FullName)))
                    {
                        type = FindType(typeName, Assembly.Load(referencedAssembly));
                        if (type != null)
                            return type;

                        previouslyListed.Add(referencedAssembly.FullName);
                    }
            }

            throw new Exception($"Cannot find type '{typeName}'");
        }

        public static Type FindType(string typeName, Assembly assembly)
        {
            var cacheKey = $"{assembly.FullName}.{typeName}";
            if (TypeCache.ContainsKey(cacheKey))
                return TypeCache[cacheKey];

            var type = assembly.GetTypes().FirstOrDefault(f => f.Name == typeName);

            TypeCache.Add(cacheKey, type);

            return type;
        }

        public static Type GetTypeDefinition(Type objectType)
            => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

        public static void CheckEnumLiteral(Type type, string literalName)
        {
            if (!type.IsEnum)
                throw new Exception($"Type '{type.Name}' is not an Enum");

            if (!Enum.IsDefined(type, literalName))
                //if (!type.GetEnumNames().Contains(literalName))
                throw new Exception($"Value '{literalName}' is not part of enum: {type}");
        }
    }
}