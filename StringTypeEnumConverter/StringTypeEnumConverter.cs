using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#pragma warning disable 1591

namespace Jonnidip
{
    public class StringTypeEnumConverter : StringEnumConverter
    {
        private static IEnumerable<Assembly>? _assemblies;
        private static IEnumerable<Assembly> Assemblies
        {
            get => _assemblies ??= AppDomain.CurrentDomain.GetAssemblies();
            set => _assemblies = value;
        }

        private static StringTypeEnumConverterBehavior _behavior = StringTypeEnumConverterBehavior.AlwaysUseTypeNameInValue;
        private static Dictionary<string, Type> _knownEnumTypes = new Dictionary<string, Type>();

        public StringTypeEnumConverter()
            : this(null as IEnumerable<Assembly>) { }

        public StringTypeEnumConverter(IEnumerable<Assembly>? assemblies)
            : this(assemblies, null, null) { }

        public StringTypeEnumConverter(IEnumerable<Type> knownEnumTypes)
            : this(null, null, knownEnumTypes) { }

        public StringTypeEnumConverter(StringTypeEnumConverterBehavior behavior)
            : this(null, behavior, null) { }

        public StringTypeEnumConverter(StringTypeEnumConverterBehavior behavior, IEnumerable<Type> knownEnumTypes)
            : this(null, behavior, knownEnumTypes) { }

        public StringTypeEnumConverter(IEnumerable<Assembly>? assemblies, StringTypeEnumConverterBehavior? behavior, IEnumerable<Type>? knownEnumTypes)
        {
            Assemblies = assemblies;

            if (behavior != null)
                _behavior = (StringTypeEnumConverterBehavior)behavior;

            if (knownEnumTypes != null)
                lock (_knownEnumTypes)
                    _knownEnumTypes = knownEnumTypes.ToDictionary(x => x.Name, x => x);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    GetObjectType(ref reader, ref objectType);
                    break;
                case JsonToken.Integer:
                    if (objectType.Name == "Enum")
                        throw new Exception($"Value '{reader.Value}' cannot be converted to type: {objectType.Name}.");
                    break;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var objectType = value.GetType();
            var convertType = false;

            switch (_behavior)
            {
                case StringTypeEnumConverterBehavior.AlwaysUseTypeNameInValue:
                    convertType = true;
                    break;
                case StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly:
                    convertType = GetDestinationType(writer, serializer).Name == "Enum";
                    break;
            }

            if (convertType)
            {
                TypeHelper.CheckEnumLiteral(objectType, value.ToString());

                var enumType = EnumTypeBuilder.CreateType(objectType.Name,
                    objectType.GetEnumUnderlyingType(),
                    $"{objectType.Name}.{value}", value);

                value = Enum.GetValues(enumType).GetValue(0);
            }

            base.WriteJson(writer, value, serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            var t = TypeHelper.GetTypeDefinition(objectType);

            return t != null && (t.Name == "Enum" || t.IsEnum);
        }

        private static void GetObjectType(ref JsonReader reader, ref Type objectType)
        {
            var readerValue = (string)reader.Value;

            var t = TypeHelper.GetTypeDefinition(objectType);

            if (GetTypeValueString(readerValue, out var typeName, out var value))
            {
                if (t.Name == "Enum")
                {
                    lock (_knownEnumTypes)
                        if (!_knownEnumTypes.TryGetValue(typeName, out objectType))
                            objectType = TypeHelper.FindType(typeName, Assemblies, true);

                    TypeHelper.CheckEnumLiteral(objectType, value);
                }
                else
                    TypeHelper.CheckEnumLiteral(t, value);

                reader = new JsonTextReader(new StringReader($"{{'{typeName}': '{value}'}}"));
                while (reader.Read() && reader.TokenType != JsonToken.String)
                { }

                return;
            }

            if (t.Name != "Enum" && _behavior == StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly)
            {
                TypeHelper.CheckEnumLiteral(t, readerValue);
                return;
            }

            throw new Exception($"Cannot infer type name from string value '{readerValue}'");
        }

        private static bool GetTypeValueString(string readerValue, out string typeName, out string value)
        {
            typeName = string.Empty;
            value = string.Empty;

            if (!string.IsNullOrWhiteSpace(readerValue)
                && readerValue.Contains("."))
            {
                var typeValue = readerValue.Split('.');
                typeName = typeValue[0];
                value = typeValue[1];

                return true;
            }

            return false;
        }

        private static Type GetDestinationType(JsonWriter writer, JsonSerializer serializer)
        {
            var serializeStack = GetSerializeStack(serializer).First();
            var properties = serializeStack.GetType().GetProperties();

            var current = writer.Path.Split('.')
                .Aggregate<string, PropertyInfo>(null, (current1, s) => current1 == null
                    ? properties.First(f => f.Name == s)
                    : current1.PropertyType.GetProperties().First(f => f.Name == s));

            return TypeHelper.GetTypeDefinition(current.PropertyType);
        }

        private static IEnumerable<object> GetSerializeStack(JsonSerializer serializer)
        {
            var internalSerializer = serializer.GetType()
                .GetMethod("GetInternalSerializer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(serializer, null);

            var serializeStack = internalSerializer?.GetType()
                .GetField("_serializeStack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(internalSerializer) as List<object>;

            return serializeStack;
        }
    }

    public enum StringTypeEnumConverterBehavior
    {
        AlwaysUseTypeNameInValue,
        UseTypeNameInValueForStrictEnumsOnly
    }
}