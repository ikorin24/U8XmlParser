#nullable enable
using System.Linq;
using Xunit;
using U8Xml;
using U8Xml.Internal;

namespace UnitTest
{
    public partial class ParserTest
    {
        [Fact]
        public void Parse()
        {
            using(var obj = XmlParser.Parse(Data.Sample1)) {
                Assert.NotNull(obj);
                var root = obj.Root;
                Assert.True(root.Name == "あいうえお");
                Assert.True(root.Attributes.First().Name == "ほげ");
                Assert.True(root.HasChildren);
                Assert.True(root.Children.First().Name == "かきくけこ");
            }
            AllocationSafety.Ensure();
        }
    }
}
