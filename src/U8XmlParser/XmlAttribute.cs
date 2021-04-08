#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    /// <summary>Attribute of node in xml</summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct XmlAttribute : IEquatable<XmlAttribute>
    {
        /// <summary>Attribute name</summary>
        public readonly RawString Name;
        /// <summary>Attribute value</summary>
        public readonly RawString Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute(RawString name, RawString value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"{Name.ToString()}=\"{Value.ToString()}\"";

        public override bool Equals(object? obj) => obj is XmlAttribute attribute && Equals(attribute);

        public bool Equals(XmlAttribute other) => Name.Equals(other.Name) && Value.Equals(other.Value);

        public override int GetHashCode() => HashCode.Combine(Name, Value);

        public static bool operator ==(XmlAttribute left, XmlAttribute right) => left.Equals(right);

        public static bool operator !=(XmlAttribute left, XmlAttribute right) => !(left == right);
    }
}
