using FluentAssertions;
using Jonnidip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace StringTypeEnumConverterTests
{
    public class ConversionTests
    {
        public static IEnumerable<object[]> CommonData
            => new List<object[]>
               {
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}", new TestClass
                                                                                                                                     {
                                                                                                                                         Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                                                         Enum2 = StringSplitOptions.None,
                                                                                                                                         SubClass = null
                                                                                                                                     }
                   },
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":{\"Enum1\":\"ConsoleModifiers.Alt\",\"Enum2\":\"ConsoleColor.Black\"}}", new TestClass
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
                   new object[]
                   {
                       "{\"Enum1\":null,\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}", new TestClass
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
                new object[]
                {
                    "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":null,\"SubClass\":null}", new TestClass
                                                                                                           {
                                                                                                               Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                               Enum2 = null,
                                                                                                               SubClass = null
                                                                                                           }
                }
            };

        public static IEnumerable<object[]> SerializeDataStrictEnumOnlyBehavior =>
            new List<object[]>
            {
                new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"None\",\"SubClass\":null}", new TestClass
                                                                                                                                     {
                                                                                                                                         Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                                                         Enum2 = StringSplitOptions.None,
                                                                                                                                         SubClass = null
                                                                                                                                     }
                   },
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"None\",\"SubClass\":{\"Enum1\":\"ConsoleModifiers.Alt\",\"Enum2\":\"Black\"}}", new TestClass
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
                   new object[]
                   {
                       "{\"Enum1\":null,\"Enum2\":\"None\",\"SubClass\":null}", new TestClass
                                                                                                   {
                                                                                                       Enum1 = null,
                                                                                                       Enum2 = StringSplitOptions.None,
                                                                                                       SubClass = null
                                                                                                   }
                   },
                new object[]
                {
                    "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":null,\"SubClass\":null}", new TestClass
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
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}", new TestClass
                                                                                                                                     {
                                                                                                                                         Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                                                         Enum2 = StringSplitOptions.None,
                                                                                                                                         SubClass = null
                                                                                                                                     }
                   },
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":0,\"SubClass\":null}", new TestClass
                                                                                                           {
                                                                                                               Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                               Enum2 = StringSplitOptions.None,
                                                                                                               SubClass = null
                                                                                                           }
                   }
               };

        public static IEnumerable<object[]> DeserializeDataStrictEnumOnlyBehavior
            => new List<object[]>
               {
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"None\",\"SubClass\":null}", new TestClass
                                                                                                                                     {
                                                                                                                                         Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                                                         Enum2 = StringSplitOptions.None,
                                                                                                                                         SubClass = null
                                                                                                                                     }
                   },
                   new object[]
                   {
                       "{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":0,\"SubClass\":null}", new TestClass
                                                                                                           {
                                                                                                               Enum1 = StringComparison.OrdinalIgnoreCase,
                                                                                                               Enum2 = StringSplitOptions.None,
                                                                                                               SubClass = null
                                                                                                           }
                   }
               };

        [Theory]
        [MemberData(nameof(CommonData))]
        [MemberData(nameof(SerializeData))]
        public void JsonConvert_SerializeObject_ReturnsExpectedValue(string expectedValue, TestClass testClass)
        {
            var serialized = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.AlwaysUseTypeNameInValue));

            Assert.Equal(expectedValue, serialized);
        }

        [Theory]
        [MemberData(nameof(SerializeDataStrictEnumOnlyBehavior))]
        public void JsonConvert_SerializeObject_ReturnsExpectedValue_StrictEnumOnlyBehavior(string expectedValue, TestClass testClass)
        {
            var serialized = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly));

            Assert.Equal(expectedValue, serialized);
        }

        [Theory]
        [MemberData(nameof(CommonData))]
        [MemberData(nameof(DeserializeData))]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue(string serializedValue, TestClass result)
        {
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter());

            result.Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue_KnownEnumType()
        {
            var testClass = new TestClass
            {
                Enum1 = TestEnum.Value1,
                Enum2 = StringSplitOptions.None,
                SubClass = null
            };

            const string serializedValue = "{\"Enum1\":\"TestEnum.Value1\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(new List<Type> { typeof(TestEnum) }));

            testClass.Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue_DifferentBehaviors()
        {
            var testClass = new TestClass
            {
                Enum1 = TestEnum.Value1,
                Enum2 = StringSplitOptions.None,
                SubClass = null
            };

            const string serializedValue = "{\"Enum1\":\"TestEnum.Value1\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly, new List<Type> { typeof(TestEnum) }));

            testClass.Should().BeEquivalentTo(deserialized);

            var deserialized2 = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.AlwaysUseTypeNameInValue, new List<Type> { typeof(TestEnum) }));

            testClass.Should().BeEquivalentTo(deserialized2);
        }

        [Fact]
        public void JsonConvert_SerializeObject_ReturnsExpectedValue_DifferentBehaviors()
        {
            var testClass = new TestClass
            {
                Enum1 = TestEnum.Value1,
                Enum2 = StringSplitOptions.None,
                SubClass = null
            };

            const string expectedSerialization1 = "{\"Enum1\":\"TestEnum.Value1\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var serializedValue1 = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.AlwaysUseTypeNameInValue, new List<Type> { typeof(TestEnum) }));

            Assert.Equal(expectedSerialization1, serializedValue1);

            const string expectedSerialization2 = "{\"Enum1\":\"TestEnum.Value1\",\"Enum2\":\"None\",\"SubClass\":null}";
            var serializedValue2 = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly, new List<Type> { typeof(TestEnum) }));

            Assert.Equal(expectedSerialization2, serializedValue2);
        }

        [Theory]
        [MemberData(nameof(DeserializeDataStrictEnumOnlyBehavior))]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue_StrictEnumOnlyBehavior(string serializedValue, TestClass result)
        {
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(StringTypeEnumConverterBehavior.UseTypeNameInValueForStrictEnumsOnly));

            result.Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ThrowsException_If_NumericValue_CannotBeConverted()
        {
            const string serializedValue = "{\"Enum1\":5,\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";

            var exception = Assert.Throws<Exception>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter()));
            Assert.Equal("Value '5' cannot be converted to type: Enum.", exception.Message);
        }

        [Theory]
        [InlineData("{\"Enum1\":\"StringComparison.OrdinalIgnoreCase\",\"Enum2\":\"StringSplitOptions.NonExisting\",\"SubClass\":null}")]
        [InlineData("{\"Enum1\":\"StringComparison.NonExisting\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}")]
        public void JsonConvert_DeserializeObject_ThrowsException_If_LiteralDoesNotBelongToEnum(string serializedValue)
        {
            var exception = Assert.Throws<Exception>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter()));
            Assert.StartsWith("Value 'NonExisting' is not part of enum:", exception.Message);
        }

        [Fact]
        public void JsonConvert_DeserializeObject_ThrowsException_If_TypeNotFound()
        {
            const string serializedValue = "{\"Enum1\":\"NonExisting.Value\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            var exception = Assert.Throws<Exception>(() => JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter()));
            Assert.Equal("Cannot find type 'NonExisting'", exception.Message);
        }

        [Fact]
        public void TypeCache_Is_FilledAsExpected()
        {
            var typeHelper = typeof(TypeHelper);
            var typeCacheProperty = typeHelper.GetField("TypeCache", BindingFlags.NonPublic | BindingFlags.Static);
            var value = typeCacheProperty?.GetValue(typeHelper) as Dictionary<string, Type>;

            Assert.NotNull(value);
            Assert.False(value.ContainsValue(typeof(StringEscapeHandling)));

            const string serialized = "{\"Enum1\":\"StringEscapeHandling.Default\",\"Enum2\":\"StringSplitOptions.None\",\"SubClass\":null}";
            JsonConvert.DeserializeObject<TestClass>(serialized, new StringTypeEnumConverter());

            Assert.True(value.ContainsValue(typeof(StringEscapeHandling)));

            JsonConvert.DeserializeObject<TestClass>(serialized, new StringTypeEnumConverter());
            //TODO: Check if value is being read from TypeCache.
        }

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

        public enum TestEnum
        {
            Value1
        }
    }
}