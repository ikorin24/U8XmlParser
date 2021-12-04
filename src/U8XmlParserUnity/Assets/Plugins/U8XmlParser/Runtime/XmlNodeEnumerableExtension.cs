#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Privides extensions of <see cref="XmlNode"/> enumeration.</summary>
    public static class XmlNodeEnumerableExtension
    {
        private const string NoMatchingMessage = "Sequence contains no matching elements.";

        /// <summary>Find a node by name. Returns the first node found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node as <see cref="Option{T}"/></returns>
        public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
        {
            foreach(var child in source) {
                if(child.Name == name) {
                    return child;
                }
            }
            return Option<XmlNode>.Null;
        }

        /// <summary>Find a node by name. Returns the first node found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name.AsSpan());
        }

        /// <summary>Find a node by name. Returns the first node found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node as <see cref="Option{T}"/></returns>
        public unsafe static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
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
                return FindOrDefault(source, span);
            }
            else {
                var rentArray = ArrayPool<byte>.Shared.Rent(byteLen);
                try {
                    fixed(byte* buf = rentArray)
                    fixed(char* ptr = name) {
                        utf8.GetBytes(ptr, name.Length, buf, byteLen);
                        var span = SpanHelper.CreateReadOnlySpan<byte>(buf, byteLen);
                        return FindOrDefault(source, span);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        /// <summary>Find a node by name. Returns the first node found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name.AsSpan());
        }

        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            foreach(var child in source) {
                var childName = child.Name;
                if(childName.EndsWith(name)) {
                    if(XmlNode.TryResolveNamespaceAlias(namespaceName, child, out var nsAlias)) {

                        if(nsAlias.IsEmpty) {
                            if(childName.Length == name.Length) {
                                return child;
                            }
                        }
                        else {
                            if((childName.Length == nsAlias.Length + 1 + name.Length) && (childName.At(nsAlias.Length) == (byte)':')) {
                                return child;
                            }
                        }
                    }
                }
            }
            return Option<XmlNode>.Null;

            // --------
            //if(source.Parent.TryGetValue(out var parent) == false) {
            //    return Option<XmlNode>.Null;
            //}
            //if(XmlNode.TryResolveNamespaceAlias(namespaceName, parent, out var nsAlias) == false) {
            //    return Option<XmlNode>.Null;
            //}
            //if(nsAlias.IsEmpty) {
            //    foreach(var child in source) {
            //        if(child.Name == name) {
            //            if(child.TryFindXmlns(nsAlias.AsSpan(), out var nsNameActual) == false) {
            //                return child;
            //            }
            //            if(nsNameActual == namespaceName) {
            //                return child;
            //            }
            //        }
            //    }
            //}
            //else {
            //    var fullNameLength = nsAlias.Length + 1 + name.Length;
            //    foreach(var child in source) {
            //        var childName = child.Name;
            //        if(childName.Length == fullNameLength && childName.StartsWith(nsAlias)
            //                                              && childName.At(nsAlias.Length) == (byte)':'
            //                                              && childName.Slice(nsAlias.Length + 1) == name) {

            //            if(child.TryFindXmlns(nsAlias.AsSpan(), out var nsNameActual) == false) {
            //                return child;
            //            }
            //            if(nsNameActual == namespaceName) {
            //                return child;
            //            }
            //        }
            //    }
            //}
            //return Option<XmlNode>.Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, RawString name) => FindOrDefault(source, namespaceName, name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, RawString namespaceName, ReadOnlySpan<byte> name) => FindOrDefault(source, namespaceName.AsSpan(), name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, RawString namespaceName, RawString name) => FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());

        [SkipLocalsInit]
        public unsafe static Option<XmlNode> FindOrDefault(this XmlNodeList source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
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
                return FindOrDefault(source, nsNameUtf8, nameUtf8);
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
                        return FindOrDefault(source, nsNameUtf8, nameUtf8);
                    }
                }
                finally {
                    ArrayPool<byte>.Shared.Return(rentArray);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, ReadOnlySpan<char> namespaceName, string name) => FindOrDefault(source, namespaceName, name.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, string namespaceName, ReadOnlySpan<char> name) => FindOrDefault(source, namespaceName.AsSpan(), name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlNode> FindOrDefault(this XmlNodeList source, string namespaceName, string name) => FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, RawString name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, RawString namespaceName, ReadOnlySpan<byte> name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, RawString namespaceName, RawString name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, ReadOnlySpan<char> namespaceName, string name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, string namespaceName, ReadOnlySpan<char> name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode Find(this XmlNodeList source, string namespaceName, string name)
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<byte> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TNodes>(this TNodes source, RawString name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TNodes>(this TNodes source, ReadOnlySpan<char> name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TNodes>(this TNodes source, string name, out XmlNode node) where TNodes : IEnumerable<XmlNode>
        {
            return FindOrDefault(source, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, ReadOnlySpan<byte> namespaceName, RawString name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, RawString namespaceName, ReadOnlySpan<byte> name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, RawString namespaceName, RawString name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, ReadOnlySpan<char> namespaceName, string name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, string namespaceName, ReadOnlySpan<char> name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind(this XmlNodeList source, string namespaceName, string name, out XmlNode node)
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out node);
        }
    }
}
