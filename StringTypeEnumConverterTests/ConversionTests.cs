using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Jonnidip;
using Newtonsoft.Json;
using Xunit;

namespace StringTypeEnumConverterTests
{
    public class ConversionTests
    {
        [Theory]
        [MemberData(nameof(CommonData))]
        [MemberData(nameof(SerializeData))]
        public void JsonConvert_SerializeObject_ReturnsExpectedValue(string expectedValue, TestClass testClass)
        {
            var serialized = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));

            Assert.Equal(expectedValue, serialized);
        }

        [Theory]
        [MemberData(nameof(CommonData))]
        [MemberData(nameof(DeserializeData))]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue(string serializedValue, TestClass result)
        {
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));

            result.Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ThrowsException_If_NumericValue_CannotBeConverted()
        {
            const string serializedValue = "{\"Enum1\":5,\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";

            var exception = Assert.Throws<Exception>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies())));
            Assert.Equal("Value 5 cannot be converted to Enum.", exception.Message);
        }

        [Theory]
        [InlineData("{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.NonExisting\",\"SubClass\":null}")]
        [InlineData("{\"Enum1\":\"StringComparison.NonExisting\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}")]
        public void JsonConvert_DeserializeObject_ThrowsException_If_LiteralDoesNotBelongToEnum(string serializedValue)
        {
            var exception = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies())));
            Assert.StartsWith("Error converting value \"NonExisting\" to type", exception.Message);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ThrowsException_If_TypeNotFound()
        {
            var serializedValue = "{\"Enum1\":\"NonExisting.Value\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var exception = Assert.Throws<Exception>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies())));
            Assert.Equal("Cannot find type NonExisting", exception.Message);
        }

        [Fact]
        public void TypeCache_Is_FilledAsExpected()
        {
            var typeHelper = typeof(TypeHelper);
            var typeCacheProperty = typeHelper.GetField("TypeCache", BindingFlags.NonPublic | BindingFlags.Static);
            var value = typeCacheProperty.GetValue(typeHelper) as Dictionary<string, Type>;

            Assert.NotNull(value);
            Assert.Empty(value);

            const string serialized = "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var deserialized1 = JsonConvert.DeserializeObject<TestClass>(serialized, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));

            Assert.True(value.ContainsValue(typeof(StringComparison)));

            var deserialized2 = JsonConvert.DeserializeObject<TestClass>(serialized, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));
            //TODO: Check if value is being read from TypeCache.
        }

        public static IEnumerable<object[]> CommonData
            => new List<object[]>
               {
                   new object[] { "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}",
                                    new TestClass
                                    {
                                        Enum1 = StringComparison.OrdinalIgnoreCase,
                                        Enum2 = StringSplitOptions.None,
                                        SubClass = null
                                    }
                                },
                   new object[] { "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":{\"Enum1\":\"ConsoleModifiers.Alt\",\"Enum2\":\"ConsoleColor.Black\"}}",
                                    new TestClass
                                    {
                                        Enum1 = StringComparison.OrdinalIgnoreCase,
                                        Enum2 = StringSplitOptions.None,
                                        SubClass = new TestSubClass
                                                   {
                                                       Enum1 = ConsoleModifiers.Alt,
                                                       Enum2 = ConsoleColor.Black
                                                   }
                                    }
                                },
                   new object[] { "{\"Enum1\":null,\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}",
                                    new TestClass
                                    {
                                        Enum1 = null,
                                        Enum2 = StringSplitOptions.None,
                                        SubClass = null
                                    }
                                }
               };

        public static IEnumerable<object[]> SerializeData =>
            new List<object[]>
            {
                new object[] { "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":null,\"SubClass\":null}",
                                 new TestClass
                                 {
                                     Enum1 = StringComparison.OrdinalIgnoreCase,
                                     Enum2 = null,
                                     SubClass = null
                                 }
                             }
            };

        public static IEnumerable<object[]> DeserializeData
            => new List<object[]>
               {
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}",
                       new TestClass
                       {
                           Enum1 = StringComparison.OrdinalIgnoreCase,
                           Enum2 = StringSplitOptions.None,
                           SubClass = null
                       }
                   },
                   new object[] { "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":0,\"SubClass\":null}",
                        new TestClass
                        {
                            Enum1 = StringComparison.OrdinalIgnoreCase,
                            Enum2 = StringSplitOptions.None,
                            SubClass = null
                        }
                    }
               };

        public class TestClass
        {
            public Enum Enum1 { get; set; }

            public StringSplitOptions? Enum2 { get; set; }

            public TestSubClass SubClass { get; set; }
        }

        public class TestSubClass
        {
            public Enum Enum1 { get; set; }

            public ConsoleColor Enum2 { get; set; }
        }
    }
}