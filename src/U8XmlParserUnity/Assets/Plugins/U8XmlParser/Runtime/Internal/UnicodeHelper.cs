#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace U8Xml.Internal
{
    internal static class UnicodeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryEncodeCodePointToUtf8(uint codePoint, Span<byte> destination, out int bytesWritten)
        {
#if NETCOREAPP3_1_OR_GREATER
            return new System.Text.Rune(codePoint).TryEncodeToUtf8(destination, out bytesWritten);
#else
            return TryEncodeToUtf8ForLegacyRuntime(codePoint, destination, out bytesWritten);
#endif
        }

#if !NETCOREAPP3_1_OR_GREATER

        // The following implementation is copied from System.Text.Rune
        // 
        // original source code
        // https://github.com/dotnet/runtime/blob/be19e1cc6aad0f28e3bf1d7c7201c455a96b3a9d/src/libraries/System.Private.CoreLib/src/System/Text/Rune.cs#L1061

        private static bool TryEncodeToUtf8ForLegacyRuntime(uint codePoint, Span<byte> destination, out int bytesWritten)
        {
            // The bit patterns below come from the Unicode Standard, Table 3-6.

            if(!destination.IsEmpty) {
                if(codePoint <= 0x7Fu) {
                    destination[0] = (byte)codePoint;
                    bytesWritten = 1;
                    return true;
                }

                if(1 < (uint)destination.Length) {
                    if((int)codePoint <= 0x7FFu) {
                        // Scalar 00000yyy yyxxxxxx -> bytes [ 110yyyyy 10xxxxxx ]
                        destination[0] = (byte)((codePoint + (0b110u << 11)) >> 6);
                        destination[1] = (byte)((codePoint & 0x3Fu) + 0x80u);
                        bytesWritten = 2;
                        return true;
                    }

                    if(2 < (uint)destination.Length) {
                        if((int)codePoint <= 0xFFFFu) {
                            // Scalar zzzzyyyy yyxxxxxx -> bytes [ 1110zzzz 10yyyyyy 10xxxxxx ]
                            destination[0] = (byte)((codePoint + (0b1110 << 16)) >> 12);
                            destination[1] = (byte)(((codePoint & (0x3Fu << 6)) >> 6) + 0x80u);
                            destination[2] = (byte)((codePoint & 0x3Fu) + 0x80u);
                            bytesWritten = 3;
                            return true;
                        }

                        if(3 < (uint)destination.Length) {
                            // Scalar 000uuuuu zzzzyyyy yyxxxxxx -> bytes [ 11110uuu 10uuzzzz 10yyyyyy 10xxxxxx ]
                            destination[0] = (byte)((codePoint + (0b11110 << 21)) >> 18);
                            destination[1] = (byte)(((codePoint & (0x3Fu << 12)) >> 12) + 0x80u);
                            destination[2] = (byte)(((codePoint & (0x3Fu << 6)) >> 6) + 0x80u);
                            destination[3] = (byte)((codePoint & 0x3Fu) + 0x80u);
                            bytesWritten = 4;
                            return true;
                        }
                    }
                }
            }

            // Destination buffer not large enough

            bytesWritten = default;
            return false;
        }
#endif
    }
}
