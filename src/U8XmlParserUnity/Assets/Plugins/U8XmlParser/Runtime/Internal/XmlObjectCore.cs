#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;

namespace U8Xml.Internal
{
    internal unsafe readonly struct XmlObjectCore : IDisposable, IXmlObject
    {
        private readonly IntPtr _rawByteData;
        private readonly int _byteLength;
        private readonly int _offset;        // 0 or 3 (3 is UTF-8 BOM)
        private readonly CustomList<XmlNode_> _nodes;
        private readonly CustomList<XmlAttribute_> _attributes;
        private readonly OptionalNodeList _optional;
        private readonly RawStringTable _entities;

        public bool IsDisposed => _rawByteData == IntPtr.Zero;

        public XmlNode Root => new XmlNode(_nodes.FirstItem);

        public Option<XmlDeclaration> Declaration => new XmlDeclaration(_optional.Declaration);

        public Option<XmlDocumentType> DocumentType => new XmlDocumentType(_optional.DocumentType);

        public XmlEntityTable EntityTable => new XmlEntityTable(_entities);

        internal XmlObjectCore(ref UnmanagedBuffer buffer, int offset, CustomList<XmlNode_> nodes, CustomList<XmlAttribute_> attributes, OptionalNodeList optional, RawStringTable entities)
        {
            buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
            _offset = offset;
            _nodes = nodes;
            _attributes = attributes;
            _optional = optional;
            _entities = entities;
        }

        public void Dispose()
        {
            var data = Interlocked.Exchange(ref Unsafe.AsRef(_rawByteData), default);
            if(data != IntPtr.Zero) {
                AllocationSafety.Remove(_byteLength);
                Marshal.FreeHGlobal(data);
                Unsafe.AsRef(_byteLength) = 0;
                Unsafe.AsRef(_offset) = 0;
                _nodes.Dispose();
                _attributes.Dispose();
                _optional.Dispose();
                _entities.Dispose();
            }
        }

        /// <summary>Get whole xml string as utf-8 bytes data.</summary>
        /// <returns>whole xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => new RawString((byte*)_rawByteData + _offset, _byteLength - _offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes() => new AllNodeList(_nodes, XmlNodeType.ElementNode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes(XmlNodeType? targetType) => new AllNodeList(_nodes, targetType);
    }
}
