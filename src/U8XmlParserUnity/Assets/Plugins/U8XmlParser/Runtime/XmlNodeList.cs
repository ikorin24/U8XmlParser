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
    public unsafe readonly struct XmlNodeList : IEnumerable<XmlNode>, ICollection<XmlNode>, IReference
    {
        private readonly XmlNode_* _parent;

        internal Option<XmlNode> Parent => new XmlNode(_parent);

        public bool IsNull => _parent == null;

        public bool IsEmpty => _parent->FirstChild == null;
        //public bool IsEmpty => _parent->ChildElementCount == 0;

        public int Count => _parent->ChildCount;
        //public int Count => _parent->ChildElementCount;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebugDisplay => _parent != null ? $"{nameof(XmlNode)}[{Count}]" : $"{nameof(XmlNode)} (invalid instance)";

        bool ICollection<XmlNode>.IsReadOnly => true;

        internal XmlNodeList(XmlNode_* parent)
        {
            _parent = parent;

            // TODO: Make behaviour same as `new TypedXmlNodeList(parent, XmlNodeType.ElementNode)`
        }

        public XmlNode First()
        {
            if(Count == 0) { ThrowHelper.ThrowInvalidOperation("Sequence contains no elements."); }
            return new XmlNode(_parent->FirstChild);
        }

        public Option<XmlNode> FirstOrDefault()
        {
            return new XmlNode(_parent->FirstChild);
        }

        public XmlNode First(Func<XmlNode, bool> predicate)
        {
            if(FirstOrDefault(predicate).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        public Option<XmlNode> FirstOrDefault(Func<XmlNode, bool> predicate)
        {
            if(predicate is null) { ThrowHelper.ThrowNullArg(nameof(predicate)); }
            foreach(var node in this) {
                if(predicate!(node)) {
                    return node;
                }
            }
            return new Option<XmlNode>(default);
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

    [DebuggerTypeProxy(typeof(TypedXmlNodeListTypeProxy))]
    public unsafe readonly struct TypedXmlNodeList : IEnumerable<XmlNode>
    {
        private readonly XmlNode_* _parent;
        private readonly XmlNodeType? _targetType;

        internal TypedXmlNodeList(XmlNode_* parent, XmlNodeType? targetType)
        {
            _parent = parent;
            _targetType = targetType;
        }

        public Enumerator GetEnumerator() => new Enumerator(_parent, _targetType);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_parent, _targetType);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_parent, _targetType);

        public unsafe struct Enumerator : IEnumerator<XmlNode>
        {
            private XmlNode_* _current;
            private XmlNode_* _next;
            private readonly XmlNodeType _targetType;
            private readonly bool _hasTargetType;

            internal Enumerator(XmlNode_* parent, XmlNodeType? targetType)
            {
                _next = parent->FirstChild;
                _current = null;
                (_targetType, _hasTargetType) = targetType.HasValue switch
                {
                    true => (targetType.Value, true),
                    false => (default, false),
                };
            }

            public XmlNode Current => new XmlNode(_current);

            object IEnumerator.Current => Current;

            public void Dispose() { }   // nop

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
            MoveNext:
                if(_next == null) { return false; }
                _current = _next;
                _next = _next->Sibling;

                if(_hasTargetType == false || _current->NodeType == _targetType) {
                    return true;
                }

                goto MoveNext;
            }

            public void Reset() => throw new NotSupportedException("Reset() is not supported.");
        }

        private sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private Enumerator _enumerator;     // mutable object, don't make it readonly.

            public XmlNode Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EnumeratorClass(XmlNode_* parent, XmlNodeType? targetType)
            {
                _enumerator = new Enumerator(parent, targetType);
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }

        private sealed class TypedXmlNodeListTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TypedXmlNodeList _entity;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public unsafe XmlNode[] Items => System.Linq.Enumerable.ToArray(_entity);

            public TypedXmlNodeListTypeProxy(TypedXmlNodeList entity)
            {
                _entity = entity;
            }
        }
    }
}
