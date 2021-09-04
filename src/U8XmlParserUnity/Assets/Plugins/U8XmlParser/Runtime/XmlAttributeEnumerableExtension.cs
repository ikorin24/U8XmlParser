#nullable enable
using System;
using System.Collections.Generic;
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
        public static Option<XmlAttribute> FindNameOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
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
        public static Option<XmlAttribute> FindNameOrDefault<TAttributes>(this TAttributes source, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindNameOrDefault(source, name.AsSpan());
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static Option<XmlAttribute> FindNameOrDefault<TAttributes>(this TAttributes source, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
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
        public static Option<XmlAttribute> FindNameOrDefault<TAttributes>(this TAttributes source, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            return FindNameOrDefault(source, name.AsSpan());
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static XmlAttribute FindName<TAttributes>(this TAttributes source, ReadOnlySpan<byte> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static XmlAttribute FindName<TAttributes>(this TAttributes source, RawString name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static XmlAttribute FindName<TAttributes>(this TAttributes source, ReadOnlySpan<char> name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }

        /// <summary>Find an attribute by name. Returns the first attribute found.</summary>
        /// <param name="source">source list to enumerate</param>
        /// <param name="name">attribute name to find</param>
        /// <returns>a found attribute as <see cref="Option{T}"/></returns>
        public static XmlAttribute FindName<TAttributes>(this TAttributes source, string name) where TAttributes : IEnumerable<XmlAttribute>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var attr) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return attr;
        }
    }
}
