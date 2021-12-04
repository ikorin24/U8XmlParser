#nullable enable
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>A xml node type.</summary>
    [DebuggerDisplay("<{ToString(),nq}>")]
    public readonly unsafe struct XmlNode : IEquatable<XmlNode>, IReference
    {
        private readonly IntPtr _node;  // XmlNode_*

        /// <summary>Get whether the node is null. (Valid nodes always return false.)</summary>
        public bool IsNull => _node == IntPtr.Zero;

        /// <summary>Get name of the node.</summary>
        public RawString Name => ((XmlNode_*)_node)->Name;

        /// <summary>Get an inner text of the node.</summary>
        public RawString InnerText => ((XmlNode_*)_node)->InnerText;

        /// <summary>Get whether the node has any attribute.</summary>
        public bool HasAttribute => ((XmlNode_*)_node)->HasAttribute;

        /// <summary>Get attributes of the node.</summary>
        public XmlAttributeList Attributes => new XmlAttributeList((XmlNode_*)_node);

        /// <summary>Get whether the node has any children.</summary>
        public bool HasChildren => ((XmlNode_*)_node)->HasChildren;

        /// <summary>Get children of the node.</summary>
        public XmlNodeList Children => new XmlNodeList((XmlNode_*)_node);

        /// <summary>Get descendant nodes in the way of depth-first search.</summary>
        public XmlNodeDescendantList Descendants => new XmlNodeDescendantList((XmlNode_*)_node);

        /// <summary>Get depth of the node in xml. (The root node is 0.)</summary>
        public int Depth => ((XmlNode_*)_node)->Depth;

        /// <summary>Get whether the node is root.</summary>
        public bool IsRoot => ((XmlNode_*)_node)->Parent == null;

        /// <summary>Get a parent node of the node.</summary>
        public Option<XmlNode> Parent => new XmlNode(((XmlNode_*)_node)->Parent);

        /// <summary>Get the first child node.</summary>
        public Option<XmlNode> FirstChild => new XmlNode(((XmlNode_*)_node)->FirstChild);

        /// <summary>Get the last child of the node.</summary>
        public Option<XmlNode> LastChild => new XmlNode(((XmlNode_*)_node)->LastChild);

        /// <summary>Get the next sibling of the node.</summary>
        public Option<XmlNode> NextSibling => new XmlNode(((XmlNode_*)_node)->Sibling);

        internal bool HasXmlNamespaceAttr => ((XmlNode_*)_node)->HasXmlNamespaceAttr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlNode(XmlNode_* node) => _node = (IntPtr)node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetParent(out XmlNode parent) => Parent.TryGetValue(out parent);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetFirstChild(out XmlNode firstChild) => FirstChild.TryGetValue(out firstChild);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetLastChild(out XmlNode lastChild) => LastChild.TryGetValue(out lastChild);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryNextSibling(out XmlNode nextSibling) => NextSibling.TryGetValue(out nextSibling);

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(RawString name) => FindChildOrDefault(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> name)
        {
            foreach(var child in Children) {
                if(child.Name == name) {
                    return child;
                }
            }
            return Option<XmlNode>.Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(RawString namespaceName, RawString name) => Children.FindOrDefault(namespaceName.AsSpan(), name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, RawString name) => Children.FindOrDefault(namespaceName, name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(RawString namespaceName, ReadOnlySpan<byte> name) => Children.FindOrDefault(namespaceName.AsSpan(), name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) => Children.FindOrDefault(namespaceName, name);

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(string name) => Children.FindOrDefault(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> name) => Children.FindOrDefault(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(string namespaceName, string name) => Children.FindOrDefault(namespaceName.AsSpan(), name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, string name) => Children.FindOrDefault(namespaceName, name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(string namespaceName, ReadOnlySpan<char> name) => Children.FindOrDefault(namespaceName.AsSpan(), name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) => Children.FindOrDefault(namespaceName, name);

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(RawString name) => FindChild(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<byte> name)
        {
            if(FindChildOrDefault(name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(RawString namespaceName, RawString name) => FindChild(namespaceName.AsSpan(), name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<byte> namespaceName, RawString name) => FindChild(namespaceName, name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(RawString namespaceName, ReadOnlySpan<byte> name) => FindChild(namespaceName.AsSpan(), name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(FindChildOrDefault(namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(string name) => FindChild(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<char> name)
        {
            if(FindChildOrDefault(name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(string namespaceName, string name) => FindChild(namespaceName.AsSpan(), name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<char> namespaceName, string name) => FindChild(namespaceName, name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(string namespaceName, ReadOnlySpan<char> name) => FindChild(namespaceName.AsSpan(), name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlNode FindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
        {
            if(FindChildOrDefault(namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(RawString name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(RawString namespaceName, RawString name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<byte> namespaceName, RawString name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(RawString namespaceName, ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(string name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(string namespaceName, string name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<char> namespaceName, string name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(string namespaceName, ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(RawString name) => Attributes.FindOrDefault(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> name) => Attributes.FindOrDefault(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(string name) => Attributes.FindOrDefault(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> name) => Attributes.FindOrDefault(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> namespaceName, RawString name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(RawString namespaceName, ReadOnlySpan<byte> name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(RawString namespaceName, RawString name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> namespaceName, string name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(string namespaceName, ReadOnlySpan<char> name) => Attributes.FindOrDefault(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<XmlAttribute> FindAttributeOrDefault(string namespaceName, string name) => Attributes.FindOrDefault(namespaceName, name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(RawString name) => Attributes.Find(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<byte> name) => Attributes.Find(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(string name) => Attributes.Find(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<char> name) => Attributes.Find(name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<byte> namespaceName, RawString name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(RawString namespaceName, ReadOnlySpan<byte> name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(RawString namespaceName, RawString name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(ReadOnlySpan<char> namespaceName, string name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(string namespaceName, ReadOnlySpan<char> name) => Attributes.Find(namespaceName, name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute FindAttribute(string namespaceName, string name) => Attributes.Find(namespaceName, name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(RawString name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<byte> name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(string name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<char> name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<byte> namespaceName, RawString name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(RawString namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(RawString namespaceName, RawString name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(ReadOnlySpan<char> namespaceName, string name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(string namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindAttribute(string namespaceName, string name, out XmlAttribute attribute) => Attributes.TryFind(namespaceName, name, out attribute);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is XmlNode node && Equals(node);

        /// <summary>Returns whether the value is same as the specified instance.</summary>
        /// <param name="other">an instance to check</param>
        /// <returns>equal or not</returns>s
        public bool Equals(XmlNode other) => _node == other._node;

        /// <inheritdoc/>
        public override int GetHashCode() => _node.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => _node != IntPtr.Zero ? ((XmlNode_*)_node)->Name.ToString() : "";

        /// <summary>Returns true if both <see cref="XmlNode"/>s are same objects.</summary>
        /// <param name="left">left operand</param>
        /// <param name="right">right operand</param>
        /// <returns>true if both <see cref="XmlNode"/>s are same objects</returns>
        public static bool operator ==(XmlNode left, XmlNode right) => left.Equals(right);

        /// <summary>Returns true if both <see cref="XmlNode"/>s are not same objects.</summary>
        /// <param name="left">left operand</param>
        /// <param name="right">right operand</param>
        /// <returns>true if both <see cref="XmlNode"/>s are not same objects</returns>
        public static bool operator !=(XmlNode left, XmlNode right) => !(left == right);
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal unsafe struct XmlNode_
    {
        private readonly IntPtr _wholeNodes;    // Its type is CustomList<XmlNode_>, but the field is IntPtr. *** See the comment in the constructor ***
        public readonly int NodeIndex;
        public readonly int Depth;
        public readonly RawString Name;
        public RawString InnerText;

        public XmlNode_* Parent;
        public XmlNode_* FirstChild;
        public XmlNode_* LastChild;
        public XmlNode_* Sibling;
        public int ChildCount;

        public int AttrIndex;
        public int AttrCount;
        public readonly CustomList<XmlAttribute_> WholeAttrs;

        public bool HasXmlNamespaceAttr;

        public readonly CustomList<XmlNode_> WholeNodes
        {
            // See the comment in the constructor to know what the following means.
            get => Unsafe.As<IntPtr, CustomList<XmlNode_>>(ref Unsafe.AsRef(_wholeNodes));
        }

        public bool HasAttribute => AttrCount > 0;

        public bool HasChildren => FirstChild != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlNode_(CustomList<XmlNode_> wholeNodes, int nodeIndex, int depth, RawString name, CustomList<XmlAttribute_> wholeAttrs)
        {
            // [NOTE]
            // _wholeNodes is CustomList<XmlNode_>,
            // but XmlNode_ cannot have any fields of type CustomList<XmlNode_> because of the bug of the dotnet runtime.
            // CustomList<XmlNode_> has same memory layout as IntPtr.
            // So XmlNode_ has 'wholeNodes' as IntPtr.

            Debug.Assert(sizeof(CustomList<XmlNode_>) == sizeof(IntPtr));
            _wholeNodes = Unsafe.As<CustomList<XmlNode_>, IntPtr>(ref wholeNodes);

            NodeIndex = nodeIndex;
            Depth = depth;
            Name = name;
            InnerText = RawString.Empty;
            Parent = null;
            FirstChild = null;
            LastChild = null;
            Sibling = null;
            ChildCount = 0;
            AttrIndex = 0;
            AttrCount = 0;
            WholeAttrs = wholeAttrs;
            HasXmlNamespaceAttr = false;
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
            child->Parent = parent;
        }
    }
}
