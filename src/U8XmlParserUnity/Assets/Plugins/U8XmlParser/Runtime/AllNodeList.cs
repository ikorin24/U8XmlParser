#nullable enable
using System.Collections;
using System.Collections.Generic;
using U8Xml.Internal;

namespace U8Xml
{
    public readonly unsafe struct AllNodeList : IEnumerable<XmlNode>
    {
        private readonly CustomList<XmlNode_> _nodes;

        public readonly int Count => _nodes.Count;

        internal AllNodeList(CustomList<XmlNode_> nodes)
        {
            _nodes = nodes;
        }

        public Enumerator GetEnumerator() => new Enumerator(_nodes.GetEnumerator());

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_nodes.GetEnumerator());

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_nodes.GetEnumerator());


        public struct Enumerator : IEnumerator<XmlNode>
        {
            private CustomList<XmlNode_>.Enumerator _e;

            internal Enumerator(CustomList<XmlNode_>.Enumerator e)
            {
                _e = e;
            }

            public XmlNode Current => new XmlNode(_e.Current);

            object IEnumerator.Current => Current;

            public void Dispose() => _e.Dispose();

            public bool MoveNext() => _e.MoveNext();

            public void Reset() => _e.Reset();
        }

        public sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private CustomList<XmlNode_>.Enumerator _e;

            internal EnumeratorClass(CustomList<XmlNode_>.Enumerator e)
            {
                _e = e;
            }

            public XmlNode Current => new XmlNode(_e.Current);

            object IEnumerator.Current => Current;

            public void Dispose() => _e.Dispose();

            public bool MoveNext() => _e.MoveNext();

            public void Reset() => _e.Reset();
        }
    }
}
