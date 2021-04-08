#nullable enable
using System;
using System.Threading;
using U8Xml.Internal;
using System.Runtime.InteropServices;

namespace U8Xml
{
    public sealed class XmlObject : IDisposable
    {
        private IntPtr _rawByteData;
        private int _byteLength;
        private int _offset;        // 0 or 3 (3 is UTF-8 BOM)
        private CustomList<XmlNode> _nodes;
        private CustomList<XmlAttribute> _attributes;
        private AllNodesList? _allNodes;

        public bool IsDisposed => _rawByteData == IntPtr.Zero;

        public unsafe XmlNodeList Children => _nodes.IsDisposed ? XmlNodeList.Empty : new XmlNodeList((IntPtr)_nodes.FirstItem);

        internal XmlObject(ref UnmanagedBuffer buffer, int offset, CustomList<XmlNode> nodes, CustomList<XmlAttribute> attributes)
        {
            buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
            _offset = offset;
            _nodes = nodes;
            _attributes = attributes;
        }

        public void Dispose()
        {
            var data = Interlocked.Exchange(ref _rawByteData, default);
            if (data != IntPtr.Zero)
            {
                AllocationSafety.Remove(_byteLength);
                Marshal.FreeHGlobal(_rawByteData);
                _rawByteData = IntPtr.Zero;
                _byteLength = 0;
                _offset = 0;
                _nodes.Dispose();
                _attributes.Dispose();
            }
        }

        public AllNodesList AllNodes() => _allNodes ??= new AllNodesList(_nodes);

        public unsafe RawString AsRawString()
        {
            if (IsDisposed) { ThrowHelper.ThrowDisposed(nameof(XmlObject)); }
            return new RawString((byte*)_rawByteData + _offset, _byteLength - _offset);
        }
    }
}
