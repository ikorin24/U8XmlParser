#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    [DebuggerDisplay("<{ToString(),nq}>")]
    public readonly unsafe struct XmlNode : IEquatable<XmlNode>
    {
        private readonly IntPtr _node;  // XmlNode_*

        public RawString Name => ((XmlNode_*)_node)->Name;
        public RawString InnerText => ((XmlNode_*)_node)->InnerText;
        public bool HasAttribute => ((XmlNode_*)_node)->HasAttribute;
        public XmlAttributeList Attributes => ((XmlNode_*)_node)->Attributes;
        public bool HasChildren => ((XmlNode_*)_node)->HasChildren;
        public XmlNodeList Children => new XmlNodeList((XmlNode_*)_node);

        internal XmlNode(XmlNode_* node) => _node = (IntPtr)node;

        public override bool Equals(object? obj) => obj is XmlNode node && Equals(node);

        public bool Equals(XmlNode other) => _node == other._node;

        public override int GetHashCode() => _node.GetHashCode();

        public override string ToString() => _node != IntPtr.Zero ? ((XmlNode_*)_node)->Name.ToString() : "";
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal unsafe struct XmlNode_
    {
        public readonly RawString Name;
        public RawString InnerText;

        public XmlNode_* FirstChild;    // XmlNode_*
        public XmlNode_* LastChild;     // XmlNode_*
        public XmlNode_* Sibling;       // XmlNode_*
        public int ChildCount;

        public int AttrIndex;
        public int AttrCount;
        private readonly CustomList<XmlAttribute_> _wholeAttrs;

        public bool HasAttribute => AttrCount > 0;

        public bool HasChildren => FirstChild != null;

        public XmlAttributeList Attributes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new XmlAttributeList(_wholeAttrs, AttrIndex, AttrCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlNode_(RawString name, CustomList<XmlAttribute_> wholeAttrs)
        {
            Name = name;
            InnerText = RawString.Empty;
            FirstChild = null;
            LastChild = null;
            Sibling = null;
            ChildCount = 0;
            AttrIndex = 0;
            AttrCount = 0;
            _wholeAttrs = wholeAttrs;
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddChild(XmlNode_* parent, XmlNode_* child)
        {
            if(parent->FirstChild == null) {
                parent->FirstChild = child;
            }
            else {
                parent->LastChild->Sibling = child;
            }
            parent->LastChild = child;
            parent->ChildCount++;
        }
    }
}
