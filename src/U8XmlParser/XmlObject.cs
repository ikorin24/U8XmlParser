#nullable enable
using System;
using UnmanageUtility;
using System.Threading;
using U8Xml.Internal;

namespace U8Xml
{
    public sealed class XmlObject : IDisposable
    {
        private UnmanagedList<byte>? _data;
        private CustomList<XmlNode> _nodes;
        private CustomList<XmlAttribute> _attributes;
        private AllNodesList? _allNodes;

        public bool IsDisposed => _data is null;

        public unsafe XmlNodeList Children => _nodes.IsDisposed ? XmlNodeList.Empty : new XmlNodeList((IntPtr)_nodes.FirstItem);

        internal XmlObject(UnmanagedList<byte> data, CustomList<XmlNode> nodes, CustomList<XmlAttribute> attributes)
        {
            _data = data;
            _nodes = nodes;
            _attributes = attributes;
        }

        public void Dispose()
        {
            var data = Interlocked.Exchange(ref _data, null);
            if (data is not null)
            {
                data.Dispose();
                _nodes.Dispose();
                _attributes.Dispose();
            }
        }

        public AllNodesList AllNodes() => _allNodes ??= new AllNodesList(_nodes);
    }
}
