#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace U8Xml
{
    [DebuggerDisplay("{DebugDisplay,nq}")]
    public unsafe readonly struct XmlNodeList : IEnumerable<XmlNode>, ICollection<XmlNode>
    {
        private readonly XmlNode_* _parent;

        public bool IsEmpty => _parent->FirstChild == null;

        public int Count => _parent->ChildCount;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugDisplay => _parent != null ? $"{nameof(XmlNode)}[{Count}]" : $"{nameof(XmlNode)} (invalid instance)";

        bool ICollection<XmlNode>.IsReadOnly => false;

        internal XmlNodeList(XmlNode_* parent)
        {
            _parent = parent;
        }

        public Enumerator GetEnumerator() => new Enumerator(_parent->FirstChild);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_parent->FirstChild);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_parent->FirstChild);

        void ICollection<XmlNode>.Add(XmlNode item) => throw new NotSupportedException();

        void ICollection<XmlNode>.Clear() => throw new NotSupportedException();

        bool ICollection<XmlNode>.Contains(XmlNode item) => throw new NotImplementedException();        // TODO:

        void ICollection<XmlNode>.CopyTo(XmlNode[] array, int arrayIndex) => throw new NotImplementedException();   // TODO:

        bool ICollection<XmlNode>.Remove(XmlNode item) => throw new NotSupportedException();

        public struct Enumerator : IEnumerator<XmlNode>
        {
            private XmlNode_* _current;
            private XmlNode_* _next;

            internal Enumerator(XmlNode_* firstChild)
            {
                _next = firstChild;
                _current = null;
            }

            public XmlNode Current => new XmlNode(_current);

            object IEnumerator.Current => Current;

            public void Dispose() { }   // nop

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if(_next == null) { return false; }
                _current = _next;
                _next = _next->Sibling;
                return true;
            }

            public void Reset() => throw new NotSupportedException("Reset() is not supported.");
        }

        private sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private Enumerator _enumerator;     // mutable object, don't make it readonly.

            public XmlNode Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EnumeratorClass(XmlNode_* firstChild)
            {
                _enumerator = new Enumerator(firstChild);
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}
