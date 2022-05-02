#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("XmlNode[{Count}]")]
    [DebuggerTypeProxy(typeof(AllNodeListDebuggerTypeProxy))]
    public readonly unsafe struct AllNodeList : IEnumerable<XmlNode>
    {
        private readonly CustomList<XmlNode_> _nodes;
        private readonly XmlNodeType? _targetType;

        public readonly int Count => _nodes.Count;  // TODO:

        internal AllNodeList(CustomList<XmlNode_> nodes, XmlNodeType? targetType)
        {
            _nodes = nodes;
            _targetType = targetType;
        }

        public XmlNode First()
        {
            if(FirstOrDefault().TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no elements.");
            }
            return node;
        }

        public Option<XmlNode> FirstOrDefault()
        {
            using var e = GetEnumerator();
            if(e.MoveNext() == false) {
                return Option<XmlNode>.Null;
            }
            return e.Current;
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
            return Option<XmlNode>.Null;
        }

        public Enumerator GetEnumerator() => new Enumerator(_nodes.GetEnumerator(), _targetType);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_nodes.GetEnumerator(), _targetType);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_nodes.GetEnumerator(), _targetType);


        public struct Enumerator : IEnumerator<XmlNode>
        {
            private CustomList<XmlNode_>.Enumerator _e;
            private readonly XmlNodeType _targetType;
            private readonly bool _hasTargetType;

            internal Enumerator(CustomList<XmlNode_>.Enumerator e, XmlNodeType? targetType)
            {
                _e = e;
                (_hasTargetType, _targetType) = targetType.HasValue switch
                {
                    true => (true, targetType.Value),
                    false => (false, default),
                };
            }

            public XmlNode Current => new XmlNode(_e.Current);

            object IEnumerator.Current => Current;

            public void Dispose() => _e.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
            MoveNext:
                if(_e.MoveNext() == false) {
                    return false;
                }
                if(_hasTargetType == false || _e.Current->NodeType == _targetType) {
                    return true;
                }
                goto MoveNext;
            }

            public void Reset() => _e.Reset();
        }

        internal sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private Enumerator _e;

            internal EnumeratorClass(CustomList<XmlNode_>.Enumerator e, XmlNodeType? targetType)
            {
                _e = new Enumerator(e, targetType);
            }

            public XmlNode Current => _e.Current;

            object IEnumerator.Current => Current;

            public void Dispose() => _e.Dispose();

            public bool MoveNext() => _e.MoveNext();

            public void Reset() => _e.Reset();
        }

        internal sealed class AllNodeListDebuggerTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly AllNodeList _list;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public XmlNode[] Item => System.Linq.Enumerable.ToArray(_list);

            public AllNodeListDebuggerTypeProxy(AllNodeList list)
            {
                _list = list;
            }
        }
    }
}
