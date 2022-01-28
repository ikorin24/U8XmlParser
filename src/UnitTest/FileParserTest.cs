#nullable enable
using System;
using System.Text;
using Xunit;
using U8Xml;
using U8Xml.Unsafes;

namespace UnitTest
{
    public class FileParserTest
    {
        [Theory]
        [InlineData(@"TestFiles/test_utf8.xml")]
        public void ParseFile(string filePath)
        {
            using(var xml = XmlParser.ParseFile(filePath)) {
                CheckFileContents(xml.Root);
            }
        }

        [Theory]
        [InlineData(@"TestFiles/test_utf8.xml", 0)]
        [InlineData(@"TestFiles/test_utf8_with_bom.xml", 1)]
        [InlineData(@"TestFiles/test_utf16_le.xml", 2)]
        [InlineData(@"TestFiles/test_utf16_be.xml", 3)]
        public void ParseFileWithEncoding(string filePath, int encodingNum)
        {
            var encoding = encodingNum switch
            {
                0 => Encoding.UTF8,
                1 => Encoding.UTF8,
                2 => Encoding.Unicode,
                3 => Encoding.BigEndianUnicode,
                _ => throw new NotImplementedException(),
            };

            using(var xml = XmlParser.ParseFile(filePath, encoding)) {
                CheckFileContents(xml.Root);
            }
        }

        [Theory]
        [InlineData(@"TestFiles/test_utf8.xml")]
        public void ParseFileUnsafe(string filePath)
        {
            using(var xml = XmlParserUnsafe.ParseFileUnsafe(filePath)) {
                CheckFileContents(xml.Root);
            }
        }

        [Theory]
        [InlineData(@"TestFiles/test_utf8.xml", 0)]
        [InlineData(@"TestFiles/test_utf8_with_bom.xml", 1)]
        [InlineData(@"TestFiles/test_utf16_le.xml", 2)]
        [InlineData(@"TestFiles/test_utf16_be.xml", 3)]
        public void ParseFileUnsafeWithEncoding(string filePath, int encodingNum)
        {
            var encoding = encodingNum switch
            {
                0 => Encoding.UTF8,
                1 => Encoding.UTF8,
                2 => Encoding.Unicode,
                3 => Encoding.BigEndianUnicode,
                _ => throw new NotImplementedException(),
            };

            using(var xml = XmlParserUnsafe.ParseFileUnsafe(filePath, encoding)) {
                CheckFileContents(xml.Root);
            }
        }

        private static void CheckFileContents(XmlNode root)
        {
            Assert.Equal("Sample", root.Name.ToString());
            Assert.Equal("香風智乃", root.FindAttribute("TestString").Value.ToString());
        }
    }
}
