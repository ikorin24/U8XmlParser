#nullable enable
#if NETCOREAPP3_1_OR_GREATER
#define SUPPORT_BIT_OPERATIONS
#endif

using System.Runtime.CompilerServices;

#if SUPPORT_BIT_OPERATIONS
using System.Numerics;
#else
using System;
using System.Runtime.InteropServices;
#endif

namespace U8Xml.Internal
{
    internal static class BitOperationHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
#if SUPPORT_BIT_OPERATIONS
            return BitOperations.Log2(value);
#else
            // https://source.dot.net/#System.Private.CoreLib/BitOperations.cs,178

            // The 0->0 contract is fulfilled by setting the LSB to 1.
            // Log(1) is 0, and setting the LSB for values > 1 does not change the log2 result.
            value |= 1;
            return Log2SoftwareFallback(value);
#endif
        }

#if !SUPPORT_BIT_OPERATIONS
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        private static int Log2SoftwareFallback(uint value)
        {
            // https://source.dot.net/#System.Private.CoreLib/BitOperations.cs,253

            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
#endif
    }
}
