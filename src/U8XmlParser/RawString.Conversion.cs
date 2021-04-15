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
        public bool TryToInt32(out int value)
		{
            return Utf8SpanHelper.TryParseInt32(AsSpan(), out value);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToInt32()
		{
            if(Utf8SpanHelper.TryParseInt32(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
			return result;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToUInt32(out uint value)
        {
            return Utf8SpanHelper.TryParseUInt32(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ToUInt32()
        {
            if(Utf8SpanHelper.TryParseUInt32(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToInt64(out long value)
        {
            return Utf8SpanHelper.TryParseInt64(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToInt64()
        {
            if(Utf8SpanHelper.TryParseInt64(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToUInt64(out ulong value)
        {
            return Utf8SpanHelper.TryParseUInt64(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ToUInt64()
        {
            if(Utf8SpanHelper.TryParseUInt64(AsSpan(), out var result) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToInt16(out short value)
        {
            return Utf8SpanHelper.TryParseInt16(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ToInt16()
        {
            if(TryToInt16(out var value) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToUInt16(out ushort value)
        {
            return Utf8SpanHelper.TryParseUInt16(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ToUInt16()
        {
            if(TryToUInt16(out var value) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToInt8(out sbyte value)
        {
            return Utf8SpanHelper.TryParseInt8(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ToInt8()
        {
            if(TryToInt8(out var value) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryToUInt8(out byte value)
        {
            return Utf8SpanHelper.TryParseUInt8(AsSpan(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ToUInt8()
        {
            if(TryToUInt8(out var value) == false) {
                ThrowHelper.ThrowInvalidOperation(InvalidFormatMessage);
            }
            return value;
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
