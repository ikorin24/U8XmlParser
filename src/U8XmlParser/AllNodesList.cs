#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using U8Xml.Internal;

namespace U8Xml
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AllNodesList : IEnumerable<XmlNode>
    {
        private readonly CustomList<XmlNode> _list;

        internal AllNodesList(CustomList<XmlNode> list)
        {
            _list = list;
        }

        public CustomListEnumerator<XmlNode> GetEnumerator()
        {
            if (_list.IsDisposed)
            {
                ThrowHelper.ThrowDisposed("Xml Object");
            }
            return _list.GetEnumerator();
        }

        IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
