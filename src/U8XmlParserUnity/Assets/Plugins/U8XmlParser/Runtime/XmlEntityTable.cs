#nullable enable

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using U8Xml.Internal;

namespace U8Xml
{
    public readonly struct XmlEntityTable
    {
        private readonly Option<RawStringTable> _rawStringTable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlEntityTable(RawStringTable rawStringTable)
        {
            _rawStringTable = rawStringTable;
        }

        /// <summary>Check to see if the input string needs to be resolved.</summary>
        /// <param name="str">string to check</param>
        /// <param name="requiredBufferLength">byte length to required to resolve</param>
        /// <returns>state of resolver</returns>
        public unsafe XmlEntityResolverState CheckNeedToResolve(ReadOnlySpan<byte> str, out int requiredBufferLength)
        {
            fixed(byte* p = str) {
                // It is safe to create RawString from ReadOnlySpan<byte>
                // because the method does not store the RawString instance anywhere.
                return CheckNeedToResolve(new RawString(p, str.Length), out requiredBufferLength);
            }
        }

        /// <summary>Check to see if the input string needs to be resolved.</summary>
        /// <param name="str">string to check</param>
        /// <param name="requiredBufferLength">byte length to required to resolve</param>
        /// <returns>state of resolver</returns>
        [SkipLocalsInit]
        public unsafe XmlEntityResolverState CheckNeedToResolve(RawString str, out int requiredBufferLength)
        {
            const int ExBufLen = 5;         // A unicode character can be up to 5 bytes.
            byte* exBuf = stackalloc byte[ExBufLen];

            var len = str.Length;
            var pos = -1;
            var needToResolve = false;

            for(int i = 0; i < str.Length; i++) {
                var c = str.At(i);
                if(c == '&') {
                    if(pos >= 0) {
                        goto CanNotResolve;
                    }
                    needToResolve = true;
                    pos = i + 1;
                    continue;
                }
                else if(c == ';') {
                    if(pos < 0) {
                        goto CanNotResolve;
                    }
                    var alias = str.SliceUnsafe(pos, i - pos);
                    if(TryGetValue(alias, out var value) == false) {
                        var tmp = SpanHelper.CreateSpan<byte>(exBuf, ExBufLen);
                        if(TryUnicodePointToUtf8(alias, tmp, out int byteLen) == false) {
                            goto CanNotResolve;
                        }
                        value = tmp.Slice(0, byteLen);
                    }
                    else {
                        // The value of entity may contains other enity alias.
                        var recursiveResolveNeeded = CheckNeedToResolve(value, out int l);
                        if(recursiveResolveNeeded == XmlEntityResolverState.CannotResolve) {
                            goto CanNotResolve;
                        }
                        else if(recursiveResolveNeeded == XmlEntityResolverState.NeedToResolve) {
                            len = len - value.Length + l;
                        }
                    }
                    len = len - 2 - alias.Length + value.Length;
                    pos = -1;
                    continue;
                }
            }
            if(pos >= 0) {
                goto CanNotResolve;
            }

            requiredBufferLength = len;
            return needToResolve ? XmlEntityResolverState.NeedToResolve : XmlEntityResolverState.NoNeeded;

        CanNotResolve:
            {
                requiredBufferLength = 0;
                return XmlEntityResolverState.CannotResolve;
            }
        }

        /// <summary>Get byte length of a buffer which the resolver need to resolve the string.</summary>
        /// <param name="str">string to check</param>
        /// <returns>byte length of a buffer</returns>
        public unsafe int GetResolvedByteLength(ReadOnlySpan<byte> str)
        {
            fixed(byte* p = str) {
                // It is safe to create RawString from ReadOnlySpan<byte>
                // because the method does not store the RawString instance anywhere.
                return GetResolvedByteLength(new RawString(p, str.Length));
            }
        }

        /// <summary>Get byte length of a buffer which the resolver need to resolve the string.</summary>
        /// <param name="str">string to check</param>
        /// <returns>byte length of a buffer</returns>
        public int GetResolvedByteLength(RawString str)
        {
            var state = CheckNeedToResolve(str, out var requiredBufLen);
            if(state == XmlEntityResolverState.CannotResolve) {
                throw new ArgumentException("Could not resolve the input string.");
            }
            return requiredBufLen;
        }

        /// <summary>Resolve the input utf-8 string to <see langword="string"/></summary>
        /// <param name="str">utf-8 string to resolve</param>
        /// <returns>resolved <see langword="string"/></returns>
        public unsafe string ResolveToString(ReadOnlySpan<byte> str)
        {
            fixed(byte* p = str) {
                // It is safe to create RawString from ReadOnlySpan<byte>
                // because the method does not store the RawString instance anywhere.
                return ResolveToString(new RawString(p, str.Length));
            }
        }

        /// <summary>Resolve the input utf-8 string to <see langword="string"/></summary>
        /// <param name="str">utf-8 string to resolve</param>
        /// <returns>resolved <see langword="string"/></returns>
        [SkipLocalsInit]
        public unsafe string ResolveToString(RawString str)
        {
            var byteLen = GetResolvedByteLength(str);
            const int Threshold = 128;
            if(byteLen <= Threshold) {
                Span<byte> buf = stackalloc byte[Threshold];
                Resolve(str, buf);
                fixed(byte* ptr = buf) {
                    return UTF8ExceptionFallbackEncoding.Instance.GetString(ptr, byteLen);
                }
            }
            else {
                var buf = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    Resolve(str, buf.AsSpan());
                    fixed(byte* ptr = buf) {
                        return UTF8ExceptionFallbackEncoding.Instance.GetString(ptr, byteLen);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(buf);
                }
            }
        }

        /// <summary>Resolve the string.</summary>
        /// <param name="str">the string to resolve</param>
        /// <returns>resolved utf-8 string as byte array</returns>
        public unsafe byte[] Resolve(ReadOnlySpan<byte> str)
        {
            fixed(byte* p = str) {
                // It is safe to create RawString from ReadOnlySpan<byte>
                // because the method does not store the RawString instance anywhere.
                return Resolve(new RawString(p, str.Length));
            }
        }

        /// <summary>Resolve the string.</summary>
        /// <param name="str">the string to resolve</param>
        /// <returns>resolved utf-8 string as byte array</returns>
        [SkipLocalsInit]
        public byte[] Resolve(RawString str)
        {
            var byteLen = GetResolvedByteLength(str);
            if(byteLen <= 128) {
                Span<byte> buf = stackalloc byte[byteLen];
                Resolve(str, buf);
                return buf.ToArray();
            }
            else {
                var buf = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    Resolve(str, buf.AsSpan());

                    // Don't return buf. Must be copied
                    return buf.AsSpan(0, byteLen).ToArray();
                }
                finally {
                    ArrayPool<byte>.Shared.Return(buf);
                }
            }
        }

        /// <summary>Resolve the string to the specified buffer.</summary>
        /// <remarks>The buffer must be large enough to resolve.</remarks>
        /// <param name="str">string to resolve</param>
        /// <param name="bufferToResolve">the buffer used in resolving the string</param>
        /// <returns>byte length of the resolved string</returns>
        public unsafe int Resolve(ReadOnlySpan<byte> str, Span<byte> bufferToResolve)
        {
            fixed(byte* p = str) {
                // It is safe to create RawString from ReadOnlySpan<byte>
                // because the method does not store the RawString instance anywhere.
                return Resolve(new RawString(p, str.Length), bufferToResolve);
            }
        }

        /// <summary>Resolve the string to the specified buffer.</summary>
        /// <remarks>The buffer must be large enough to resolve.</remarks>
        /// <param name="str">string to resolve</param>
        /// <param name="bufferToResolve">the buffer used in resolving the string</param>
        /// <returns>byte length of the resolved string</returns>
        [SkipLocalsInit]
        public unsafe int Resolve(RawString str, Span<byte> bufferToResolve)
        {
            const int ExBufLen = 5;
            byte* exBuf = stackalloc byte[ExBufLen];

            // Use pointer to avoid the overhead in case of SlowSpan runtime.
            fixed(byte* buf = bufferToResolve) {
                int i = 0;
                int j = 0;
            None:
                {
                    if(i >= str.Length) {
                        goto End;
                    }
                    var c = str.At(i++);
                    if(c == '&') {
                        goto Alias;
                    }
                    if(j >= bufferToResolve.Length) {
                        throw new ArgumentOutOfRangeException("Buffer is too short.");
                    }
                    buf[j++] = c;
                    goto None;
                }

            Alias:
                {
                    var start = i;
                    while(true) {
                        if(i >= str.Length) { throw new FormatException($"Cannot end with '&'. Invalid input string: '{str}'"); }
                        if(str.At(i++) == ';') {
                            var alias = str.SliceUnsafe(start, i - 1 - start);
                            if(TryGetValue(alias, out var value) == false) {
                                var tmp = SpanHelper.CreateSpan<byte>(exBuf, ExBufLen);
                                if(TryUnicodePointToUtf8(alias, tmp, out int byteLen) == false) {
                                    throw new FormatException($"Could not resolve the entity: '&{alias};'");
                                }
                                value = tmp.Slice(0, byteLen);
                            }
                            else {
                                // The value of entity may contains other enity alias.
                                var recursiveResolveNeeded = CheckNeedToResolve(value, out int l);
                                if(recursiveResolveNeeded == XmlEntityResolverState.CannotResolve) {
                                    throw new FormatException($"Could not resolve an entity");
                                }
                                else if(recursiveResolveNeeded == XmlEntityResolverState.NeedToResolve) {
                                    value = Resolve(value);
                                }
                            }

                            // If my implementation is correct, value.Length will not be zero. However, I will check just to be sure.
                            if(value.Length > 0) {
                                if(j + value.Length - 1 >= bufferToResolve.Length) {
                                    throw new ArgumentOutOfRangeException("Buffer is too short.");
                                }

                                fixed(byte* v = value) {
                                    Buffer.MemoryCopy(v, buf + j, value.Length, value.Length);
                                }
                                j += value.Length;
                            }
                            break;
                        }
                    }
                    goto None;
                }

            End:
                {
                    return j;
                }
            }
        }

        private bool TryGetValue(RawString alias, out ReadOnlySpan<byte> value)
        {
            if(PredefinedEntityTable.TryGetPredefinedValue(alias, out value)) {
                return true;
            }

            if(_rawStringTable.TryGetValue(out var table) == false) {
                value = ReadOnlySpan<byte>.Empty;
                return false;
            }
            var success = table.TryGetValue(alias, out var v);
            value = v.AsSpan();
            return success;
        }

        private static bool TryUnicodePointToUtf8(RawString str, Span<byte> buffer, out int byteLength)
        {
            // str is like as "#1234" or "#x12AB"

            if(str.Length < 2 || str.At(0) != '#') {
                byteLength = 0;
                return false;
            }

            if(str.At(1) == 'x') {
                var hex = str.SliceUnsafe(2, str.Length - 2).AsSpan();
                if(Utf8Parser.TryParse(hex, out uint codePoint, out _, 'x') == false) {
                    byteLength = 0;
                    return false;
                }
                return UnicodeHelper.TryEncodeCodePointToUtf8(codePoint, buffer, out byteLength);
            }
            else {
                if(str.SliceUnsafe(1, str.Length - 1).TryToUInt32(out uint codePoint) == false) {
                    byteLength = 0;
                    return false;
                }
                return UnicodeHelper.TryEncodeCodePointToUtf8(codePoint, buffer, out byteLength);
            }
        }
    }

    /// <summary>State of <see cref="XmlEntityTable"/></summary>
    public enum XmlEntityResolverState
    {
        /// <summary>It does not need to resolve the string.</summary>
        NoNeeded,
        /// <summary>It needs to resolve the string.</summary>
        NeedToResolve,
        /// <summary>The string is invalid. The resolver cannot resolve it.</summary>
        CannotResolve,
    }
}
