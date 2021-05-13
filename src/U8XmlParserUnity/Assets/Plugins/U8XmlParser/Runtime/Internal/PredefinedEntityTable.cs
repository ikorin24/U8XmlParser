#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace U8Xml.Internal
{
    internal static class PredefinedEntityTable
    {
        private static ReadOnlySpan<byte> EntityAMP => new byte[1] { (byte)'&' };
        private static ReadOnlySpan<byte> EntityLT => new byte[1] { (byte)'<' };
        private static ReadOnlySpan<byte> EntityGT => new byte[1] { (byte)'>' };
        private static ReadOnlySpan<byte> EntityQUOT => new byte[1] { (byte)'"' };
        private static ReadOnlySpan<byte> EntityAPOS => new byte[1] { (byte)'\'' };

        public static unsafe bool TryGetPredefinedValue(in RawString alias, out ReadOnlySpan<byte> value)
        {
            if(alias.Length == 2) {
                if(alias.At(0) == 'l' && alias.At(1) == 't') {
                    // &lt;
                    value = EntityLT;
                    return true;
                }
                if(alias.At(0) == 'g' && alias.At(1) == 't') {
                    // &gt;
                    value = EntityGT;
                    return true;
                }
            }
            else if(alias.Length == 3) {
                if(alias.At(0) == 'a' && alias.At(1) == 'm' && alias.At(1) == 'p') {
                    // &amp;
                    value = EntityAMP;
                    return true;
                }
            }
            else if(alias.Length == 4) {
                if(alias.At(0) == 'q' && alias.At(1) == 'u' && alias.At(2) == 'o' && alias.At(3) == 't') {
                    // &quot;
                    value = EntityQUOT;
                    return true;
                }
                if(alias.At(0) == 'a' && alias.At(1) == 'p' && alias.At(2) == 'o' && alias.At(3) == 's') {
                    // &apos;
                    value = EntityAPOS;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
