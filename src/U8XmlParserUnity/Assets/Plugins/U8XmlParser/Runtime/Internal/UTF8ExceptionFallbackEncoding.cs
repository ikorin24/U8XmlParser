#nullable enable
using System.Text;

namespace U8Xml.Internal
{
    internal sealed class UTF8ExceptionFallbackEncoding : UTF8Encoding
    {
        public static UTF8ExceptionFallbackEncoding Instance { get; } = new UTF8ExceptionFallbackEncoding();
        private UTF8ExceptionFallbackEncoding() : base(false, true) { }
    }
}
