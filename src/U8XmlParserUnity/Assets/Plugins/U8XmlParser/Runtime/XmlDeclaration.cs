#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    [DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct XmlDeclaration : IEquatable<XmlDeclaration>
    {
        private readonly XmlDeclaration_* _declaration;

        public XmlAttribute Version => new XmlAttribute(_declaration->Version);
        public XmlAttribute Encoding => new XmlAttribute(_declaration->Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlDeclaration(XmlDeclaration_* declaration)
        {
            _declaration = declaration;
        }

        public RawString AsRawString() => _declaration != null ? _declaration->Body : RawString.Empty;

        public override bool Equals(object? obj) => obj is XmlDeclaration declaration && Equals(declaration);

        public bool Equals(XmlDeclaration other) => _declaration == other._declaration;

        public override int GetHashCode() => new IntPtr(_declaration).GetHashCode();

        public override string ToString() => _declaration != null ? _declaration->Body.ToString() : "";
    }

    internal unsafe struct XmlDeclaration_
    {
        public RawString Body;
        public XmlAttribute_* Version;
        public XmlAttribute_* Encoding;
    }
}
