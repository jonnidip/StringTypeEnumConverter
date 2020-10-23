using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Jonnidip
{
    internal static class EnumTypeBuilder
    {
        private const TypeAttributes EnumTypeAttributes = TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed;
        private const FieldAttributes ValueFieldAttributes = FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName;
        private const FieldAttributes FieldBuilderTypeAttributes = FieldAttributes.Static | FieldAttributes.Public | FieldAttributes.Literal;
        private static readonly Dictionary<string, Type> Cache = new Dictionary<string, Type>();

        public static Type CreateType(string typeName, Type underlyingType, string literalName, object value)
        {
            if (Cache.TryGetValue(literalName, out var cachedValue))
                return cachedValue;

            var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName { Name = "TempAssembly" }, AssemblyBuilderAccess.Run)
                .DefineDynamicModule("Temp");

            var enumTypeBuilder = builder.DefineType(typeName,
                                                    EnumTypeAttributes, 
                                                    typeof(Enum), 
                                                    PackingSize.Unspecified, 
                                                    TypeBuilder.UnspecifiedTypeSize);
            enumTypeBuilder.DefineField("value__", underlyingType, ValueFieldAttributes);

            var enumerationFieldBuilder = enumTypeBuilder.DefineField(literalName, enumTypeBuilder, FieldBuilderTypeAttributes);
            enumerationFieldBuilder.SetConstant(Convert.ChangeType(value, underlyingType));

            var enumType = enumTypeBuilder.CreateType();
            if (!Cache.ContainsKey(literalName))
                Cache.Add(literalName, enumType);

            return enumType;
        }
    }
}