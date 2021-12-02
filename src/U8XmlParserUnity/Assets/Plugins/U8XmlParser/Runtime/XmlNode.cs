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

        internal XmlNode(XmlNode_* node) => _node = (IntPtr)node;

        public bool TryGetParent(out XmlNode parent) => Parent.TryGetValue(out parent);
        public bool TryGetFirstChild(out XmlNode firstChild) => FirstChild.TryGetValue(out firstChild);
        public bool TryGetLastChild(out XmlNode lastChild) => LastChild.TryGetValue(out lastChild);
        public bool TryNextSibling(out XmlNode nextSibling) => NextSibling.TryGetValue(out nextSibling);

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
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

        public Option<XmlNode> FindChildOrDefault(RawString namespaceName, RawString name) => FindChildOrDefault(namespaceName.AsSpan(), name.AsSpan());

        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, RawString name) => FindChildOrDefault(namespaceName, name.AsSpan());

        public Option<XmlNode> FindChildOrDefault(RawString namespaceName, ReadOnlySpan<byte> name) => FindChildOrDefault(namespaceName.AsSpan(), name);

        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(TryGetNamespaceAlias(namespaceName, this, out var nsAlias) == false) {
                return Option<XmlNode>.Null;
            }
            if(nsAlias.IsEmpty) {
                return FindChildOrDefault(name);
            }
            var fullNameLength = nsAlias.Length + 1 + name.Length;
            foreach(var child in Children) {
                var childName = child.Name;
                if(childName.Length == fullNameLength && childName.StartsWith(nsAlias)
                                                      && childName.At(nsAlias.Length) == (byte)':'
                                                      && childName.Slice(nsAlias.Length + 1) == name) {

                    if(TryGetNamespaceAlias(namespaceName, child, out var nsAliasActual) == false) {
                        return child;
                    }
                    if(nsAliasActual == nsAlias) {
                        return child;
                    }
                }
            }
            return Option<XmlNode>.Null;
        }

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        public Option<XmlNode> FindChildOrDefault(string name) => FindChildOrDefault(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node as <see cref="Option{T}"/></returns>
        [SkipLocalsInit]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> name)
        {
            var utf8 = Encoding.UTF8;
            var byteLen = utf8.GetByteCount(name);

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = name) {
                    utf8.GetBytes(ptr, name.Length, buf, byteLen);
                }
                var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                return FindChildOrDefault(span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = name) {
                        utf8.GetBytes(ptr, name.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return FindChildOrDefault(span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        public Option<XmlNode> FindChildOrDefault(string namespaceName, string name) => FindChildOrDefault(namespaceName.AsSpan(), name.AsSpan());

        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, string name) => FindChildOrDefault(namespaceName, name.AsSpan());

        public Option<XmlNode> FindChildOrDefault(string namespaceName, ReadOnlySpan<char> name) => FindChildOrDefault(namespaceName.AsSpan(), name);

        [SkipLocalsInit]
        public Option<XmlNode> FindChildOrDefault(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
        {
            var utf8 = Encoding.UTF8;
            var nsNameByteLen = utf8.GetByteCount(namespaceName);
            var nameByteLen = utf8.GetByteCount(name);
            var byteLen = nsNameByteLen + nameByteLen;

            const int Threshold = 128;
            if(byteLen <= Threshold) {
                byte* buf = stackalloc byte[Threshold];
                fixed(char* ptr = namespaceName) {
                    utf8.GetBytes(ptr, namespaceName.Length, buf, nsNameByteLen);
                }
                var nsNameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf, nsNameByteLen);
                fixed(char* ptr = name) {
                    utf8.GetBytes(ptr, name.Length, buf + nsNameByteLen, nameByteLen);
                }
                var nameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf + nsNameByteLen, nameByteLen);
                return FindChildOrDefault(nsNameUtf8, nameUtf8);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = namespaceName)
                    fixed(char* ptr2 = name) {
                        utf8.GetBytes(ptr, namespaceName.Length, buf, nsNameByteLen);
                        var nsNameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf, nsNameByteLen);
                        utf8.GetBytes(ptr2, name.Length, buf + nsNameByteLen, nameByteLen);
                        var nameUtf8 = SpanHelper.CreateReadOnlySpan<byte>(buf + nsNameByteLen, nameByteLen);
                        return FindChildOrDefault(nsNameUtf8, nameUtf8);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        public XmlNode FindChild(RawString name) => FindChild(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        public XmlNode FindChild(ReadOnlySpan<byte> name)
        {
            if(FindChildOrDefault(name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        public XmlNode FindChild(RawString namespaceName, RawString name) => FindChild(namespaceName.AsSpan(), name.AsSpan());
        public XmlNode FindChild(ReadOnlySpan<byte> namespaceName, RawString name) => FindChild(namespaceName, name.AsSpan());
        public XmlNode FindChild(RawString namespaceName, ReadOnlySpan<byte> name) => FindChild(namespaceName.AsSpan(), name);
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
        public XmlNode FindChild(string name) => FindChild(name.AsSpan());

        /// <summary>Find a child by name. Returns the first child found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="name">child name to find</param>
        /// <returns>a found child node</returns>
        public XmlNode FindChild(ReadOnlySpan<char> name)
        {
            if(FindChildOrDefault(name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        public XmlNode FindChild(string namespaceName, string name) => FindChild(namespaceName.AsSpan(), name.AsSpan());
        public XmlNode FindChild(ReadOnlySpan<char> namespaceName, string name) => FindChild(namespaceName, name.AsSpan());
        public XmlNode FindChild(string namespaceName, ReadOnlySpan<char> name) => FindChild(namespaceName.AsSpan(), name);
        public XmlNode FindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
        {
            if(FindChildOrDefault(namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation("Sequence contains no matching elements.");
            }
            return node;
        }

        public bool TryFindChild(ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        public bool TryFindChild(RawString name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        public bool TryFindChild(RawString namespaceName, RawString name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(ReadOnlySpan<byte> namespaceName, RawString name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(RawString namespaceName, ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        public bool TryFindChild(string name, out XmlNode node) => FindChildOrDefault(name).TryGetValue(out node);
        public bool TryFindChild(string namespaceName, string name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(ReadOnlySpan<char> namespaceName, string name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(string namespaceName, ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);
        public bool TryFindChild(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlNode node) => FindChildOrDefault(namespaceName, name).TryGetValue(out node);

        public Option<XmlAttribute> FindAttributeOrDefault(RawString name) => Attributes.FindOrDefault(name);
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<byte> name) => Attributes.FindOrDefault(name);
        public Option<XmlAttribute> FindAttributeOrDefault(string name) => Attributes.FindOrDefault(name);
        public Option<XmlAttribute> FindAttributeOrDefault(ReadOnlySpan<char> name) => Attributes.FindOrDefault(name);

        public XmlAttribute FindAttribute(RawString name) => Attributes.Find(name);
        public XmlAttribute FindAttribute(ReadOnlySpan<byte> name) => Attributes.Find(name);
        public XmlAttribute FindAttribute(string name) => Attributes.Find(name);
        public XmlAttribute FindAttribute(ReadOnlySpan<char> name) => Attributes.Find(name);

        public bool TryFindAttribute(RawString name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        public bool TryFindAttribute(ReadOnlySpan<byte> name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        public bool TryFindAttribute(string name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);
        public bool TryFindAttribute(ReadOnlySpan<char> name, out XmlAttribute attribute) => Attributes.TryFind(name, out attribute);

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

        internal static bool TryGetNamespaceAlias(ReadOnlySpan<byte> nsName, XmlNode node, out RawString alias)
        {
            const uint xmln = (byte)'x' + ((byte)'m' << 8) + ((byte)'l' << 16) + ((byte)'n' << 24);

            var n = node;
            while(true) {
                if(n.HasXmlNamespaceAttr) {
                    foreach(var attr in n.Attributes) {
                        var attrName = attr.Name;
                        if((attrName.Length >= 5) && (*(uint*)attrName.GetPtr() == xmln)
                                                  && (attrName.At(4) == (byte)'s')) {
                            if(attrName.Length == 5 && attr.Value == nsName) {
                                alias = RawString.Empty;
                                return true;
                            }
                            else if(attrName.Length >= 7 && attrName[5] == (byte)':' && attr.Value == nsName) {
                                alias = attrName.Slice(6);
                                return true;
                            }
                        }
                    }
                }
                if(n.TryGetParent(out n) == false) {
                    alias = RawString.Empty;
                    return false;
                }
            }
        }
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
