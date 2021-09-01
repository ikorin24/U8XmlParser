#nullable enable
using Xunit;
using U8Xml;

namespace UnitTest
{
    public class XmlAttributesTest
    {
        [Fact]
        public void CopyTo()
        {
            const string XmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Node a0='0' a1='1' a2='2' a3='3'>
</Node>
";

            using(var xml = XmlParser.Parse(XmlString)) {
                var root = xml.Root;
                var count = root.Attributes.Count;
                Assert.Equal(4, count);
                var copies = new XmlAttribute[count];
                root.Attributes.CopyTo(copies);
                return;
            }
        }
    }
}
