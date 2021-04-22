#nullable enable
using System;
using System.Threading;
using U8Xml.Internal;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    /// <summary>Xml structual object parsed from xml file.</summary>
    /// <remarks>[CAUTION] DO NOT call any methods or properties except <see cref="IsDisposed"/> after calling <see cref="Dispose"/>.</remarks>
    public unsafe sealed class XmlObject : IDisposable
    {
        private IntPtr _rawByteData;
        private int _byteLength;
        private int _offset;        // 0 or 3 (3 is UTF-8 BOM)
        private CustomList<XmlNode_> _nodes;
        private CustomList<XmlAttribute_> _attributes;
        private OptionalNodeList _optional;

        /// <summary>Get whether the xml object is disposed or not.</summary>
        /// <remarks>DO NOT call any other methods or properties if the property is false.</remarks>
        public bool IsDisposed => _rawByteData == IntPtr.Zero;

        /// <summary>Get th root node</summary>
        public XmlNode Root => new XmlNode(_nodes.FirstItem);

        /// <summary>Get xml declaration</summary>
        public XmlDeclaration Declaration => new XmlDeclaration(_optional.Declaration);

        internal XmlObject(ref UnmanagedBuffer buffer, int offset, CustomList<XmlNode_> nodes, CustomList<XmlAttribute_> attributes, OptionalNodeList optional)
        {
            buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
            _offset = offset;
            _nodes = nodes;
            _attributes = attributes;
            _optional = optional;
        }

        ~XmlObject() => DisposePrivate();

        /// <summary>Dispose the xml object and release all memoriess it has.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposePrivate();
        }

        private void DisposePrivate()
        {
            var data = Interlocked.Exchange(ref _rawByteData, default);
            if(data != IntPtr.Zero) {
                GC.SuppressFinalize(this);
                AllocationSafety.Remove(_byteLength);
                Marshal.FreeHGlobal(_rawByteData);
                _rawByteData = IntPtr.Zero;
                _byteLength = 0;
                _offset = 0;
                _nodes.Dispose();
                _attributes.Dispose();
                _optional.Dispose();
            }
        }

        /// <summary>Get whole xml string as utf-8 bytes data.</summary>
        /// <returns>whole xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => new RawString((byte*)_rawByteData + _offset, _byteLength - _offset);
    }
}
