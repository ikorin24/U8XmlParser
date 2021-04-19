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
        private static readonly float[] _pow10FloatTable = new float[21]
        {
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
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloatPow10(int value)
        {
            var index = (value + 10);
            if((uint)index < _pow10FloatTable.Length) {
                return _pow10FloatTable[index];
            }
            else {
                return Pow10Slow(value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static float Pow10Slow(int value)
            {
#if AVAILABLE_MATH_F
                return MathF.Pow(10f, value);
#else
                return (float)Math.Pow(10f, value);
#endif
            }
        }
    }
}
