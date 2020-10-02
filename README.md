# StringTypeEnumConverter
A more precise enum converter for Newtonsoft json.

When serializing and deserializing an Enum using Newtonsoft.Json, you may get the problem of knowing what was the specifi enum type.<br>
This may happen if your class declares a property of type Enum, instead of the specific enum.
<br>
<br>
#### The problem:
```<language>
        public class TestClass
        {
            public Enum Enum1 { get; set; }
        }
```
The "Enum1" field is declared as Enum.<br>This allows it to be filled with **any enum**.<br>
If you try to fill it with *StringComparison* enum, using the value *OrdinalIgnoreCase*, the serialized output, will be:
```<language>
        {"Enum1":5}
```
The result cannot be acceptable for common uses, because you actually don't know what enum was used.<br>
The serialization **lost the information**.
<br>
<br>
#### The solution:
This enum converter aims to solve this problem, by extending the out-of-the-box converter "StringEnumConverter", that writes the string value instead of its underlying type (int in this case).<br>
The class we are extending only writes the *string* value of the enum:
```<language>
        {"Enum1":"OrdinalIgnoreCase"}
```
**StringTypeEnumConverter** writes the string value along with the originating enum type name, in this way:
```<language>
        {"Enum1":"StringComparison.OrdinalIgnoreCase"}
```
When deserializing, it looks for the *StringComparison* enum type in a given array of assemblies, and returns that type.
<br>
<br>
<br>
For code and more info: <https://github.com/jonnidip/StringTypeEnumConverter>
