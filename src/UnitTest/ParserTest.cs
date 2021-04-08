#nullable enable
using System;
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
<‚ ‚¢‚¤‚¦‚¨>
    <‚©‚«‚­‚¯‚±/>
</‚ ‚¢‚¤‚¦‚¨>
")]
        private static partial ReadOnlySpan<byte> Xml1();

        [Fact]
        public void Parse()
        {
            using(var obj = XmlParser.Parse(Xml1())) {
                Assert.NotNull(obj);
            }
            AllocationSafety.Ensure();
        }
    }
}
