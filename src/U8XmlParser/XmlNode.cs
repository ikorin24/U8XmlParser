#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly unsafe struct XmlNode : IEquatable<XmlNode>
    {
        private readonly IntPtr _node;

        public RawString Name => ((XmlNode_*)_node)->Name;
        public RawString InnerText => ((XmlNode_*)_node)->InnerText;
        public bool HasAttribute => ((XmlNode_*)_node)->HasAttribute;
        public XmlAttributeList Attributes => ((XmlNode_*)_node)->Attributes;
        public bool HasChildren => ((XmlNode_*)_node)->HasChildren;
        public XmlNodeList Children => ((XmlNode_*)_node)->Children;

        internal XmlNode(XmlNode_* node) => _node = (IntPtr)node;

        public override bool Equals(object? obj) => obj is XmlNode node && Equals(node);

        public bool Equals(XmlNode other) => _node == other._node;

        public override int GetHashCode() => _node.GetHashCode();

        public override string ToString() => _node != IntPtr.Zero ? ((XmlNode_*)_node)->Name.ToString() : "";
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal readonly unsafe struct XmlNode_
    {
        public readonly RawString Name;
        public readonly RawString InnerText;

        internal readonly IntPtr FirstChild;    // XmlNode_*
        internal readonly IntPtr LastChild;     // XmlNode_*
        internal readonly IntPtr Sibling;       // XmlNode_*

        internal readonly int AttrIndex;
        internal readonly int AttrCount;
        private readonly CustomList<XmlAttribute> _wholeAttrs;

        public bool HasAttribute => AttrCount > 0;

        public bool HasChildren => FirstChild != IntPtr.Zero;

        public XmlAttributeList Attributes => new XmlAttributeList(_wholeAttrs, AttrIndex, AttrCount);

        public XmlNodeList Children => new XmlNodeList(FirstChild);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlNode_(RawString name, CustomList<XmlAttribute> wholeAttrs)
        {
            Name = name;
            InnerText = RawString.Empty;
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


        internal static void AddChild(XmlNode_* parent, XmlNode_* child)
        {
            if(parent->HasChildren) {
                Unsafe.AsRef(((XmlNode_*)parent->LastChild)->Sibling) = (IntPtr)child;
            }
            else {
                Unsafe.AsRef(parent->FirstChild) = (IntPtr)child;
            }
            Unsafe.AsRef(parent->LastChild) = (IntPtr)child;
        }
    }
}
