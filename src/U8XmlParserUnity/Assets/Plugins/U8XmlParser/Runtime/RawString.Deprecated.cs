#nullable enable
using System;
using System.ComponentModel;

namespace U8Xml
{
    partial struct RawString
    {
        /// <summary>Use <see cref="StartsWith(RawString)"/> instead. (The correct method name is Start**s**With)</summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [Obsolete("Use RawString.StartsWith instead. (The method name is Start*s*With)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool StartWith(RawString other) => StartsWith(other);

        /// <summary>Use <see cref="StartsWith(ReadOnlySpan{byte})"/> instead. (The correct method name is Start**s**With)</summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [Obsolete("Use RawString.StartsWith instead. (The method name is Start*s*With)")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool StartWith(ReadOnlySpan<byte> other) => StartsWith(other);
    }
}
