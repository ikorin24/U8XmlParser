#nullable enable

#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if NETSTANDARD2_0 || NET48 || IS_UNITY
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte At(this ReadOnlySpan<byte> span, int index)
        {
#if FAST_SPAN
#if DEBUG
            if((uint)index >= (uint)span.Length) { throw new IndexOutOfRangeException(); }
#endif
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);
#else
            return ref span[index];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte At(this Span<byte> span, int index)
        {
#if FAST_SPAN
#if DEBUG
            if((uint)index >= (uint)span.Length) { throw new IndexOutOfRangeException(); }
#endif
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);
#else
            return ref span[index];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> SliceUnsafe(this ReadOnlySpan<byte> span, int start, int length)
        {
#if DEBUG
            if((uint)start > (uint)span.Length) { ThrowHelper.ThrowArgOutOfRange(nameof(start)); }
            if((uint)length > (uint)(span.Length - start)) { ThrowHelper.ThrowArgOutOfRange(nameof(length)); }
#endif

#if FAST_SPAN
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), start), length);
#else
            return span.Slice(start, length);
#endif
        }

        /// <summary>Trim invisible charactors. (whitespace, '\t', '\r', and '\n')</summary>
        /// <returns>trimmed string</returns>
        internal static ReadOnlySpan<byte> Trim(this ReadOnlySpan<byte> span)
        {
            return span.TrimStart().TrimEnd();
        }

        /// <summary>Trim invisible charactors of start. (whitespace, '\t', '\r', and '\n')</summary>
        /// <returns>trimmed string</returns>
        internal static ReadOnlySpan<byte> TrimStart(this ReadOnlySpan<byte> span)
        {
            for(int i = 0; i < span.Length; i++) {
                ref readonly var p = ref span.At(i);
                if(p != ' ' && p != '\t' && p != '\r' && p != '\n') {
                    return span.SliceUnsafe(i, span.Length - i);
                }
            }
            return ReadOnlySpan<byte>.Empty;
        }

        /// <summary>Trim invisible charactors of end. (whitespace, '\t', '\r' and '\n')</summary>
        /// <returns>trimmed string</returns>
        internal static ReadOnlySpan<byte> TrimEnd(this ReadOnlySpan<byte> span)
        {
            for(int i = span.Length - 1; i >= 0; i--) {
                ref readonly var p = ref span.At(i);
                if(p != ' ' && p != '\t' && p != '\r' && p != '\n') {
                    return span.SliceUnsafe(0, i + 1);
                }
            }
            return span;
        }
    }
}
