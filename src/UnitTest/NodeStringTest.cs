#nullable enable
using Xunit;
using U8Xml;

namespace UnitTest
{
    public class NodeStringTest
    {
        [Fact]
        public void NodeAsString()
        {
            const string XmlString =
@"<?xml version='1.0' ?>
<root>
  <foo>
    <bar1 id='1'/>
    <bar2 id='2'>
        <baz>abc</baz>
    </bar2>
  </foo>
</root>";

            using var xml = XmlParser.Parse(XmlString);
            Assert.True(xml.AsRawString() == XmlString);

            var root = xml.Root;
            Assert.True(root.AsRawString() ==
@"<root>
  <foo>
    <bar1 id='1'/>
    <bar2 id='2'>
        <baz>abc</baz>
    </bar2>
  </foo>
</root>");

            var foo = xml.Root.FindChild("foo");
            Assert.True(foo.AsRawString() ==
@"<foo>
    <bar1 id='1'/>
    <bar2 id='2'>
        <baz>abc</baz>
    </bar2>
  </foo>");

            var bar1 = foo.FindChild("bar1");
            Assert.True(bar1.AsRawString() == @"<bar1 id='1'/>");

            var bar2 = foo.FindChild("bar2");
            Assert.True(bar2.AsRawString() ==
@"<bar2 id='2'>
        <baz>abc</baz>
    </bar2>");

            var baz = bar2.FindChild("baz");
            Assert.True(baz.AsRawString() == @"<baz>abc</baz>");
        }

        [Fact]
        public void TextNodeAsString()
        {
            const string XmlString = @"<root>abc</root>";

            using var xml = XmlParser.Parse(XmlString);
            var textNode = xml.Root.GetChildren(XmlNodeType.TextNode).First();

            Assert.Equal(XmlNodeType.TextNode, textNode.NodeType);
            Assert.Equal("abc", textNode.InnerText.ToString());
            Assert.Equal("abc", textNode.AsRawString().ToString());
        }
    }
}
