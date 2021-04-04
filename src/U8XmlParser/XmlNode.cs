﻿#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly unsafe struct XmlNode
    {
        public readonly RawString Name;
        public readonly RawString Content;

        internal readonly IntPtr FirstChild;    // XmlNode*
        internal readonly IntPtr LastChild;     // XmlNode*
        internal readonly IntPtr Sibling;       // XmlNode*

        internal readonly int AttrIndex;
        internal readonly int AttrCount;
        private readonly CustomList<XmlAttribute> _wholeAttrs;

        public bool HasAttribute => AttrCount > 0;

        public bool HasChildren => FirstChild != IntPtr.Zero;

        //public XmlAttributeList Attributes => new XmlAttributeList(AttributesInternal, AttrCount);
        public XmlAttributeList Attributes => throw new NotImplementedException();

        public XmlNodeList Children => new XmlNodeList(FirstChild);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlNode(RawString name, CustomList<XmlAttribute> wholeAttrs)
        {
            Name = name;
            Content = RawString.Empty;
            FirstChild = IntPtr.Zero;
            LastChild = IntPtr.Zero;
            Sibling = IntPtr.Zero;
            AttrIndex = 0;
            AttrCount = 0;
            _wholeAttrs = wholeAttrs;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
