#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Text;
using U8Xml.Internal;
using System.Buffers;

namespace U8Xml
{
    /// <summary>Attribute of node in xml</summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public unsafe readonly struct XmlAttribute : IEquatable<XmlAttribute>, IReference
    {
        // Don't add any other fields. The layout must be same as IntPtr.
        private readonly IntPtr _attr;  // XmlAttribute_*

        public bool IsNull => _attr == IntPtr.Zero;

        /// <summary>Get attribute name</summary>
        public ref readonly RawString Name => ref ((XmlAttribute_*)_attr)->Name;
        /// <summary>Get attribute value</summary>
        public ref readonly RawString Value => ref ((XmlAttribute_*)_attr)->Value;

        internal Option<XmlNode> Node => ((XmlAttribute_*)_attr)->Node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal XmlAttribute(XmlAttribute_* attr) => _attr = (IntPtr)attr;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out RawString name, out RawString value)
        {
            name = Name;
            value = Value;
        }

        public bool IsName(ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(namespaceName.IsEmpty || name.IsEmpty) {
                return false;
            }
            if(Node.TryGetValue(out var node) == false) {
                return false;
            }
            if(XmlnsHelper.TryResolveNamespaceAlias(namespaceName, node, out var alias) == false) {
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

        /// <summary>Get full name of the attribute. Returns false if the full name could not be resolved.</summary>
        /// <param name="namespaceName">
        /// namespace name of the attribute<para/>
        /// ex) "abcde" in the case the attribute is a:bar="123" in &lt;node xmlns:a="abcde" a:bar="123" /&gt;<para/>
        /// </param>
        /// <param name="name">
        /// local name of the attribute<para/>
        /// ex) "bar" in the case the attribute is a:bar="123" in &lt;node xmlns:a="abcde" a:bar="123" /&gt;<para/>
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetFullName(out RawString namespaceName, out RawString name) => XmlnsHelper.TryGetAttributeFullName(this, out namespaceName, out name);

        /// <summary>Get full name of the attribute. The method throws <see cref="InvalidOperationException"/> if the full name could not resolved.</summary>
        /// <remarks>
        /// ex) Returns ("abcde", "bar") in the case the attribute is a:bar="123" in &lt;node xmlns:a="abcde" a:bar="123" /&gt;<para/>
        /// </remarks>
        /// <exception cref="InvalidOperationException">the full name could not resolved</exception>
        /// <returns>A pair of namespace name and local name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (RawString NamespaceName, RawString Name) GetFullName()
        {
            if(XmlnsHelper.TryGetAttributeFullName(this, out var namespaceName, out var name) == false) {
                ThrowNoNamespace();
                static void ThrowNoNamespace() => throw new InvalidOperationException("Could not resolve the full name of the node.");
            }
            return (namespaceName, name);
        }

        public override bool Equals(object? obj) => obj is XmlAttribute attribute && Equals(attribute);

        public bool Equals(XmlAttribute other) => _attr == other._attr;

        public override int GetHashCode() => _attr.GetHashCode();

        public override string ToString() => _attr == IntPtr.Zero ? "" : ((XmlAttribute_*)_attr)->ToString();

        public static implicit operator (RawString Name, RawString Value)(XmlAttribute attr) => (attr.Name, attr.Value);

        public static bool operator ==(XmlAttribute attr, ValueTuple<RawString, RawString> pair) => attr.Name == pair.Item1 && attr.Value == pair.Item2;
        public static bool operator !=(XmlAttribute attr, ValueTuple<RawString, RawString> pair) => !(attr == pair);
        public static bool operator ==(ValueTuple<RawString, RawString> pair, XmlAttribute attr) => attr == pair;
        public static bool operator !=(ValueTuple<RawString, RawString> pair, XmlAttribute attr) => !(attr == pair);

        public static bool operator ==(XmlAttribute attr, ValueTuple<string, string> pair) => attr.Name == pair.Item1 && attr.Value == pair.Item2;
        public static bool operator !=(XmlAttribute attr, ValueTuple<string, string> pair) => !(attr == pair);
        public static bool operator ==(ValueTuple<string, string> pair, XmlAttribute attr) => attr == pair;
        public static bool operator !=(ValueTuple<string, string> pair, XmlAttribute attr) => !(attr == pair);
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal unsafe readonly struct XmlAttribute_
    {
        /// <summary>Attribute name</summary>
        public readonly RawString Name;
        /// <summary>Attribute value</summary>
        public readonly RawString Value;

        public readonly Option<XmlNode> Node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlAttribute_(RawString name, RawString value, XmlNode_* node)
        {
            // [NOTE]
            // 'node' is null when the attribute is belonging to the xml declaration.

            Name = name;
            Value = value;
            Node = new XmlNode(node);
        }

        public override string ToString() => $"{Name.ToString()}=\"{Value.ToString()}\"";
    }
}
