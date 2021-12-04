#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Privides extensions of <see cref="XmlAttribute"/> enumeration.</summary>
    public static class XmlAttributeEnumerableExtension
    {
        private const string NoMatchingMessage = "Sequence contains no matching elements.";

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            foreach(var attr in source) {
                if(attr.Name == name) {
                    return attr;
                }
            }
            return Option<XmlAttribute>.Null;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name.AsSpan());
        }

        public static Option<XmlAttribute> FindOrDefault(this XmlAttributeList source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name)
        {
            if(XmlnsHelper.TryResolveNamespaceAlias(namespaceName, source.Node, out var nsAlias) == false) {
                return Option<XmlAttribute>.Null;
            }
            if(nsAlias.IsEmpty) {
                return FindOrDefault(source, name);
            }
            var fullNameLength = nsAlias.Length + 1 + name.Length;
            foreach(var attr in source) {
                var attrName = attr.Name;
                if(attrName.Length == fullNameLength && attrName.StartsWith(nsAlias)
                                                     && attrName.At(nsAlias.Length) == (byte)':'
                                                     && attrName.Slice(nsAlias.Length + 1) == name) {
                    return attr;
                }
            }
            return Option<XmlAttribute>.Null;
        }

        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(typeof(TAttributes) == typeof(XmlAttributeList)) {
                return Unsafe.As<TAttributes, XmlAttributeList>(ref source).FindOrDefault(namespaceName, name);
            }

            foreach(var attr in source) {
                if(attr.Node.TryGetValue(out var n) == false) {
                    continue;
                }
                if(XmlnsHelper.TryResolveNamespaceAlias(namespaceName, n, out var nsAlias) == false) {
                    continue;
                }
                if(nsAlias.IsEmpty) {
                    if(attr.Name == name) {
                        return attr;
                    }
                }
                else {
                    var fullNameLength = nsAlias.Length + 1 + name.Length;
                    var attrName = attr.Name;
                    if(attrName.Length == fullNameLength && attrName.StartsWith(nsAlias)
                                                         && attrName.At(nsAlias.Length) == (byte)':'
                                                         && attrName.Slice(nsAlias.Length + 1) == name) {
                        return attr;
                    }
                }
            }
            return Option<XmlAttribute>.Null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, RawString namespaceName, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName.AsSpan(), name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, RawString namespaceName, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [SkipLocalsInit]
        public unsafe static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(name.IsEmpty) {
                return Option<XmlAttribute>.Null;
            }

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

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name.AsSpan());
        }

        [SkipLocalsInit]
        public unsafe static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(namespaceName.IsEmpty || name.IsEmpty) {
                return Option<XmlAttribute>.Null;
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
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, string namespaceName, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName.AsSpan(), name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<XmlAttribute> FindOrDefault<TAttributes>(this TAttributes source, string namespaceName, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName.AsSpan(), name.AsSpan());
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, RawString namespaceName, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, RawString namespaceName, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, string namespaceName, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlAttribute Find<TAttributes>(this TAttributes source, string namespaceName, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindOrDefault(source, namespaceName, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, RawString name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<byte> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<byte> namespaceName, RawString name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, RawString namespaceName, ReadOnlySpan<byte> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, RawString namespaceName, RawString name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, string name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<char> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, ReadOnlySpan<char> namespaceName, string name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, string namespaceName, ReadOnlySpan<char> name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFind<TAttributes>(this TAttributes source, string namespaceName, string name, out XmlAttribute attribute) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindOrDefault(source, namespaceName, name).TryGetValue(out attribute);
        }
    }
}
