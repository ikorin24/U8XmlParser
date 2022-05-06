#nullable enable
using System;
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
                Assert.True(xml.AsRawString(xml.GetRange(node)).ReferenceEquals(node.AsRawString()));
            }

            // Range of attributes
            {
                var attr = xml.Root.FindAttribute("xyz");
                var attrStr = attr.AsRawString();
                Assert.True(xml.AsRawString(xml.GetRange(attr)).ReferenceEquals(attrStr));
                Assert.Equal("xyz=\"321\"", attrStr.ToString());
            }
            {
                var attr = xml.Root.FindChild("bbb").FindChild("ccc").FindAttribute("zzz");
                var attrStr = attr.AsRawString();
                Assert.True(xml.AsRawString(xml.GetRange(attr)).ReferenceEquals(attrStr));
                Assert.Equal("zzz='98765'", attrStr.ToString());
            }

            // Range of RawString
            {
                var str = xml.Root.FindChild("aaa").InnerText;
                Assert.True(xml.AsRawString(xml.GetRange(str)).ReferenceEquals(str));
                Assert.Equal("あいう", str.ToString());
            }
            {
                var str = xml.Root.FindChild("ddd").InnerText;
                Assert.True(xml.AsRawString(xml.GetRange(str)).ReferenceEquals(str));
                Assert.Equal(@"This is
a multi
line
text", str.ToString());
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

            var cases = new[]
            {
                (
                    // <root>...</root>
                    Node: xml.Root,
                    Answer: (Start: new DataLinePosition(0, 0), End: new DataLinePosition(10, 7))
                ),
                (
                    // foo --- 3 characters, 3 bytes
                    Node: xml.Root.GetChildren(XmlNodeType.TextNode).First(),
                    Answer: (Start: new DataLinePosition(1, 4), End: new DataLinePosition(1, 7))
                ),
                (
                    // <aaa>あいう</aaa> --- 14 characters, 20 bytes
                    Node: xml.Root.FindChild("aaa"),
                    Answer: (Start: new DataLinePosition(2, 4), End: new DataLinePosition(2, 4 + 14))
                ),
                (
                    // あいう --- 3 characters, 9 bytes
                    Node: xml.Root.FindChild("aaa").GetChildren(XmlNodeType.TextNode).First(),
                    Answer: (Start: new DataLinePosition(2, 9), End: new DataLinePosition(2, 9 + 3))
                ),
                (
                    // <ddd>...</ddd>
                    Node: xml.Root.FindChild("ddd"),
                    Answer: (Start: new DataLinePosition(6, 4), End: new DataLinePosition(9, 10))
                ),
            };

            {
                var (node, answer) = cases[0];
                var location = xml.GetLocation(node);
                Assert.Equal(answer.Start, location.Start);
                Assert.Equal(answer.End, location.End);
                Assert.Equal(xml.GetRange(node), location.Range);
                Assert.Equal(node.AsRawString(), xml.AsRawString(location.Range));
            }
            {
                var (node, answer) = cases[1];
                var location = xml.GetLocation(node);
                Assert.Equal(answer.Start, location.Start);
                Assert.Equal(answer.End, location.End);
                Assert.Equal(xml.GetRange(node), location.Range);
                Assert.Equal(node.AsRawString(), xml.AsRawString(location.Range));
            }
            {
                var (node, answer) = cases[2];
                var location = xml.GetLocation(node);
                Assert.Equal(answer.Start, location.Start);
                Assert.Equal(answer.End, location.End);
                Assert.Equal(xml.GetRange(node), location.Range);
                Assert.Equal(node.AsRawString(), xml.AsRawString(location.Range));
            }
            {
                var (node, answer) = cases[3];
                var location = xml.GetLocation(node);
                Assert.Equal(answer.Start, location.Start);
                Assert.Equal(answer.End, location.End);
                Assert.Equal(xml.GetRange(node), location.Range);
                Assert.Equal(node.AsRawString(), xml.AsRawString(location.Range));
            }
            {
                var (node, answer) = cases[4];
                var location = xml.GetLocation(node);
                Assert.Equal(answer.Start, location.Start);
                Assert.Equal(answer.End, location.End);
                Assert.Equal(xml.GetRange(node), location.Range);
                Assert.Equal(node.AsRawString(), xml.AsRawString(location.Range));
            }
        }

        [Fact]
        public unsafe void LocationOfEnd()
        {
            // 01234567
            // <a></a>_
            //        |
            //        `--> next character of the end
            //
            // No exceptions if call GetRange and GetLocation here.

            using var xml = XmlParser.Parse("<a></a>");

            var xmlRawString = xml.AsRawString();
            var str = xmlRawString.Slice(7);

            // str is empty, str.Ptr is next character of the end
            Assert.Equal(0, str.Length);
            Assert.Equal(new IntPtr(xmlRawString.GetPtr() + 7), str.Ptr);

            var range = xml.GetRange(str);
            Assert.Equal(new DataRange(7, 0), range);

            var location = xml.GetLocation(str);
            Assert.Equal(new DataLinePosition(0, 7), location.Start);
            Assert.Equal(new DataLinePosition(0, 7), location.End);
            Assert.Equal(new DataRange(7, 0), location.Range);
        }
    }
}
