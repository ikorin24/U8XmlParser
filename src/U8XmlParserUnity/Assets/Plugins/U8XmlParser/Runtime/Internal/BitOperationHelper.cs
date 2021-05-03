#nullable enable
#if NETCOREAPP3_1_OR_GREATER
#define SUPPORT_BIT_OPERATIONS
#endif

using System;
using System.Diagnostics;
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
        public static uint RotateLeft(uint value, int offset)
        {
            return (value << offset) | (value >> (32 - offset));
        }

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

    internal static unsafe class XXHash32
    {
        private static readonly uint _seed = (uint)DateTime.UtcNow.Ticks;

        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash(byte* data, int byteLength)
        {
            Debug.Assert(byteLength >= 0);

            if(byteLength == 0) { return 0; }
            if(byteLength < 16) {
                var acc = _seed + Prime5 + (uint)byteLength;
                return (int)ComputeHashShort(data, byteLength, acc);
            }
            else {
                return (int)ComputeHashFull(data, byteLength);
            }
        }

        private static uint ComputeHashShort(byte* data, int byteLength, uint acc)
        {
            Debug.Assert(byteLength < 16 && byteLength >= 0);

            var laneCount = Math.DivRem(byteLength, 4, out int mod4);
            if(laneCount == 1) {
                acc = AccumRemainingLane(acc, *(uint*)data);
            }
            else if(laneCount == 2) {
                acc = AccumRemainingLane(acc, *(uint*)data);
                acc = AccumRemainingLane(acc, *(uint*)(data + 4));
            }
            else if(laneCount == 3) {
                acc = AccumRemainingLane(acc, *(uint*)data);
                acc = AccumRemainingLane(acc, *(uint*)(data + 4));
                acc = AccumRemainingLane(acc, *(uint*)(data + 8));
            }

            byte* bytes = data + 4 * laneCount;
            if(mod4 == 1) {
                acc = AccumByte(acc, bytes[0]);
            }
            else if(mod4 == 2) {
                acc = AccumByte(acc, bytes[0]);
                acc = AccumByte(acc, bytes[1]);
            }
            else if(mod4 == 3) {
                acc = AccumByte(acc, bytes[0]);
                acc = AccumByte(acc, bytes[1]);
                acc = AccumByte(acc, bytes[2]);
            }
            return MixFinal(acc);
        }

        private static uint ComputeHashFull(byte* data, int byteLength)
        {
            var blockCount = Math.DivRem(byteLength, 16, out int mod16);
            Initialize(out uint acc1, out uint acc2, out uint acc3, out uint acc4);
            for(int i = 0; i < blockCount; i++) {
                uint lane1 = *(uint*)(data + 16 * i);
                uint lane2 = *(uint*)(data + 16 * i + 4);
                uint lane3 = *(uint*)(data + 16 * i + 8);
                uint lane4 = *(uint*)(data + 16 * i + 12);
                acc1 = AccumBlockLane(acc1, lane1);
                acc2 = AccumBlockLane(acc2, lane2);
                acc3 = AccumBlockLane(acc3, lane3);
                acc4 = AccumBlockLane(acc4, lane4);
            }
            uint acc = MixState(acc1, acc2, acc3, acc4) + (uint)byteLength;
            return ComputeHashShort(data + byteLength - mod16, mod16, acc);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
        {
            v1 = _seed + Prime1 + Prime2;
            v2 = _seed + Prime2;
            v3 = _seed;
            v4 = _seed - Prime1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AccumBlockLane(uint hash, uint lane)
        {
            return BitOperationHelper.RotateLeft(hash + lane * Prime2, 13) * Prime1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AccumRemainingLane(uint hash, uint lane)
        {
            return BitOperationHelper.RotateLeft(hash + lane * Prime3, 17) * Prime4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AccumByte(uint hash, byte b)
        {
            return BitOperationHelper.RotateLeft(hash + (uint)b * Prime5, 11) * Prime1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixState(uint v1, uint v2, uint v3, uint v4)
        {
            return BitOperationHelper.RotateLeft(v1, 1) + BitOperationHelper.RotateLeft(v2, 7) + BitOperationHelper.RotateLeft(v3, 12) + BitOperationHelper.RotateLeft(v4, 18);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash)
        {
            hash ^= hash >> 15;
            hash *= Prime2;
            hash ^= hash >> 13;
            hash *= Prime3;
            hash ^= hash >> 16;
            return hash;
        }
    }
}
