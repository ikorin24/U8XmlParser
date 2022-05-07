#nullable enable

#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if !NETSTANDARD2_0 && !NET48 && !IS_UNITY
#define CAN_USE_HASHCODE
#endif

using System;
using System.Collections.Generic;
using System.Text;

namespace U8Xml
{
    public struct XmlParserOptions : IEquatable<XmlParserOptions>
    {
        public Encoding? Encoding { get; set; }

        public IExternalDtdGetter? ExternalDtdGetter { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is XmlParserOptions options && Equals(options);
        }

        public bool Equals(XmlParserOptions other)
        {
            return EqualityComparer<Encoding?>.Default.Equals(Encoding, other.Encoding) &&
                   EqualityComparer<IExternalDtdGetter?>.Default.Equals(ExternalDtdGetter, other.ExternalDtdGetter);
        }

        public override int GetHashCode()
        {
#if CAN_USE_HASHCODE
            return HashCode.Combine(Encoding, ExternalDtdGetter);
#else
            int hashCode = 2032215521;
            hashCode = hashCode * -1521134295 + EqualityComparer<Encoding?>.Default.GetHashCode(Encoding);
            hashCode = hashCode * -1521134295 + EqualityComparer<IExternalDtdGetter?>.Default.GetHashCode(ExternalDtdGetter);
            return hashCode;
#endif
        }
    }

    public readonly struct XmlDtdParser
    {
        public void Parse(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
