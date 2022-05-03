#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(XmlNodeDescendantListDebuggerTypeProxy))]
    public unsafe readonly struct XmlNodeDescendantList : IEnumerable<XmlNode>
    {
        private readonly XmlNode_* _parent;
        private readonly XmlNodeType? _targetType;

        internal XmlNodeDescendantList(XmlNode_* parent, XmlNodeType? targetType)
        {
            Debug.Assert(parent != null);
            _parent = parent;
            _targetType = targetType;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                int count = 0;
                foreach(var _ in this) {
                    count++;
                }
                return $"XmlNode[{count}]";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(_parent, _targetType);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => new EnumeratorClass(_parent, _targetType);

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_parent, _targetType);

        public struct Enumerator : IEnumerator<XmlNode>
        {
            private CustomList<XmlNode_>.Enumerator _e;     // Don't make it readonly.
            private readonly int _depth;
            private readonly XmlNodeType _targetType;
            private readonly bool _hasTargetType;

            public XmlNode Current => new XmlNode(_e.Current);

            object IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(XmlNode_* parent, XmlNodeType? targetType)
            {
                (_hasTargetType, _targetType) = targetType.HasValue switch
                {
                    true => (true, targetType.Value),
                    false => (false, default),
                };

                var firstChild = parent->FirstChild;
                if(firstChild == null) {
                    // default instance is valid.
                    _e = default;
                    _depth = default;
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
            MoveNext:
                if(_e.MoveNext() == false) {
                    return false;
                }
                if(_e.Current->Depth <= _depth) {
                    return false;
                }
                if(_hasTargetType == false || _e.Current->NodeType == _targetType) {
                    return true;
                }
                goto MoveNext;
            }

            public void Reset() => _e.Reset();
        }

        private sealed class EnumeratorClass : IEnumerator<XmlNode>
        {
            private Enumerator _e;  // Don't make it readonly.

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EnumeratorClass(XmlNode_* parent, XmlNodeType? targetType)
            {
                _e = new Enumerator(parent, targetType);
            }

            public XmlNode Current => _e.Current;

            object IEnumerator.Current => _e.Current;

            public void Dispose() => _e.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => _e.MoveNext();

            public void Reset() => _e.Reset();
        }

        internal sealed class XmlNodeDescendantListDebuggerTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly XmlNodeDescendantList _list;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public XmlNode[] Item => System.Linq.Enumerable.ToArray(_list);

            public XmlNodeDescendantListDebuggerTypeProxy(XmlNodeDescendantList list)
            {
                _list = list;
            }
        }
    }
}
