#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{DebugDisplay,nq}")]
    [DebuggerTypeProxy(typeof(XmlNodeListTypeProxy))]
    public unsafe readonly struct XmlNodeList : IEnumerable<XmlNode>, ICollection<XmlNode>
    {
        private readonly XmlNode_* _parent;

        public bool IsEmpty => _parent->FirstChild == null;

        public int Count => _parent->ChildCount;

        public XmlNode Parent => new XmlNode(_parent);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugDisplay => _parent != null ? $"{nameof(XmlNode)}[{Count}]" : $"{nameof(XmlNode)} (invalid instance)";

        bool ICollection<XmlNode>.IsReadOnly => true;

        internal XmlNodeList(XmlNode_* parent)
        {
            _parent = parent;
        }

        public XmlNode First()
        {
            if(Count == 0) { ThrowHelper.ThrowInvalidOperation("Sequence contains no elements."); }
            return new XmlNode(_parent->FirstChild);
        }

        public XmlNode First(Func<XmlNode, bool> predicate)
        {
            if(predicate is null) { ThrowHelper.ThrowNullArg(nameof(predicate)); }
            foreach(var node in this) {
                if(predicate!(node)) {
                    return node;
                }
            }
            throw new InvalidOperationException("Sequence contains no matching elements.");
        }

        public Enumerator GetEnumerator() => new Enumerator(_parent->FirstChild);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_parent->FirstChild);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_parent->FirstChild);

        void ICollection<XmlNode>.Add(XmlNode item) => throw new NotSupportedException();

        void ICollection<XmlNode>.Clear() => throw new NotSupportedException();

        bool ICollection<XmlNode>.Contains(XmlNode item) => throw new NotSupportedException();

        void ICollection<XmlNode>.CopyTo(XmlNode[] array, int arrayIndex) => throw new NotSupportedException();

        bool ICollection<XmlNode>.Remove(XmlNode item) => throw new NotSupportedException();

        private XmlNode[] ToArray()
        {
            // only for debugger
            if(_parent == null || IsEmpty) { return Array.Empty<XmlNode>(); }
            var array = new XmlNode[Count];
            var i = 0;
            foreach(var item in this) {
                array[i] = item;
                i++;
            }
            return array;
        }

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

        private sealed class XmlNodeListTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private XmlNodeList _entity;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public unsafe XmlNode[] Items => _entity.ToArray();

            public XmlNodeListTypeProxy(XmlNodeList entity)
            {
                _entity = entity;
            }
        }
    }
}
