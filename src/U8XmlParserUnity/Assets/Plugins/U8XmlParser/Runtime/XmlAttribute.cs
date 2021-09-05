#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace U8Xml
{
    /// <summary>Attribute of node in xml</summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct XmlAttribute : IEquatable<XmlAttribute>, IReference
    {
        // Don't add any other fields. The layout must be same as IntPtr.
        private readonly IntPtr _attr;  // XmlAttribute_*

        public bool IsNull => _attr == IntPtr.Zero;

        /// <summary>Get attribute name</summary>
        public ref readonly RawString Name => ref ((XmlAttribute_*)_attr)->Name;
        /// <summary>Get attribute value</summary>
        public ref readonly RawString Value => ref ((XmlAttribute_*)_attr)->Value;

        internal XmlAttribute(XmlAttribute_* attr) => _attr = (IntPtr)attr;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out RawString name, out RawString value)
        {
            name = Name;
            value = Value;
        }

        public override bool Equals(object? obj) => obj is XmlAttribute attribute && Equals(attribute);

        public bool Equals(XmlAttribute other) => _attr == other._attr;

        public override int GetHashCode() => _attr.GetHashCode();

        public override string ToString() => _attr == IntPtr.Zero ? "" : ((XmlAttribute_*)_attr)->ToString();

        public static implicit operator (RawString Name, RawString Value)(XmlAttribute attr) => (attr.Name, attr.Value);

        public static bool operator ==(XmlAttribute attr, ValueTuple<RawString, RawString> pair) => attr.Name == pair.Item1 && attr.Value == pair.Item2;
        public static bool operator !=(XmlAttribute attr, ValueTuple<RawString, RawString> pair) => !(attr == pair);
        public static bool operator ==(ValueTuple<RawString, RawString> pair, XmlAttribute attr) => attr == pair;
        public static bool operator !=(ValueTuple<RawString, RawString> pair, XmlAttribute attr) => !(attr == pair);

        public static bool operator ==(XmlAttribute attr, ValueTuple<string, string> pair) => attr.Name == pair.Item1 && attr.Value == pair.Item2;
        public static bool operator !=(XmlAttribute attr, ValueTuple<string, string> pair) => !(attr == pair);
        public static bool operator ==(ValueTuple<string, string> pair, XmlAttribute attr) => attr == pair;
        public static bool operator !=(ValueTuple<string, string> pair, XmlAttribute attr) => !(attr == pair);
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal unsafe readonly struct XmlAttribute_
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
    }
}
