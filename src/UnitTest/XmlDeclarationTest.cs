#nullable enable
using Xunit;
using U8Xml;
using System;

namespace UnitTest
{
    public class XmlDeclarationTest
    {
        [Fact]
        public void NoXmlDeclaration()
        {
            // No errors.
            const string XmlString = @"<root></root>";
            using var xml = XmlParser.Parse(XmlString);
        }

        [Fact]
        public void ValidXmlDeclaration()
        {
            // No errors.
            const string XmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<root></root>";
            using var xml = XmlParser.Parse(XmlString);
        }

        [Fact]
        public void MultiXmlDeclaration()
        {
            const string XmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<?xml version=""1.0"" encoding=""UTF-8""?>
<root></root>";
            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(XmlString);
            });
        }

        [Fact]
        public void InvalidPosition()
        {
            const string XmlString =
@"<root></root>
<?xml version=""1.0"" encoding=""UTF-8""?>";
            Assert.Throws<FormatException>(() =>
            {
                using var xml = XmlParser.Parse(XmlString);
            });
        }
    }
}
