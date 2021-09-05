#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    public unsafe readonly struct XmlNodeDescendantList : IEnumerable<XmlNode>
    {
        private readonly XmlNode_* _parent;

        internal XmlNodeDescendantList(XmlNode_* parent)
        {
            Debug.Assert(parent != null);
            _parent = parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(_parent);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_parent);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_parent);

        public struct Enumerator : IEnumerator<XmlNode>
        {
            private CustomList<XmlNode_>.Enumerator _e;     // Don't make it readonly.
            private readonly int _depth;

            public XmlNode Current => new XmlNode(_e.Current);

            object IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(XmlNode_* parent)
            {
                var firstChild = parent->FirstChild;
                if(firstChild == null) {
                    this = default;         // default instance is valid.
                }
                else {
                    _e = parent->WholeNodes.GetEnumerator(firstChild->NodeIndex);
                    _depth = parent->Depth;
                }
            }

            public void Dispose() => _e.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if(_e.MoveNext() == false) {
                    return false;
                }
                if(_e.Current->Depth <= _depth) {
                    return false;
                }
                return true;
            }

            public void Reset() => _e.Reset();
        }

        private sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private Enumerator _e;  // Don't make it readonly.

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EnumeratorClass(XmlNode_* parent)
            {
                _e = new Enumerator(parent);
            }

            public XmlNode Current => _e.Current;

            object IEnumerator.Current => _e.Current;

            public void Dispose() => _e.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _e.MoveNext();

            public void Reset() => _e.Reset();
        }
    }
}
