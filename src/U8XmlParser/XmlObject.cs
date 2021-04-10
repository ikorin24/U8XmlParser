#nullable enable
using System;
using System.Threading;
using U8Xml.Internal;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    public unsafe sealed class XmlObject : IDisposable
    {
        private IntPtr _rawByteData;
        private int _byteLength;
        private int _offset;        // 0 or 3 (3 is UTF-8 BOM)
        private CustomList<XmlNode_> _nodes;
        private CustomList<XmlAttribute> _attributes;

        public bool IsDisposed => _rawByteData == IntPtr.Zero;

        public XmlNode Root => new XmlNode(_nodes.FirstItem);

        internal XmlObject(ref UnmanagedBuffer buffer, int offset, CustomList<XmlNode_> nodes, CustomList<XmlAttribute> attributes)
        {
            buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
            _offset = offset;
            _nodes = nodes;
            _attributes = attributes;
        }

        public void Dispose()
        {
            var data = Interlocked.Exchange(ref _rawByteData, default);
            if(data != IntPtr.Zero) {
                AllocationSafety.Remove(_byteLength);
                Marshal.FreeHGlobal(_rawByteData);
                _rawByteData = IntPtr.Zero;
                _byteLength = 0;
                _offset = 0;
                _nodes.Dispose();
                _attributes.Dispose();
            }
        }

        public RawString AsRawString() => new RawString((byte*)_rawByteData + _offset, _byteLength - _offset);
    }
}
