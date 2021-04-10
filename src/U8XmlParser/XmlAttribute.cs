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
        private readonly IntPtr _attr;  // XmlAttribute_*

        /// <summary>Get attribute name</summary>
        public ref readonly RawString Name => ref ((XmlAttribute_*)_attr)->Name;
        /// <summary>Get attribute value</summary>
        public ref readonly RawString Value => ref ((XmlAttribute_*)_attr)->Value;

        internal XmlAttribute(XmlAttribute_* attr) => _attr = (IntPtr)attr;

        public override bool Equals(object? obj) => obj is XmlAttribute attribute && Equals(attribute);

        public bool Equals(XmlAttribute other) => _attr == other._attr;

        public override int GetHashCode() => _attr.GetHashCode();

        public override string ToString() => _attr == IntPtr.Zero ? "" : ((XmlAttribute_*)_attr)->ToString();
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal unsafe readonly struct XmlAttribute_ : IEquatable<XmlAttribute_>
    {
        /// <summary>Attribute name</summary>
        public readonly RawString Name;
        /// <summary>Attribute value</summary>
        public readonly RawString Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute_(RawString name, RawString value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"{Name.ToString()}=\"{Value.ToString()}\"";

        public override bool Equals(object? obj) => obj is XmlAttribute_ attribute && Equals(attribute);

        public bool Equals(XmlAttribute_ other) => Name.Equals(other.Name) && Value.Equals(other.Value);

        public override int GetHashCode() => HashCode.Combine(Name, Value);
    }
}
