# StringTypeEnumConverter
An enum converter for Newtonsoft json.

When serializing and deserializing an Enum using Newtonsoft.Json, you may get the problem of knowing where that enum was coming from.
This may happen if your class declares a property of type Enum, instead of the specific enum.

Example:
        public class TestClass
        {
            public Enum Enum1 { get; set; }
        }
The "Enum1" field is declared as Enum. This allows it to be filled with any enum.
If we try to fill it with StringComparison enum, using the value OrdinalIgnoreCase, the serialized output, will be:
        {"Enum1":5}
The result cannot be acceptable for common uses, because you need to know what enum was used, and the json serialization does not have that info.

This enum converter aims to solve this problem, by extending the out-of-the-box converter "StringEnumConverter", that writes the string value instead of its underlying type (int in this case).
StringTypeEnumConverter writes the string value along with the enum name, in this way:
        {"Enum1":"StringComparison.OrdinalIgnoreCase"}
When deserializing, it looks for the "StringComparison" enum type in a given array of assemblies, and returns that type.
