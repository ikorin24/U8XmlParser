#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Provides list of <see cref="XmlAttribute"/></summary>
    [DebuggerDisplay("XmlAttribute[{_length}]")]
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

        void ICollection<XmlAttribute>.Add(XmlAttribute item) => throw new NotSupportedException();

        void ICollection<XmlAttribute>.Clear() => throw new NotSupportedException();

        bool ICollection<XmlAttribute>.Contains(XmlAttribute item) => throw new NotImplementedException();          // TODO:

        void ICollection<XmlAttribute>.CopyTo(XmlAttribute[] array, int arrayIndex) => throw new NotImplementedException();     // TODO:

        bool ICollection<XmlAttribute>.Remove(XmlAttribute item) => throw new NotSupportedException();

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
    }
}
