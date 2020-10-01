using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Jonnidip
{
    public class StringTypeEnumConverter : StringEnumConverter
    {
        private readonly Assembly[] _assemblies;

        public StringTypeEnumConverter(Assembly[] assemblies) => _assemblies = assemblies;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    {
                        var typeValue = ((string)reader.Value ?? string.Empty).Split('.');

                        var newTypeName = typeValue[0];
                        var newValue = typeValue[1];

                        var newReader = new JsonTextReader(new StringReader($"{{'{newTypeName}': '{newValue}'}}"));
                        while (newReader.Read() && newReader.TokenType != JsonToken.String)
                        { }

                        var newType = TypeHelper.FindType(newTypeName, _assemblies, true);

                        return base.ReadJson(newReader, newType, existingValue, serializer);
                    }
                default:
                    return base.ReadJson(reader, objectType, existingValue, serializer);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var objectType = value.GetType();

            CheckLiteral(objectType, value.ToString());

            var enumType = EnumTypeBuilder.CreateType(objectType.Name, 
                                                        objectType.GetEnumUnderlyingType(), 
                                                        $"{objectType.Name}.{value}", value);
            base.WriteJson(writer, Enum.GetValues(enumType).GetValue(0), serializer);
        }

        private static void CheckLiteral(Type type, string literalName)
        {
            if (!type.GetEnumNames().Contains(literalName))
                throw new Exception($"Value {literalName} is not part of enum: {type}");
        }

        public override bool CanConvert(Type objectType)
        {
            var t = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return t != null && (t.Name == "Enum" || t.IsEnum);
        }
    }
}