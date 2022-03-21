#nullable enable
using System;
using Xunit;
using U8Xml;

namespace UnitTest
{
    public class ParseAttributeTest
    {
        [Fact]
        public void ParseAttribute()
        {
            var xmlString =
@"<root>
    <n0 abc=""123""/>
    <n1 abc =""123""/>
    <n2 abc  =""123""/>
    <n3 abc= ""123""/>
    <n4 abc = ""123""/>
    <n5 abc  = ""123""/>
    <n6 abc=  ""123""/>
    <n7 abc =  ""123""/>
    <n8 abc  =  ""123""/>
    <n9 abc
       =""123""/>
    <n10 abc=
        ""123""/>
    <n11 abc 
       =
        ""123""/>
</root>";

            using var xml = XmlParser.Parse(xmlString);
            var root = xml.Root;

            ReadOnlySpan<byte> name_abc = stackalloc byte[3] { (byte)'a', (byte)'b', (byte)'c' };
            ReadOnlySpan<byte> value_123 = stackalloc byte[3] { (byte)'1', (byte)'2', (byte)'3' };

            foreach(var n in root.Children) {
                var (name, value) = n.FindAttribute(name_abc);
                Assert.True(value == value_123);
                Assert.True(name == name_abc);
            }
        }

        [Fact]
        public void ParseAttributeFail()
        {
            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(@"<root><foo =""123"" /></root>");
            });

            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(@"<root><foo abc=123 /></root>");
            });

            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(@"<root><foo abc='123"" /></root>");
            });

            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(@"<root><foo abc=""123' /></root>");
            });
        }
    }
}
