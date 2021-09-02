#nullable enable

#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if !(NETSTANDARD2_0 || NET48 || IS_UNITY)
#define ENCODING_SPAN_API
#endif


#if !ENCODING_SPAN_API
using System;
using System.Text;

namespace U8Xml.Internal
{
    internal static class EncodingExtension
    {
        public static unsafe int GetByteCount(this Encoding encoding, ReadOnlySpan<char> span)
        {
            fixed(char* ptr = span) {
                return encoding.GetByteCount(ptr, span.Length);
            }
        }
    }
}
#endif  // ENCODING_SPAN_API
