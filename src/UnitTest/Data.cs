#nullable enable
using System;
using StringLiteral;

namespace UnitTest
{
    internal static partial class Data
    {
        [Utf8(
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<あいうえお ほげ=""3"">
    <かきくけこ>さしすせそ</かきくけこ>
</あいうえお>")]
        private static partial ReadOnlySpan<byte> Xml1();
        public static ReadOnlySpan<byte> Sample1 => Xml1();
    }
}
