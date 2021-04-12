#nullable enable
using System;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
	unsafe partial struct RawString
	{
        private const string InvalidFormatMessage = "Invalid format";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToInt(out int value)
		{
            return Utf8SpanHelper.TryParseInt(AsSpan(), out value);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToInt()
		{
            if(Utf8SpanHelper.TryParseInt(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
			return result;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToUInt(out uint value)
        {
            return Utf8SpanHelper.TryParseUInt(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt()
        {
            if(Utf8SpanHelper.TryParseUInt(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return result;
        }

        public (RawString, RawString) Split2(byte separator)
        {
            for(int i = 0; i < _length; i++) {
                if(((byte*)_ptr)[i] == separator) {
                    var latterStart = Math.Min(i + 1, _length);
                    return (SliceUnsafe(0, i), SliceUnsafe(latterStart, _length - latterStart));
                }
            }
            return (this, Empty);
        }
    }
}
