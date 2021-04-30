#nullable enable
using System;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    public static class Utf8SpanHelper
    {
        /// <summary>utf-8 bytes of "∞"</summary>
        private static ReadOnlySpan<byte> InfinityUtf8Str => new byte[] { 0xE2, 0x88, 0x9E };

        /// <summary>Copy utf-8 data to the specified buffer with converting lowercase alphabet into uppercase.</summary>
        /// <param name="utf8Data">source utf-8 data</param>
        /// <param name="buffer">buffer to copy</param>
        public static void ToUpper(ReadOnlySpan<byte> utf8Data, Span<byte> buffer)
        {
            if(buffer.Length < utf8Data.Length) { ThrowHelper.ThrowArg("buffer is too short."); }

            const uint offset = (uint)'z' - (uint)'a';
            for(int i = 0; i < utf8Data.Length; i++) {
                if(utf8Data[i] - (uint)'a' <= offset) {
                    buffer.At(i) = (byte)(utf8Data[i] - 32);
                }
                else {
                    buffer.At(i) = utf8Data[i];
                }
            }
        }

        /// <summary>Copy utf-8 data with converting lowercase alphabet into uppercase.</summary>
        /// <param name="utf8Data">source utf-8 data</param>
        /// <returns>copied utf-8 data</returns>
        public static byte[] ToUpper(ReadOnlySpan<byte> utf8Data)
        {
            var buffer = new byte[utf8Data.Length];
            ToUpper(utf8Data, buffer);
            return buffer;
        }

        /// <summary>Copy utf-8 data to the specified buffer with converting uppercase alphabet into lowercase.</summary>
        /// <param name="utf8Data">source utf-8 data</param>
        /// <param name="buffer">buffer to copy</param>
        public static void ToLower(ReadOnlySpan<byte> utf8Data, Span<byte> buffer)
        {
            if(buffer.Length < utf8Data.Length) { ThrowHelper.ThrowArg("buffer is too short."); }

            const uint offset = (uint)'Z' - (uint)'A';
            for(int i = 0; i < utf8Data.Length; i++) {
                if((utf8Data[i] - (uint)'A' <= offset)) {
                    buffer.At(i) = (byte)(utf8Data[i] + 32);
                }
                else {
                    buffer.At(i) = utf8Data[i];
                }
            }
        }

        /// <summary>Copy utf-8 data with converting uppercase alphabet into lowercase.</summary>
        /// <param name="utf8Data">source utf-8 data</param>
        /// <returns>copied utf-8 data</returns>
        public static byte[] ToLower(ReadOnlySpan<byte> utf8Data)
        {
            var buffer = new byte[utf8Data.Length];
            ToLower(utf8Data, buffer);
            return buffer;
        }

        public static int DecodeEscape(ReadOnlySpan<byte> utf8Data, Span<byte> buffer)
        {
            throw new NotImplementedException();
            // &#\d+;
            // &#x[0-9a-fA-F]+;
            // &lt;
            // &gt;
            // &amp;
            // &quot;
            // &apos;


            // https://www.ipentec.com/document/xml-character-escape
            // https://gorogoronyan.web.fc2.com/htmlsample/html5_crlf1.html
            int i = 0;
            for(int j = 0; j < utf8Data.Length; j++) {
                if(utf8Data.At(j) == '&') {
                }
                if((uint)i >= buffer.Length) { ThrowHelper.ThrowArgOutOfRange("buffer is too short"); }
                buffer.At(i++) = utf8Data.At(j);
            }
            return i;
        }

        public static bool TryParseInt32(ReadOnlySpan<byte> utf8String, out int result)
        {
            // Regex
            // ^(-|\+)?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            int sign = 1;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '-') {
                sign = -1;
                i++;
            }
            else if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            result *= sign;

            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out int num) == false) { return Error(out result); }
                result = checked(result * 10 + num * sign);
            }
            return true;

            static bool Error(out int result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseUInt32(ReadOnlySpan<byte> utf8String, out uint result)
        {
            // Regex
            // ^\+?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out uint num) == false) { return Error(out result); }
                result = checked(result * 10 + num);
            }
            return true;

            static bool Error(out uint result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseInt64(ReadOnlySpan<byte> utf8String, out long result)
        {
            // Regex
            // ^(-|\+)?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            int sign = 1;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '-') {
                sign = -1;
                i++;
            }
            else if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            result *= sign;

            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out int num) == false) { return Error(out result); }
                result = checked(result * 10 + num * sign);
            }
            return true;

            static bool Error(out long result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseUInt64(ReadOnlySpan<byte> utf8String, out ulong result)
        {
            // Regex
            // ^\+?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out uint num) == false) { return Error(out result); }
                result = checked(result * 10 + num);
            }
            return true;

            static bool Error(out ulong result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseInt16(ReadOnlySpan<byte> utf8String, out short result)
        {
            // Regex
            // ^(-|\+)?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            short sign = 1;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '-') {
                sign = -1;
                i++;
            }
            else if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            result *= sign;

            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out int num) == false) { return Error(out result); }
                result = (short)checked(result * 10 + num * sign);
            }
            return true;

            static bool Error(out short result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseUInt16(ReadOnlySpan<byte> utf8String, out ushort result)
        {
            // Regex
            // ^\+?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out uint num) == false) { return Error(out result); }
                result = (ushort)checked(result * 10 + num);
            }
            return true;

            static bool Error(out ushort result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseInt8(ReadOnlySpan<byte> utf8String, out sbyte result)
        {
            // Regex
            // ^(-|\+)?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            sbyte sign = 1;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '-') {
                sign = -1;
                i++;
            }
            else if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            result *= sign;

            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out int num) == false) { return Error(out result); }
                result = (sbyte)checked(result * 10 + num * sign);
            }
            return true;

            static bool Error(out sbyte result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseUInt8(ReadOnlySpan<byte> utf8String, out byte result)
        {
            // Regex
            // ^\+?[0-9]+$

            utf8String = utf8String.Trim();
            int i = 0;
            if(i >= utf8String.Length) { return Error(out result); }
            if(utf8String.At(i) == '+') {
                i++;
            }
            if(i >= utf8String.Length) { return Error(out result); }
            if(Atoi(utf8String.At(i++), out result) == false) { return Error(out result); }
            while(true) {
                if(i >= utf8String.Length) { break; }
                if(Atoi(utf8String.At(i++), out int num) == false) { return Error(out result); }
                result = (byte)checked(result * 10 + num);
            }
            return true;

            static bool Error(out byte result)
            {
                result = 0;
                return false;
            }
        }

        public static bool TryParseFloat32(ReadOnlySpan<byte> utf8String, out float result)
        {
            // Regex
            // ^(-|\+)?((\d+\.?\d*)|(\.\d+))((E|e)(-|\+)?\d+)?$
            // Regex (nan)
            // ^(-|\+)?(N|n)(A|a)(N|n)$
            // Regex (infinity)
            // ^(-|\+)?∞$

            int i = 0;
            int sign;
            float frac;
            int expSign = 1;
            int exp = 0;

            utf8String = utf8String.Trim();

            // (-|\+)?
            if(TryParseSign(utf8String, ref i, out sign) == false) { return Error(out result); }

            if(i >= utf8String.Length) { return Error(out result); }

            // (N|n)(A|a)(N|n)
            if(utf8String.At(i) == 'N' || utf8String.At(i) == 'n') {
                if((i + 2 < utf8String.Length) &&
                    (utf8String.At(i + 1) == 'a' || utf8String.At(i + 1) == 'A') &&
                    (utf8String.At(i + 2) == 'N' || utf8String.At(i + 2) == 'n')) {
                    result = float.NaN;
                    return true;
                }
                return Error(out result);
            }

            // ∞
            if(utf8String.At(i) == InfinityUtf8Str[0]) {
                if((i + 2 < utf8String.Length) &&
                   (utf8String.At(i + 1) == InfinityUtf8Str[1]) &&
                   (utf8String.At(i + 2) == InfinityUtf8Str[2])) {
                    result = sign == 1 ? float.PositiveInfinity : float.NegativeInfinity;
                    return true;
                }
                return Error(out result);
            }

            // ((\d+\.?\d*)|(\.\d+))
            if(TryParseDecimals(utf8String, ref i, out frac) == false) {
                return Error(out result);
            }

            // ((E|e)(-|\+)?\d+)?$
            if(i < utf8String.Length) {
                var current = utf8String.At(i++);
                if(current != 'E' && current != 'e') { return Error(out result); }

                // (-|\+)?
                if(TryParseSign(utf8String, ref i, out expSign) == false) { return Error(out result); }

                // \d+$
                if(TryParseNumbers(utf8String, ref i, out exp, out _, false) == false) { return Error(out result); }
                if(i != utf8String.Length) { return Error(out result); }
            }

            if(MathHelper.TryFloatPow10(expSign * exp, out var pow) == false) { return Error(out result); }
            result = sign * frac * pow;
            return true;


            static bool TryParseNumbers(in ReadOnlySpan<byte> utf8String, ref int i, out int num, out int numLength, bool isOptional)
            {
                // isOptional
                // true  : \d*
                // false : \d+

                num = 0;
                numLength = 0;
                if(isOptional == false) {
                    if(i >= utf8String.Length) { return false; }
                    if(Atoi(utf8String.At(i++), out num) == false) { return false; }
                    numLength++;
                }
                while(true) {
                    if(i >= utf8String.Length) { break; }
                    if(Atoi(utf8String.At(i), out int a) == false) { break; }
                    i++;
                    num = checked(num * 10 + a);
                    numLength++;
                }
                return true;
            }

            static bool TryParseDecimals(in ReadOnlySpan<byte> utf8String, ref int i, out float value)
            {
                // Check number
                // ((\d+\.?\d*)|(\.\d+))
                value = 0f;

                if(i >= utf8String.Length) { return false; }
                if(utf8String.At(i) == '.') {
                    i++;
                    // \d+
                    if(TryParseNumbers(utf8String, ref i, out var num, out var len, false)) {
                        if(MathHelper.TryFloatPow10(-len, out var pow) == false) { return false; }
                        value = num * pow;
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    // \d+\.?\d*
                    if(TryParseNumbers(utf8String, ref i, out var num1, out var len1, false) == false) { return false; }
                    if(i == utf8String.Length) {
                        value = num1;
                        return true;
                    }
                    if(utf8String.At(i) == '.') {
                        i++;
                        if(TryParseNumbers(utf8String, ref i, out var num2, out var len2, true)) {
                            if(MathHelper.TryFloatPow10(-len2, out var pow) == false) { return false; }
                            value = num1 + pow * num2;
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        value = num1;
                        return true;
                    }
                }
            }

            static bool TryParseSign(in ReadOnlySpan<byte> utf8String, ref int i, out int sign)
            {
                if(i >= utf8String.Length) {
                    sign = default;
                    return false;
                }
                if(utf8String.At(i) == '-') {
                    sign = -1;
                    i++;
                }
                else if(utf8String.At(i) == '+') {
                    sign = 1;
                    i++;
                }
                else {
                    sign = 1;
                }
                return true;
            }

            static bool Error(out float result)
            {
                result = 0;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out int i)
        {
            i = a - '0';
            return (uint)i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out uint i)
        {
            i = (uint)(a - '0');
            return i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out long i)
        {
            i = (long)(a - '0');
            return (ulong)i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out ulong i)
        {
            i = (ulong)(a - '0');
            return i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out short i)
        {
            i = (short)(a - '0');
            return (ushort)i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out ushort i)
        {
            i = (ushort)(a - '0');
            return i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out sbyte i)
        {
            i = (sbyte)(a - '0');
            return (byte)i <= 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out byte i)
        {
            i = (byte)(a - '0');
            return i <= 9;
        }
    }
}
