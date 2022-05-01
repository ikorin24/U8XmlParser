#nullable enable
using U8Xml;
using Xunit;
using System.Linq;

namespace UnitTest
{
    public class ParseTextNodeTest
    {
        [Fact]
        public void ParseTextNode()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>123</aaa>
    bar
    <bbb/>
    baz
</root>";
            using var xml = XmlParser.Parse(xmlString);
            var root = xml.Root;

            {
                Assert.True(root.Attributes.Count == 1);
                var attr = root.Attributes.First();
                Assert.True(attr.Name == "xyz");
                Assert.True(attr.Value == "321");

                Assert.Equal(5, root.GetChildren().Count());
                Assert.False(root.FindChildOrDefault(RawString.Empty).HasValue);
                Assert.True(root.FindChildOrDefault("aaa").HasValue);
                Assert.True(root.FindChildOrDefault("bbb").HasValue);
                Assert.False(root.FindChildOrDefault("foo").HasValue);
                Assert.False(root.FindChildOrDefault("bar").HasValue);
                Assert.False(root.FindChildOrDefault("baz").HasValue);
            }

            AssertTextNode(root.GetChildren().ElementAt(0), "foo");
            AssertTextNode(root.GetChildren().ElementAt(2), "bar");
            AssertTextNode(root.GetChildren().ElementAt(4), "baz");

            static void AssertTextNode(XmlNode textNode, string text)
            {
                Assert.True(textNode.NodeType == XmlNodeType.TextNode);
                Assert.True(textNode.Name.IsEmpty);
                Assert.True(textNode.InnerText == text);
                Assert.False(textNode.HasAttribute);
                Assert.False(textNode.HasChildren);
                Assert.False(textNode.IsNull);

                var aaa = textNode.FindChildOrDefault(RawString.Empty);
                Assert.False(aaa.HasValue);
            }
        }
    }
}
