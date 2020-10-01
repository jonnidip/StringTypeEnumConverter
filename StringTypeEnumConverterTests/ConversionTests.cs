using FluentAssertions;
using Jonnidip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace StringTypeEnumConverterTests
{
    public class ConversionTests
    {
        [Theory]
        [MemberData(nameof(SerializeData))]
        public void JsonConvert_SerializeObject_ReturnsExpectedValue(string expectedValue, TestClass testClass)
        {
            var serialized = JsonConvert.SerializeObject(testClass, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));

            Assert.Equal(expectedValue, serialized);
        }

        [Theory]
        [MemberData(nameof(SerializeData))]
        public void JsonConvert_DeserializeObject_ReturnsExpectedValue(string serializedValue, TestClass result)
        {
            var deserialized = JsonConvert.DeserializeObject<TestClass>(serializedValue, new StringTypeEnumConverter(AppDomain.CurrentDomain.GetAssemblies()));

            result.Should().BeEquivalentTo(deserialized);
        }

        public static IEnumerable<object[]> SerializeData =>
            new List<object[]>
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
                //new object[] { -2, 2, 0 },
                //new object[] { int.MinValue, -1, int.MaxValue },
            };

        public class TestClass
        {
            public Enum Enum1 { get; set; }

            public StringSplitOptions Enum2 { get; set; }

            public TestSubClass SubClass { get; set; }
        }

        public class TestSubClass
        {
            public Enum Enum1 { get; set; }

            public ConsoleColor Enum2 { get; set; }
        }
    }
}