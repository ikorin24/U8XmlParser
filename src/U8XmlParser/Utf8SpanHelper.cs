#nullable enable
using System;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    public static class Utf8SpanHelper
    {
        public static bool TryParseInt32(ReadOnlySpan<byte> utf8String, out int result)
        {
            // Regex
            // ^(-|\+)?[0-9]+$

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out long i)
        {
            i = (long)(a - '0');
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out ulong i)
        {
            i = (ulong)(a - '0');
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out short i)
        {
            i = (short)(a - '0');
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out ushort i)
        {
            i = (ushort)(a - '0');
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out sbyte i)
        {
            i = (sbyte)(a - '0');
            return i <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Atoi(byte a, out byte i)
        {
            i = (byte)(a - '0');
            return i <= '9';
        }
    }
}
