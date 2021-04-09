#nullable enable
using System;
using System.Linq;
using Xunit;
using StringLiteral;
using U8Xml;
using U8Xml.Internal;

namespace UnitTest
{
    public partial class ParserTest
    {
        [Utf8(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<あいうえお ほげ=""3"">
    <かきくけこ/>
</あいうえお>")]
        private static partial ReadOnlySpan<byte> Xml1();

        [Fact]
        public void Parse()
        {
            using(var obj = XmlParser.Parse(Xml1())) {
                Assert.NotNull(obj);
                ref readonly var root = ref obj.Root;
                Assert.True(root.Name == "あいうえお");
                Assert.True(root.Attributes.First().Name == "ほげ");
                Assert.True(root.HasChildren);
                Assert.True(root.Children.First().Name == "かきくけこ");
            }
            AllocationSafety.Ensure();
        }
    }
}
