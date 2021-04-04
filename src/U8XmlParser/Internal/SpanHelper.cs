#nullable enable

#if NETSTANDARD2_0 || NET48
#define NO_SPAN_API
#endif

#if NETCOREAPP3_1_OR_GREATER
#define FAST_SPAN
#endif

using System;
using System.Text;
using System.Runtime.CompilerServices;

#if FAST_SPAN
using System.Runtime.InteropServices;
#endif

namespace U8Xml.Internal
{
    internal static class SpanHelper
    {
        public static string Utf8ToString(this ReadOnlySpan<byte> source)
        {
#if NO_SPAN_API
            unsafe { fixed(byte* ptr = source) { return Encoding.UTF8.GetString(ptr, source.Length); } }
#else
            return Encoding.UTF8.GetString(source);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(void* ptr, int length) where T : unmanaged
        {
#if FAST_SPAN
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(ptr), length);
#else
            return new ReadOnlySpan<T>(ptr, length);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> CreateSpan<T>(void* ptr, int length) where T : unmanaged
        {
#if FAST_SPAN
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(ptr), length);
#else
            return new Span<T>(ptr, length);
#endif
        }
    }
}
