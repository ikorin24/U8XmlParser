#nullable enable
using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using U8Xml.Internal;
using System.Buffers;
using System.Collections.Generic;

namespace U8Xml
{
    /// <summary>Provides raw byte array of utf8, which is compatible <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</summary>
    [DebuggerTypeProxy(typeof(RawStringDebuggerTypeProxy))]
    [DebuggerDisplay("{ToString()}")]
    public readonly unsafe partial struct RawString : IEquatable<RawString>
    {
        private readonly IntPtr _ptr;
        private readonly int _length;

        /// <summary>Get an empty instance of <see cref="RawString"/>.</summary>
        public static RawString Empty => default;

        /// <summary>Get whether the byte array is empty or not.</summary>
        public bool IsEmpty => _length == 0;

        /// <summary>Get length of the byte array. (NOT number of characters)</summary>
        public int Length => _length;

        /// <summary>Get pointer to the head of the utf-8 characters.</summary>
        public IntPtr Ptr => _ptr;

        /// <summary>Get or set an item with specified index</summary>
        /// <param name="index">index of an item</param>
        /// <returns>the item</returns>
        public ref readonly byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if((uint)index >= (uint)_length) { ThrowHelper.ThrowArgOutOfRange(nameof(index)); }
                return ref ((byte*)_ptr)[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RawString(byte* ptr, int length)
        {
            Debug.Assert(length >= 0);
            _ptr = (IntPtr)ptr;
            _length = length;
        }

        /// <summary>Get number of characters</summary>
        /// <returns>characters count</returns>
        public int GetCharCount() => _length == 0 ? 0 : UTF8ExceptionFallbackEncoding.Instance.GetCharCount((byte*)_ptr, _length);

        /// <summary>Get read-only bytes data</summary>
        /// <returns><see cref="ReadOnlySpan{T}"/> of type <see langword="byte"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => SpanHelper.CreateReadOnlySpan<byte>(_ptr.ToPointer(), _length);

        /// <summary>Copy the bytes to a new byte array.</summary>
        /// <returns>new array</returns>
        public byte[] ToArray() => AsSpan().ToArray();

        /// <summary>Get slice of the <see cref="RawString"/></summary>
        /// <param name="start">start index to slice</param>
        /// <returns>sliced <see cref="RawString"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString Slice(int start)
        {
            if((uint)start > (uint)_length) { ThrowHelper.ThrowArgOutOfRange(nameof(start)); }
            return new RawString((byte*)_ptr + start, _length - start);
        }

        /// <summary>Get slice of the <see cref="RawString"/></summary>
        /// <param name="start">start index to slice</param>
        /// <param name="length">length to slice from <paramref name="start"/></param>
        /// <returns>sliced <see cref="RawString"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString Slice(int start, int length)
        {
            if((uint)start > (uint)_length) { ThrowHelper.ThrowArgOutOfRange(nameof(start)); }
            if((uint)length > (uint)(_length - start)) { ThrowHelper.ThrowArgOutOfRange(nameof(length)); }
            return new RawString((byte*)_ptr + start, length);
        }

        /// <summary>Get slice of the <see cref="RawString"/></summary>
        /// <param name="range">data range</param>
        /// <returns>sliced <see cref="RawString"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString Slice(DataRange range) => Slice(range.Start, range.Length);

        /// <summary>Trim invisible charactors. (whitespace, '\t', '\r', and '\n')</summary>
        /// <returns>trimmed string</returns>
        public RawString Trim()
        {
            return TrimStart().TrimEnd();
        }

        /// <summary>Trim invisible charactors of start. (whitespace, '\t', '\r', and '\n')</summary>
        /// <returns>trimmed string</returns>
        public RawString TrimStart()
        {
            for(int i = 0; i < _length; i++) {
                ref var p = ref ((byte*)_ptr)[i];
                if(p != ' ' && p != '\t' && p != '\r' && p != '\n') {
                    return SliceUnsafe(i, _length - i);
                }
            }
            return Empty;
        }

        /// <summary>Trim invisible charactors of end. (whitespace, '\t', '\r' and '\n')</summary>
        /// <returns>trimmed string</returns>
        public RawString TrimEnd()
        {
            for(int i = Length - 1; i >= 0; i--) {
                ref var p = ref ((byte*)_ptr)[i];
                if(p != ' ' && p != '\t' && p != '\r' && p != '\n') {
                    return SliceUnsafe(0, i + 1);
                }
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte* GetPtr() => (byte*)_ptr;

        /// <summary>Get or set an item with specified index.</summary>
        /// <remarks>[CAUTION] This method does not check index boundary!</remarks>
        /// <param name="index">index of an item</param>
        /// <returns>reference to the item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref byte At(int index)
        {
            // This method is same as this[index], but no boundary check.
#if DEBUG
            if((uint)index >= (uint)_length) { ThrowHelper.ThrowArgOutOfRange(nameof(index)); }
#endif
            return ref ((byte*)_ptr)[index];
        }

        /// <summary>Get slice of the array</summary>
        /// <remarks>[CAUTION] Boundary is not checked. Be careful !</remarks>
        /// <param name="start">start index to slice</param>
        /// <param name="length">length to slice from <paramref name="start"/></param>
        /// <returns>sliced array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RawString SliceUnsafe(int start, int length)
        {
#if DEBUG
            if((uint)start > (uint)_length) { ThrowHelper.ThrowArgOutOfRange(nameof(start)); }
            if((uint)length > (uint)(_length - start)) { ThrowHelper.ThrowArgOutOfRange(nameof(length)); }
#endif
            return new RawString((byte*)_ptr + start, length);
        }

        /// <summary>Get pinnnable reference.</summary>
        /// <returns>reference to the head of the data</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]               // Only for 'fixed' statement
        public ref readonly byte GetPinnableReference()
        {
            return ref Unsafe.AsRef<byte>((void*)_ptr);
        }

        /// <summary>Decode byte array as utf-8 and get <see langword="string"/></summary>
        /// <returns>decoded string</returns>
        public override string ToString() => IsEmpty ? "" : UTF8ExceptionFallbackEncoding.Instance.GetString((byte*)_ptr, _length);

        public override bool Equals(object? obj) => obj is RawString array && Equals(array);

        public bool Equals(RawString other) => (_ptr == other._ptr && _length == other._length) || SequenceEqual(other);

        public bool SequenceEqual(RawString other) => AsSpan().SequenceEqual(other.AsSpan());

        public bool SequenceEqual(ReadOnlySpan<byte> other) => AsSpan().SequenceEqual(other);

        public bool ReferenceEquals(RawString other) => _ptr.Equals(other.Ptr) && _length == other.Length;

        public bool StartsWith(RawString other)
        {
            return AsSpan().StartsWith(other.AsSpan());
        }

        public bool StartsWith(ReadOnlySpan<byte> other)
        {
            return AsSpan().StartsWith(other);
        }

        public bool StartsWith(string other) => StartsWith(other.AsSpan());

        [SkipLocalsInit]
        public bool StartsWith(ReadOnlySpan<char> other)
        {
            if(other.Length == 0) {
                return true;
            }
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var byteLen = utf8.GetByteCount(other);
            if(byteLen > Length) {
                return false;
            }

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = other) {
                    utf8.GetBytes(ptr, other.Length, buf, byteLen);
                }
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return AsSpan().StartsWith(span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = other) {
                        utf8.GetBytes(ptr, other.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return AsSpan().StartsWith(span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        public bool EndsWith(RawString other)
        {
            return AsSpan().EndsWith(other.AsSpan());
        }

        public bool EndsWith(ReadOnlySpan<byte> other)
        {
            return AsSpan().EndsWith(other);
        }

        public bool EndsWith(string other)
        {
            return EndsWith(other.AsSpan());
        }

        [SkipLocalsInit]
        public bool EndsWith(ReadOnlySpan<char> other)
        {
            if(other.Length == 0) {
                return true;
            }
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var byteLen = utf8.GetByteCount(other);
            if(byteLen > Length) {
                return false;
            }

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = other) {
                    utf8.GetBytes(ptr, other.Length, buf, byteLen);
                }
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return AsSpan().EndsWith(span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = other) {
                        utf8.GetBytes(ptr, other.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return AsSpan().EndsWith(span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }


        public int IndexOf(byte value)
        {
            var span = AsSpan();
            for(int i = 0; i < span.Length; i++) {
                if(span[i] == value) {
                    return i;
                }
            }
            return -1;
        }

        public DataRange RangeOf(char value)
        {
            if(value < 128) {
                // For ASCII
                var index = IndexOf((byte)value);
                return new DataRange(index, (index >= 0) ? 1 : 0);
            }
            else {
                var utf8 = UTF8ExceptionFallbackEncoding.Instance;
                byte* buf = stackalloc byte[8];
                var len = utf8.GetBytes(&value, 1, buf, 8);
                return RangeOf(SpanHelper.CreateReadOnlySpan<byte>(buf, len));
            }
        }

        public DataRange RangeOf(RawString value) => RangeOf(value.AsSpan());

        public DataRange RangeOf(ReadOnlySpan<byte> value)
        {
            if(value.Length == 0) { return new DataRange(0, 0); }

            var l = Length + 1 - value.Length;
            var span = AsSpan();
            for(int i = 0; i < l; i++) {
                if(span.SliceUnsafe(i, span.Length - i).StartsWith(value)) {
                    return new DataRange(i, value.Length);
                }
            }
            return new DataRange(-1, 0);
        }

        public DataRange RangeOf(string value) => RangeOf(value.AsSpan());

        public DataRange RangeOf(ReadOnlySpan<char> value)
        {
            if(value.Length == 0) {
                return new DataRange(0, 0);
            }
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var byteLen = utf8.GetByteCount(value);
            if(byteLen > Length) {
                return new DataRange(-1, 0);
            }

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = value) {
                    utf8.GetBytes(ptr, value.Length, buf, byteLen);
                }
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return RangeOf(span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = value) {
                        utf8.GetBytes(ptr, value.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return RangeOf(span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }


        public int LastIndexOf(byte value)
        {
            var span = AsSpan();
            for(int i = span.Length - 1; i >= 0; i--) {
                if(span.At(i) == value) {
                    return i;
                }
            }
            return -1;
        }

        public DataRange LastRangeOf(char value)
        {
            if(value < 128) {
                // For ASCII
                var index = LastIndexOf((byte)value);
                return new DataRange(index, (index >= 0) ? 1 : 0);
            }
            else {
                var utf8 = UTF8ExceptionFallbackEncoding.Instance;
                byte* buf = stackalloc byte[8];
                var len = utf8.GetBytes(&value, 1, buf, 8);
                return LastRangeOf(SpanHelper.CreateReadOnlySpan<byte>(buf, len));
            }
        }

        public DataRange LastRangeOf(RawString value) => LastRangeOf(value.AsSpan());

        public DataRange LastRangeOf(ReadOnlySpan<byte> value)
        {
            if(value.Length == 0) { return new DataRange(0, 0); }

            var l = Length + 1 - value.Length;
            var span = AsSpan();
            for(int i = l - 1; i >= 0; i--) {
                if(span.SliceUnsafe(i, span.Length - i).StartsWith(value)) {
                    return new DataRange(i, value.Length);
                }
            }
            return new DataRange(-1, 0);
        }

        public DataRange LastRangeOf(string value) => LastRangeOf(value.AsSpan());

        public DataRange LastRangeOf(ReadOnlySpan<char> value)
        {
            if(value.Length == 0) {
                return new DataRange(0, 0);
            }
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var byteLen = utf8.GetByteCount(value);
            if(byteLen > Length) {
                return new DataRange(-1, 0);
            }

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = value) {
                    utf8.GetBytes(ptr, value.Length, buf, byteLen);
                }
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return LastRangeOf(span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = value) {
                        utf8.GetBytes(ptr, value.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return LastRangeOf(span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }


        public bool Contains(byte value) => IndexOf(value) >= 0;
        public bool Contains(char value) => RangeOf(value).Start >= 0;
        public bool Contains(RawString value) => RangeOf(value).Start >= 0;
        public bool Contains(ReadOnlySpan<byte> value) => RangeOf(value).Start >= 0;
        public bool Contains(string value) => RangeOf(value).Start >= 0;
        public bool Contains(ReadOnlySpan<char> value) => RangeOf(value).Start >= 0;


        /// <summary>Compute hash code for the specified span using the same algorithm as <see cref="GetHashCode()"/>.</summary>
        /// <param name="utf8String">span to compute hash code</param>
        /// <returns>hash code</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> utf8String)
        {
            fixed(byte* ptr = utf8String) {
                return GetHashCode(ptr, utf8String.Length);
            }
        }

        /// <summary>Compute hash code for the specified span using the same algorithm as <see cref="GetHashCode()"/>.</summary>
        /// <param name="ptr">pointer to byte span head</param>
        /// <param name="length">length of byte span</param>
        /// <returns>hash code</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(IntPtr ptr, int length)
        {
            return GetHashCode((byte*)ptr, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCode(byte* ptr, int length)
        {
            // Returns hash computed by same algorithm as RawString.
            // This method is used in RawStringTable

            return XXHash32.ComputeHash(ptr, length);
        }

        public override int GetHashCode()
        {
            return GetHashCode((byte*)_ptr, _length);
        }

        public static bool operator ==(RawString left, RawString right) => left.Equals(right);

        public static bool operator !=(RawString left, RawString right) => !(left == right);

        public static bool operator ==(RawString left, ReadOnlySpan<byte> right) => left.SequenceEqual(right);

        public static bool operator !=(RawString left, ReadOnlySpan<byte> right) => !(left == right);

        public static bool operator ==(ReadOnlySpan<byte> left, RawString right) => right == left;

        public static bool operator !=(ReadOnlySpan<byte> left, RawString right) => !(left == right);

        [SkipLocalsInit]
        public static bool operator ==(RawString left, ReadOnlySpan<char> right)
        {
            if(right.IsEmpty) { return left.IsEmpty; }
            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
            var byteLen = utf8.GetByteCount(right);
            if(byteLen != left.Length) { return false; }

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = right) {
                    utf8.GetBytes(ptr, right.Length, buf, byteLen);
                }
                return SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen).SequenceEqual(left.AsSpan());
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = right) {
                        utf8.GetBytes(ptr, right.Length, buf, byteLen);
                        return SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen).SequenceEqual(left.AsSpan());
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        public static bool operator !=(RawString left, ReadOnlySpan<char> right) => !(left == right);

        public static bool operator ==(ReadOnlySpan<char> left, RawString right) => right == left;

        public static bool operator !=(ReadOnlySpan<char> left, RawString right) => !(left == right);

        public static bool operator ==(RawString left, string right) => left == right.AsSpan();

        public static bool operator !=(RawString left, string right) => !(left == right);

        public static bool operator ==(string left, RawString right) => right == left;

        public static bool operator !=(string left, RawString right) => !(right == left);

        //public static implicit operator ReadOnlySpan<byte>(RawString rawString) => rawString.AsSpan();
    }

    internal sealed class RawStringDebuggerTypeProxy
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RawString _entity;

        public byte[] ByteArray => _entity.ToArray();
        public int ByteLength => _entity.Length;
        public unsafe int CharCount => _entity.GetCharCount();

        public string[] Lines
        {
            get
            {
                var lines = new List<string>();
                foreach(var line in _entity.Split((byte)'\n')) {
                    if(line.Length > 0 && line[line.Length - 1] == '\r') {
                        lines.Add(line.Slice(0, line.Length - 1).ToString());
                    }
                    else {
                        lines.Add(line.ToString());
                    }
                }
                return lines.ToArray();
            }
        }

        public string String => _entity.ToString();

        public RawStringDebuggerTypeProxy(RawString entity)
        {
            _entity = entity;
        }
    }
}
