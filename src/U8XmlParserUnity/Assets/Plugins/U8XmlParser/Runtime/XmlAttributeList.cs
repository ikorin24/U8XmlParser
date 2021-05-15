#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections;
using U8Xml.Internal;
using System.Runtime.InteropServices;

namespace U8Xml
{
    /// <summary>Provides list of <see cref="XmlAttribute"/></summary>
    [DebuggerDisplay("XmlAttribute[{_length}]")]
    [DebuggerTypeProxy(typeof(XmlAttributeListTypeProxy))]
    public unsafe readonly struct XmlAttributeList : IEnumerable<XmlAttribute>, ICollection<XmlAttribute>
    {
        private readonly CustomList<XmlAttribute_> _list;
        private readonly int _start;
        private readonly int _length;

        /// <summary>Get count of attributes</summary>
        public int Count => _length;

        bool ICollection<XmlAttribute>.IsReadOnly => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlAttributeList(CustomList<XmlAttribute_> list, int start, int length)
        {
            _list = list;
            _start = start;
            _length = length;
        }

        public XmlAttribute First()
        {
            if(Count == 0) { ThrowHelper.ThrowInvalidOperation("Sequence contains no elements."); }
            return new XmlAttribute(_list.At(_start));
        }

        public XmlAttribute First(Func<XmlAttribute, bool> predicate)
        {
            if(FirstOrDefault(predicate).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return attr;
        }

        public Option<XmlAttribute> FirstOrDefault()
        {
            return Count == 0 ? default : new XmlAttribute(_list.At(_start));
        }

        public Option<XmlAttribute> FirstOrDefault(Func<XmlAttribute, bool> predicate)
        {
            if(predicate is null) { ThrowHelper.ThrowNullArg(nameof(predicate)); }
            foreach(var attr in this) {
                if(predicate!(attr)) {
                    return attr;
                }
            }
            return default;
        }

        void ICollection<XmlAttribute>.Add(XmlAttribute item) => throw new NotSupportedException();

        void ICollection<XmlAttribute>.Clear() => throw new NotSupportedException();

        bool ICollection<XmlAttribute>.Contains(XmlAttribute item) => throw new NotSupportedException();

        void ICollection<XmlAttribute>.CopyTo(XmlAttribute[] array, int arrayIndex) => throw new NotSupportedException();

        bool ICollection<XmlAttribute>.Remove(XmlAttribute item) => throw new NotSupportedException();

        internal void CopyTo(Span<XmlAttribute> span)
        {
            // Only for debugger

            var dest = MemoryMarshal.Cast<XmlAttribute, IntPtr>(span);
            fixed(IntPtr* buf = dest) {
                _list.CopyItemsPointer((XmlAttribute_**)buf, dest.Length, _start, _length);
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(_list.GetEnumerator(_start, _length));

        IEnumerator<XmlAttribute> IEnumerable<XmlAttribute>.GetEnumerator() => new EnumeratorClass(_list.GetEnumerator(_start, _length));

        IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(_list.GetEnumerator(_start, _length));

        public struct Enumerator : IEnumerator<XmlAttribute>
        {
            private CustomList<XmlAttribute_>.Enumerator _enumerator;       // mutable object, don't make it readonly

            public XmlAttribute Current => new XmlAttribute(_enumerator.Current);

            object IEnumerator.Current => *_enumerator.Current;

            internal Enumerator(in CustomList<XmlAttribute_>.Enumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }

        private sealed class EnumeratorClass : IEnumerator<XmlAttribute>
        {
            private CustomList<XmlAttribute_>.Enumerator _enumerator;       // mutable object, don't make it readonly

            public XmlAttribute Current => new XmlAttribute(_enumerator.Current);

            object IEnumerator.Current => *_enumerator.Current;

            internal EnumeratorClass(in CustomList<XmlAttribute_>.Enumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }

        private class XmlAttributeListTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private XmlAttributeList _entity;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public XmlAttribute[] Items
            {
                get
                {
                    var array = new XmlAttribute[_entity.Count];
                    _entity.CopyTo(array);
                    return array;
                }
            }

            public XmlAttributeListTypeProxy(XmlAttributeList entity)
            {
                _entity = entity;
            }
        }
    }
}
