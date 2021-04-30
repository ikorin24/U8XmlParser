#nullable enable
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1
#define AVAILABLE_MATH_F
#endif

using System;
using System.Runtime.CompilerServices;

namespace U8Xml.Internal
{
    internal static class MathHelper
    {
        private static readonly float[] _pow10FloatTable = new float[84]
        {
            1e-45f,
            1e-44f,
            1e-43f,
            1e-42f,
            1e-41f,
            1e-40f,
            1e-39f,
            1e-38f,
            1e-37f,
            1e-36f,
            1e-35f,
            1e-34f,
            1e-33f,
            1e-32f,
            1e-31f,
            1e-30f,
            1e-29f,
            1e-28f,
            1e-27f,
            1e-26f,
            1e-25f,
            1e-24f,
            1e-23f,
            1e-22f,
            1e-21f,
            1e-20f,
            1e-19f,
            1e-18f,
            1e-17f,
            1e-16f,
            1e-15f,
            1e-14f,
            1e-13f,
            1e-12f,
            1e-11f,
            1e-10f,
            1e-9f,
            1e-8f,
            1e-7f,
            1e-6f,
            1e-5f,
            1e-4f,
            1e-3f,
            1e-2f,
            1e-1f,
            1f,
            1e+1f,
            1e+2f,
            1e+3f,
            1e+4f,
            1e+5f,
            1e+6f,
            1e+7f,
            1e+8f,
            1e+9f,
            1e+10f,
            1e+11f,
            1e+12f,
            1e+13f,
            1e+14f,
            1e+15f,
            1e+16f,
            1e+17f,
            1e+18f,
            1e+19f,
            1e+20f,
            1e+21f,
            1e+22f,
            1e+23f,
            1e+24f,
            1e+25f,
            1e+26f,
            1e+27f,
            1e+28f,
            1e+29f,
            1e+30f,
            1e+31f,
            1e+32f,
            1e+33f,
            1e+34f,
            1e+35f,
            1e+36f,
            1e+37f,
            1e+38f,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFloatPow10(int value, out float result)
        {
            var index = (value + 45);
            if((uint)index < _pow10FloatTable.Length) {
                result = _pow10FloatTable[index];
                return true;
            }
            result = 0;
            return false;
        }
    }
}
