#nullable enable
using U8Xml;
using Xunit;

namespace UnitTest
{
    public class DataLocationTest
    {
        [Fact]
        public void RangeTest()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>あいう</aaa>
    <bbb>
        <ccc zzz='98765' />
    </bbb>
    <ddd>This is
a multi
line
text</ddd>
</root>";
            using var xml = XmlParser.Parse(xmlString);

            // Range of nodes
            foreach(var node in xml.GetAllNodes(null)) {
                CheckNode(xml, node);
            }

            // Range of attributes
            {
                var attr = xml.Root.FindAttribute("xyz");
                CheckAttr(xml, attr);
                Assert.Equal("xyz=\"321\"", attr.AsRawString().ToString());
            }
            {
                var attr = xml.Root.FindChild("bbb").FindChild("ccc").FindAttribute("zzz");
                CheckAttr(xml, attr);
                Assert.Equal("zzz='98765'", attr.AsRawString().ToString());
            }

            // Range of RawString
            {
                var str = xml.Root.FindChild("aaa").InnerText;
                CheckRawStr(xml, str);
                Assert.Equal("あいう", str.ToString());
            }
            {
                var str = xml.Root.FindChild("ddd").InnerText;
                CheckRawStr(xml, str);
                Assert.Equal(@"This is
a multi
line
text", str.ToString());
            }

            return;

            static void CheckNode(XmlObject xml, XmlNode node)
            {
                var xmlRawStr = xml.AsRawString();
                var range = xml.GetRange(node);
                var sliced = xmlRawStr.Slice(range.ByteOffset, range.ByteLength);
                var answer = node.AsRawString();
                Assert.Equal(answer, sliced);
                Assert.Equal(answer.Ptr, sliced.Ptr);
                Assert.Equal(answer.Length, sliced.Length);
            }

            static void CheckAttr(XmlObject xml, XmlAttribute attr)
            {
                var xmlRawStr = xml.AsRawString();
                var range = xml.GetRange(attr);
                var sliced = xmlRawStr.Slice(range.ByteOffset, range.ByteLength);
                var answer = attr.AsRawString();
                Assert.Equal(answer, sliced);
                Assert.Equal(answer.Ptr, sliced.Ptr);
                Assert.Equal(answer.Length, sliced.Length);
            }

            static void CheckRawStr(XmlObject xml, RawString str)
            {
                var xmlRawStr = xml.AsRawString();
                var range = xml.GetRange(str);
                var sliced = xmlRawStr.Slice(range.ByteOffset, range.ByteLength);
                var answer = str;
                Assert.Equal(answer, sliced);
                Assert.Equal(answer.Ptr, sliced.Ptr);
                Assert.Equal(answer.Length, sliced.Length);
            }
        }

        [Fact]
        public void LocationTest()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>あいう</aaa>
    <bbb>
        <ccc zzz='98765' />
    </bbb>
    <ddd>This is
a multi
line
text</ddd>
</root>";

            using var xml = XmlParser.Parse(xmlString);

            var l = xml.GetLocation(xml.Root, false);
            Assert.Equal(1, l.Start.Line);
            Assert.Equal(1, l.Start.Position);
            Assert.Equal(11, l.End.Line);
            Assert.Equal(7, l.End.Position);
        }
    }
}
