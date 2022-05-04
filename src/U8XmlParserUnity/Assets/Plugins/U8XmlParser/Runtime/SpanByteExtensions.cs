#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    public static class SpanByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Slice(this Span<byte> span, DataRange range)
        {
            return span.Slice(range.Start, range.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Slice(this ReadOnlySpan<byte> span, DataRange range)
        {
            return span.Slice(range.Start, range.Length);
        }
    }
}
