#nullable enable

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
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
        public XmlEntityResolverState CheckNeedToResolve(RawString str, out int requiredBufferLength)
        {
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
                        goto CanNotResolve;
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
        public int GetResolvedByteLength(RawString str)
        {
            var state = CheckNeedToResolve(str, out var requiredBufLen);
            if(state == XmlEntityResolverState.CannotResolve) {
                throw new ArgumentException("Could not resolve the input string.");
            }
            return requiredBufLen;
        }

        /// <summary>Resolve the string.</summary>
        /// <param name="str">the string to resolve</param>
        /// <returns>resolved utf-8 string as byte array</returns>
#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
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
        public unsafe int Resolve(RawString str, Span<byte> bufferToResolve)
        {
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
                                throw new FormatException($"Could not resolve entity: '&{alias};'");
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

        private bool TryGetValue(in RawString alias, out ReadOnlySpan<byte> value)
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
