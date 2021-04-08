#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Provides list of <see cref="XmlAttribute"/></summary>
    public unsafe readonly struct XmlAttributeList : IEnumerable<XmlAttribute>
    {
        private readonly IntPtr _ptr;      // XmlAttribute*

        /// <summary>Get count of attributes</summary>
        public int Count { get; }

        /// <summary>Get an attribute with specified name</summary>
        /// <param name="name">name of the attribute</param>
        /// <exception cref="ArgumentException">No attributes of the name.</exception>
        /// <returns>an attribute</returns>
        public ref readonly XmlAttribute this[ReadOnlySpan<byte> name]
        {
            get
            {
                for(int i = 0; i < Count; i++) {
                    if(((XmlAttribute*)_ptr)->Name.AsSpan().SequenceEqual(name)) {
                        return ref Unsafe.AsRef<XmlAttribute>((XmlAttribute*)_ptr);
                    }
                }
                throw new ArgumentException($"No attributes of name '{name.Utf8ToString()}'.");
            }
        }

        /// <summary>Get an attribute with specified index</summary>
        /// <param name="index">index of the attribute</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <returns>an attribute</returns>
        public ref readonly XmlAttribute this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if((uint)index >= (uint)Count) { ThrowHelper.ThrowArgOutOfRange(nameof(index)); }
                return ref ((XmlAttribute*)_ptr)[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlAttributeList(IntPtr attrs, int count)
        {
            _ptr = attrs;
            Count = count;
        }

        /// <summary>Get enumerator of the list</summary>
        /// <returns>enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator((XmlAttribute*)_ptr, Count);

        IEnumerator<XmlAttribute> IEnumerable<XmlAttribute>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public struct Enumerator : IEnumerator<XmlAttribute>
        {
            private readonly XmlAttribute* _ptr;
            private readonly int _count;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(XmlAttribute* ptr, int count)
            {
                _ptr = ptr;
                _count = count;
                _index = -1;
            }

            public XmlAttribute Current => _ptr[_index];

            object IEnumerator.Current => _ptr[_index];

            public void Dispose() { }   // nop

            public bool MoveNext()
            {
                _index++;
                return _index < _count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}
