using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Jonnidip
{
    public class StringTypeEnumConverter : StringEnumConverter
    {
        public StringTypeEnumConverter() : this(AppDomain.CurrentDomain.GetAssemblies()) { }

        public StringTypeEnumConverter(IEnumerable<Assembly> assemblies) => Helper.Assemblies = assemblies;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    {
                        Helper.GetObjectType(ref reader, ref objectType);

                        return base.ReadJson(reader, objectType, existingValue, serializer);
                    }
                case JsonToken.Integer:
                    if (objectType.Name == "Enum")
                        throw new Exception($"Value '{reader.Value}' cannot be converted to type: {objectType.Name}.");
                    break;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var objectType = value.GetType();

            Helper.CheckLiteral(objectType, value.ToString());

            var enumType = EnumTypeBuilder.CreateType(objectType.Name,
                                                        objectType.GetEnumUnderlyingType(),
                                                        $"{objectType.Name}.{value}", value);
            base.WriteJson(writer, Enum.GetValues(enumType).GetValue(0), serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            var t = Helper.GetTypeDefinition(objectType);

            return t != null && (t.Name == "Enum" || t.IsEnum);
        }

        internal static class Helper
        {
            internal static IEnumerable<Assembly> Assemblies;

            internal static void CheckLiteral(Type type, string literalName)
            {
                if (!type.IsEnum)
                    throw new Exception($"Type '{type.Name}' is not an Enum");

                if (!type.GetEnumNames().Contains(literalName))
                    throw new Exception($"Value '{literalName}' is not part of enum: {type}");
            }

            internal static void GetObjectType(ref JsonReader reader, ref Type objectType)
            {
                var readerValue = (string)reader.Value;

                if (GetTypeValueString(readerValue, out var typeName, out var value))
                {
                    var t = GetTypeDefinition(objectType);

                    if (t.Name == "Enum")
                    {
                        objectType = TypeHelper.FindType(typeName, Assemblies, true);
                        CheckLiteral(objectType, value);
                    }
                    else
                        CheckLiteral(t, value);

                    reader = new JsonTextReader(new StringReader($"{{'{typeName}': '{value}'}}"));
                    while (reader.Read() && reader.TokenType != JsonToken.String)
                    { }

                    return;
                }

                throw new Exception($"Cannot infer type name from string value '{readerValue}'");
            }

            internal static bool GetTypeValueString(string readerValue, out string typeName, out string value)
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

            internal static Type GetTypeDefinition(Type objectType)
                => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? Nullable.GetUnderlyingType(objectType)
                    : objectType;
        }
    }
}