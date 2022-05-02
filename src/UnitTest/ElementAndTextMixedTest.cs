#nullable enable
using U8Xml;
using Xunit;
using System.Linq;

namespace UnitTest
{
    public class ElementAndTextMixedTest
    {
        [Fact]
        public void ParseMixedNodes()
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

            Assert.True(root.Attributes.Count == 1);
            Assert.True(root.Attributes.First().Name == "xyz");
            Assert.True(root.Attributes.First().Value == "321");

            Assert.Equal(5, root.GetChildren(null).Count());
            Assert.False(root.FindChildOrDefault(RawString.Empty).HasValue);
            Assert.True(root.FindChildOrDefault("aaa").HasValue);
            Assert.True(root.FindChildOrDefault("bbb").HasValue);
            Assert.False(root.FindChildOrDefault("foo").HasValue);
            Assert.False(root.FindChildOrDefault("bar").HasValue);
            Assert.False(root.FindChildOrDefault("baz").HasValue);

            AssertTextNode(root.GetChildren(null).ElementAt(0), "foo");
            AssertTextNode(root.GetChildren(null).ElementAt(2), "bar");
            AssertTextNode(root.GetChildren(null).ElementAt(4), "baz");

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

        [Fact]
        public void MixedInnerText()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>123</aaa>
    bar
    <bbb/>
    baz
    <ccc>
        <ddd>boo</ddd>
        dee
    </ccc>
</root>";
            using var xml = XmlParser.Parse(xmlString);
            var root = xml.Root;

            Assert.True(root.InnerText.IsEmpty);
            Assert.Equal("123", root.FindChild("aaa").InnerText.ToString());
            Assert.True(root.FindChild("ccc").InnerText.IsEmpty);
            Assert.Equal("boo", root.FindChild("ccc").FindChild("ddd").InnerText.ToString());
        }

        [Fact]
        public void MixedDescendants()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>123</aaa>
    bar
    <bbb/>
    baz
    <ccc>
        <ddd>boo</ddd>
        dee
    </ccc>
</root>";
            using var xml = XmlParser.Parse(xmlString);
            var root = xml.Root;

            Assert.True(root.Descendants
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(root.GetDescendants()
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(root.GetDescendants(XmlNodeType.ElementNode)
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                root.GetDescendants(XmlNodeType.TextNode)
                .Select(n => new { Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Text = "123", NodeType = XmlNodeType.TextNode, },
                    new { Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Text = "baz", NodeType = XmlNodeType.TextNode, },
                    new { Text = "boo", NodeType = XmlNodeType.TextNode, },
                    new { Text = "dee", NodeType = XmlNodeType.TextNode, },
                }));

            Assert.True(
                root.GetDescendants(null)
                .Select(n => new { Name = n.Name.ToString(), Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "", Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Name = "aaa", Text = "123", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "123", NodeType = XmlNodeType.TextNode, },
                    new { Name = "", Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Name = "bbb", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "baz", NodeType = XmlNodeType.TextNode, },
                    new { Name = "ccc", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", Text = "boo", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "boo", NodeType = XmlNodeType.TextNode, },
                    new { Name = "", Text = "dee", NodeType = XmlNodeType.TextNode, },
                }));
        }

        [Fact]
        public void MixedChildren()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>123</aaa>
    bar
    <bbb/>
    baz
    <ccc>
        <ddd>boo</ddd>
        dee
    </ccc>
</root>";
            using var xml = XmlParser.Parse(xmlString);
            var root = xml.Root;

            Assert.True(
                root.Children
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                root.GetChildren()
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                root.GetChildren(XmlNodeType.ElementNode)
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                root.GetChildren(XmlNodeType.TextNode)
                .Select(n => new { Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Text = "baz", NodeType = XmlNodeType.TextNode, },
                }));

            Assert.True(
                root.GetChildren(null)
                .Select(n => new { Name = n.Name.ToString(), Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "", Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Name = "aaa", Text = "123", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Name = "bbb", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "baz", NodeType = XmlNodeType.TextNode, },
                    new { Name = "ccc", Text = "", NodeType = XmlNodeType.ElementNode, },
                }));
        }

        [Fact]
        public void MixedAllNodes()
        {
            const string xmlString =
@"<root xyz=""321"">
    foo
    <aaa>123</aaa>
    bar
    <bbb/>
    baz
    <ccc>
        <ddd>boo</ddd>
        dee
    </ccc>
</root>";
            using var xml = XmlParser.Parse(xmlString);

            Assert.True(
                xml.GetAllNodes()
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "root", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                xml.GetAllNodes(XmlNodeType.ElementNode)
                .Select(n => new { Name = n.Name.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "root", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "aaa", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "bbb", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ccc", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", NodeType = XmlNodeType.ElementNode, },
                }));

            Assert.True(
                xml.GetAllNodes(XmlNodeType.TextNode)
                .Select(n => new { Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Text = "123", NodeType = XmlNodeType.TextNode, },
                    new { Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Text = "baz", NodeType = XmlNodeType.TextNode, },
                    new { Text = "boo", NodeType = XmlNodeType.TextNode, },
                    new { Text = "dee", NodeType = XmlNodeType.TextNode, },
                }));

            Assert.True(
                xml.GetAllNodes(null)
                .Select(n => new { Name = n.Name.ToString(), Text = n.InnerText.ToString(), NodeType = n.NodeType, })
                .SequenceEqual(new[]
                {
                    new { Name = "root", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "foo", NodeType = XmlNodeType.TextNode, },
                    new { Name = "aaa", Text = "123", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "123", NodeType = XmlNodeType.TextNode, },
                    new { Name = "", Text = "bar", NodeType = XmlNodeType.TextNode, },
                    new { Name = "bbb", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "baz", NodeType = XmlNodeType.TextNode, },
                    new { Name = "ccc", Text = "", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "ddd", Text = "boo", NodeType = XmlNodeType.ElementNode, },
                    new { Name = "", Text = "boo", NodeType = XmlNodeType.TextNode, },
                    new { Name = "", Text = "dee", NodeType = XmlNodeType.TextNode, },
                }));
        }
    }
}
