#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    public unsafe readonly struct XmlNodeList : IEnumerable<XmlNode>
    {
        private readonly IntPtr _firstChild;

        public static XmlNodeList Empty => default;

        public bool IsEmpty => _firstChild == IntPtr.Zero;

        internal XmlNodeList(IntPtr firstChild)
        {
            _firstChild = firstChild;
        }

        public Enumerator GetEnumerator() => new Enumerator(_firstChild);

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<XmlNode>
        {
            private IntPtr _current;    // XmlNode_*
            private IntPtr _next;       // XmlNode_*

            internal Enumerator(IntPtr firstChild)
            {
                _next = firstChild;
                _current = default;
            }

            public XmlNode Current => new XmlNode((XmlNode_*)_current);

            object IEnumerator.Current => Current;

            public void Dispose() { }   // nop

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if(_next == IntPtr.Zero) { return false; }
                _current = _next;
                _next = ((XmlNode_*)_next)->Sibling;
                return true;
            }

            public void Reset() => throw new NotSupportedException("Reset() is not supported.");
        }
    }
}
