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
    [DebuggerDisplay("{DebugView(),nq}")]
    public readonly unsafe struct XmlNode : IEquatable<XmlNode>, IReference
    {
        private readonly IntPtr _node;  // XmlNode_*

        private string DebugView()
        {
            var node = ((XmlNode_*)_node);
            return (node == null)
                ? ""
                : node->NodeType switch
                {
                    XmlNodeType.ElementNode => $"<{node->Name}>",
                    XmlNodeType.TextNode => node->InnerText.ToString(),
                    _ => "",
                };
        }

        /// <summary>Get node type</summary>
        public XmlNodeType NodeType => ((XmlNode_*)_node)->NodeType;

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
        public XmlNodeDescendantList Descendants => new XmlNodeDescendantList((XmlNode_*)_node, XmlNodeType.ElementNode);

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

        /// <summary>Get children of <see cref="XmlNodeType.ElementNode"/>. (It is same as <see cref="Children"/> property.)</summary>
        /// <returns>child nodes</returns>
        public TypedXmlNodeList GetChildren() => new TypedXmlNodeList((XmlNode_*)_node, XmlNodeType.ElementNode);

        /// <summary>Get children by specifying a node type.</summary>
        /// <param name="targetType">target xml node type. (If set null, all types of nodes are returned.)</param>
        /// <returns>child nodes</returns>
        public TypedXmlNodeList GetChildren(XmlNodeType? targetType) => new TypedXmlNodeList((XmlNode_*)_node, targetType);

        /// <summary>Get descendant nodes of <see cref="XmlNodeType.ElementNode"/>. (It is same as <see cref="Descendants"/> property.)</summary>
        /// <returns>descendant nodes</returns>
        public XmlNodeDescendantList GetDescendants() => new XmlNodeDescendantList((XmlNode_*)_node, XmlNodeType.ElementNode);

        /// <summary>Get descendant nodes by specifying a node type</summary>
        /// <param name="targetType">target xml node type. (If set null, all types of nodes are returned.)</param>
        /// <returns></returns>
        public XmlNodeDescendantList GetDescendants(XmlNodeType? targetType) => new XmlNodeDescendantList((XmlNode_*)_node, targetType);

        /// <summary>Get the string that this node represents as <see cref="RawString"/>.</summary>
        /// <remarks>The indent of the node is ignored at the head.</remarks>
        /// <returns><see cref="RawString"/> this node represents</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => ((XmlNode_*)_node)->AsRawString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetParent(out XmlNode parent) => Parent.TryGetValue(out parent);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetFirstChild(out XmlNode firstChild) => FirstChild.TryGetValue(out firstChild);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetLastChild(out XmlNode lastChild) => LastChild.TryGetValue(out lastChild);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryNextSibling(out XmlNode nextSibling) => NextSibling.TryGetValue(out nextSibling);

        public bool IsName(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(namespaceName.IsEmpty || name.IsEmpty) {
                return false;
            }
            if(XmlnsHelper.TryResolveNamespaceAlias(namespaceName, this, out var alias) == false) {
                return false;
            }
            if(alias.IsEmpty) {
                return Name == name;
            }
            else {
                var nodeName = Name;
                return (nodeName.Length == alias.Length + 1 + name.Length)
                    && nodeName.StartsWith(alias)
                    && nodeName.At(alias.Length) == (byte)':'
                    && nodeName.EndsWith(name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(ReadOnlySpan<byte> namespaceName, RawString name) => IsName(namespaceName, name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(RawString namespaceName, ReadOnlySpan<byte> name) => IsName(namespaceName.AsSpan(), name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(RawString namespaceName, RawString name) => IsName(namespaceName.AsSpan(), name.AsSpan());

        [SkipLocalsInit]
        public bool IsName(ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
        {
            if(namespaceName.IsEmpty || name.IsEmpty) {
                return false;
            }

            var utf8 = UTF8ExceptionFallbackEncoding.Instance;
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
                return IsName(nsNameUtf8, nameUtf8);
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
                        return IsName(nsNameUtf8, nameUtf8);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(ReadOnlySpan<char> namespaceName, string name) => IsName(namespaceName, name.AsSpan());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(string namespaceName, ReadOnlySpan<char> name) => IsName(namespaceName.AsSpan(), name);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsName(string namespaceName, string name) => IsName(namespaceName.AsSpan(), name.AsSpan());

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
            if(name.IsEmpty) {
                return Option<XmlNode>.Null;
            }
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

        /// <summary>Get full name of the node. Returns false if the full name could not be resolved.</summary>
        /// <param name="namespaceName">
        /// namespace name of the node<para/>
        /// ex) "abcde" in the case the node is &lt;a:foo xmlns:a="abcde" /&gt;<para/>
        /// </param>
        /// <param name="name">
        /// local name of the node<para/>
        /// ex) "foo" in the case the node is &lt;a:foo xmlns:a="abcde" /&gt;<para/>
        /// </param>
        /// <returns>Whether the full name of the node could be resolved</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetFullName(out RawString namespaceName, out RawString name) => XmlnsHelper.TryGetNodeFullName(this, out namespaceName, out name);

        /// <summary>Get full name of the node. The method throws <see cref="InvalidOperationException"/> if the full name could not resolved.</summary>
        /// <remarks>
        /// ex) Returns ("abcde", "foo") in the case the node is &lt;a:foo xmlns:a="abcde" /&gt;<para/>
        /// </remarks>
        /// <exception cref="InvalidOperationException">the full name could not resolved</exception>
        /// <returns>A pair of namespace name and local name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (RawString NamespaceName, RawString Name) GetFullName()
        {
            if(XmlnsHelper.TryGetNodeFullName(this, out var namespaceName, out var name) == false) {
                ThrowNoNamespace();
                static void ThrowNoNamespace() => throw new InvalidOperationException("Could not resolve the full name of the node.");
            }
            return (namespaceName, name);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is XmlNode node && Equals(node);

        /// <summary>Returns whether the value is same as the specified instance.</summary>
        /// <param name="other">an instance to check</param>
        /// <returns>equal or not</returns>s
        public bool Equals(XmlNode other) => _node == other._node;

        /// <inheritdoc/>
        public override int GetHashCode() => _node.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => _node != IntPtr.Zero ? ((XmlNode_*)_node)->ToString() : "";

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
        public readonly byte* NodeStrPtr;
        public int NodeStrLength;

        public XmlNode_* Parent;
        public XmlNode_* FirstChild;
        public XmlNode_* LastChild;
        public XmlNode_* Sibling;
        public int ChildCount;
        public int ChildElementCount;
        public int ChildTextCount => ChildCount - ChildElementCount;

        public int AttrIndex;
        public int AttrCount;
        public readonly CustomList<XmlAttribute_> WholeAttrs;

        public bool HasXmlNamespaceAttr;

        public XmlNodeType NodeType => Name.IsEmpty ? XmlNodeType.TextNode : XmlNodeType.ElementNode;

        public readonly CustomList<XmlNode_> WholeNodes
        {
            // See the comment in the constructor to know what the following means.
            get => Unsafe.As<IntPtr, CustomList<XmlNode_>>(ref Unsafe.AsRef(_wholeNodes));
        }

        public bool HasAttribute => AttrCount > 0;

        public bool HasChildren => ChildElementCount > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private XmlNode_(CustomList<XmlNode_> wholeNodes, int nodeIndex, int depth, RawString name, byte* nodeStrPtr, CustomList<XmlAttribute_> wholeAttrs)
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
            ChildElementCount = 0;
            AttrIndex = 0;
            AttrCount = 0;
            WholeAttrs = wholeAttrs;
            NodeStrPtr = nodeStrPtr;
            NodeStrLength = 0;
            HasXmlNamespaceAttr = false;
        }

        internal static XmlNode_ CreateElementNode(CustomList<XmlNode_> wholeNodes, int nodeIndex, int depth, RawString name, byte* nodeStrPtr, CustomList<XmlAttribute_> wholeAttrs)
        {
            return new XmlNode_(wholeNodes, nodeIndex, depth, name, nodeStrPtr, wholeAttrs);
        }

        internal static XmlNode_ CreateTextNode(CustomList<XmlNode_> wholeNodes, int nodeIndex, int depth, byte* nodeStrPtr, CustomList<XmlAttribute_> wholeAttrs)
        {
            return new XmlNode_(wholeNodes, nodeIndex, depth, RawString.Empty, nodeStrPtr, wholeAttrs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => new RawString(NodeStrPtr, NodeStrLength);

        public override string ToString()
        {
            return NodeType switch
            {
                XmlNodeType.ElementNode => Name.ToString(),
                XmlNodeType.TextNode => InnerText.ToString(),
                _ => "",
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddChildElementNode(XmlNode_* parent, XmlNode_* child)
        {
            Debug.Assert(child != null);
            Debug.Assert(child->NodeType == XmlNodeType.ElementNode);
            if(parent->FirstChild == null) {
                parent->FirstChild = child;
            }
            else {
                parent->LastChild->Sibling = child;
            }
            parent->LastChild = child;
            parent->ChildCount++;
            parent->ChildElementCount++;
            child->Parent = parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddChildTextNode(XmlNode_* parent, XmlNode_* child)
        {
            Debug.Assert(child != null);
            Debug.Assert(child->NodeType == XmlNodeType.TextNode);
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

    /// <summary>Type of xml node</summary>
    public enum XmlNodeType : byte
    {
        /// <summary>Element node</summary>
        ElementNode = 0,
        /// <summary>Text node</summary>
        TextNode = 1,
    }
}
