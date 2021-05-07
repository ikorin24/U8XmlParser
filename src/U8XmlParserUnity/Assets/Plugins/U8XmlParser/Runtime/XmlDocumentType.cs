#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    [DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct XmlDocumentType : IEquatable<XmlDocumentType>, IReference
    {
        private readonly XmlDocumentType_* _docType;

        public bool IsNull => _docType == null;

        public RawString Name => _docType->Name;

        public RawString InternalSubset => _docType->InternalSubset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlDocumentType(XmlDocumentType_* docType)
        {
            _docType = docType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => _docType != null ? _docType->Body : RawString.Empty;

        public override bool Equals(object? obj) => obj is XmlDocumentType type && Equals(type);

        public bool Equals(XmlDocumentType other) => _docType == other._docType;

        public override int GetHashCode() => new IntPtr(_docType).GetHashCode();

        public override string ToString() => AsRawString().ToString();
    }

    internal unsafe struct XmlDocumentType_
    {
        public RawString Body;
        public RawString Name;
        public RawString InternalSubset;
    }
}
