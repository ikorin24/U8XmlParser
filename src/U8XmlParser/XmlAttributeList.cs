#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Provides list of <see cref="XmlAttribute"/></summary>
    public unsafe readonly struct XmlAttributeList : IEnumerable<XmlAttribute>, ICollection<XmlAttribute>
    {
        private readonly CustomList<XmlAttribute> _list;
        private readonly int _start;
        private readonly int _length;

        /// <summary>Get count of attributes</summary>
        public int Count => _length;

        bool ICollection<XmlAttribute>.IsReadOnly => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlAttributeList(CustomList<XmlAttribute> list, int start, int length)
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_list.GetEnumerator(_start, _length));
        }

        IEnumerator<XmlAttribute> IEnumerable<XmlAttribute>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<XmlAttribute>
        {
            private CustomList<XmlAttribute>.Enumerator<XmlAttribute> _enumerator;

            public XmlAttribute Current => _enumerator.Current;

            object IEnumerator.Current => _enumerator.Current;

            internal Enumerator(in CustomList<XmlAttribute>.Enumerator<XmlAttribute> enumerator)
            {
                _enumerator = enumerator;
            }

            public void Dispose() => _enumerator.Dispose();

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}
