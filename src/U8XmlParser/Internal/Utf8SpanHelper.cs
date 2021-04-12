#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace U8Xml.Internal
{
    internal static class Utf8SpanHelper
    {
        internal static bool TryParseInt(ReadOnlySpan<byte> value, out int result)
        {
            // ^(-|\+)?[0-9]+$
            int i = 0;
            var sign = 1;
            if(i >= value.Length) { return Error(out result); }
            if(value.At(i) == '-') {
                sign = -1;
                i++;
            }
            else if(value.At(i) == '+') {
                i++;
            }
            if(i >= value.Length) { return Error(out result); }
            if(Atoi(value.At(i++), out result) == false) { return Error(out result); }
            result *= sign;

            while(true) {
                if(i >= value.Length) { break; }
                if(Atoi(value.At(i++), out int num) == false) { return Error(out result); }
                result = checked(result * 10 + num * sign);
            }
            return true;

            static bool Error(out int result)
            {
                result = 0;
                return false;
            }
        }

        internal static bool TryParseUInt(ReadOnlySpan<byte> value, out uint result)
        {
            // ^\+?[0-9]+$
            int i = 0;
            if(i >= value.Length) { return Error(out result); }
            if(value.At(i) == '+') {
                i++;
            }
            if(i >= value.Length) { return Error(out result); }
            if(Atoi(value.At(i++), out result) == false) { return Error(out result); }
            while(true) {
                if(i >= value.Length) { break; }
                if(Atoi(value.At(i++), out uint num) == false) { return Error(out result); }
                result = checked(result * 10 + num);
            }
            return true;

            static bool Error(out uint result)
            {
                result = 0;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out int i)
        {
            i = a - '0';
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out uint i)
        {
            i = (uint)(a - '0');
            return i <= '9';
        }
    }
}
