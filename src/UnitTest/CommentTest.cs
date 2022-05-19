#nullable enable
using Xunit;
using U8Xml;

namespace UnitTest
{
    public class CommentTest
    {
        [Fact]
        public void CommentAtEnd()
        {
            using var xml = XmlParser.Parse(
@"<foo></foo>
<!-- comment -->");
            Assert.Equal("foo", xml.Root.Name.ToString());
        }

        [Fact]
        public void CommentAtHead()
        {
            using var xml = XmlParser.Parse(
@"<!-- comment -->
<foo></foo>");
            Assert.Equal("foo", xml.Root.Name.ToString());
        }

        [Fact]
        public void CommentInNode()
        {
            using var xml = XmlParser.Parse(
@"<foo>
<!-- comment -->
</foo>");
            Assert.Equal("foo", xml.Root.Name.ToString());
        }
    }
}
