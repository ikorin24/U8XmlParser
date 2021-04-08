#nullable enable
using System;
using System.Collections.Generic;
using System.Collections;

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
            private IntPtr _current;    // XmlNode*
            private IntPtr _next;       // XmlNode*

            internal Enumerator(IntPtr firstChild)
            {
                _next = firstChild;
                _current = default;
            }

            public XmlNode Current => *(XmlNode*)_current;

            object IEnumerator.Current => Current;

            public void Dispose() { }   // nop

            public bool MoveNext()
            {
                if(_next == IntPtr.Zero) { return false; }
                _current = _next;
                _next = ((XmlNode*)_next)->Sibling;
                return true;
            }

            public void Reset() => throw new NotSupportedException("Reset() is not supported.");
        }
    }
}
