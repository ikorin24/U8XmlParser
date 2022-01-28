#nullable enable

#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if !(NETSTANDARD2_0 || NET48) && !IS_UNITY
#define CODE_ANALYTICS
#endif

using System;
#if CODE_ANALYTICS
using System.Diagnostics.CodeAnalysis;
#endif


namespace U8Xml.Internal
{
    internal static class ThrowHelper
    {
#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowNotSupported(string message)
        {
            throw new NotSupportedException(message);
        }

#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowNullArg(string message)
        {
            throw new ArgumentNullException(message);
        }


#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowFormatException(string? message = null)
        {
            throw new FormatException(message);
        }

#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowArgOutOfRange(string? message = null)
        {
            throw new ArgumentOutOfRangeException(message);
        }

#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowArg(string? message = null)
        {
            throw new ArgumentException(message);
        }

#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowInvalidOperation(string? message = null)
        {
            throw new InvalidOperationException(message);
        }

#if CODE_ANALYTICS
        [DoesNotReturn]
#endif
        public static void ThrowDisposed(string objectName)
        {
            throw new ObjectDisposedException(objectName);
        }
    }
}
