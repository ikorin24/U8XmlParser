#nullable enable
using System;
using System.Collections.Generic;
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
        public static Option<XmlNode> FindNameOrDefault<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
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
        public static Option<XmlNode> FindNameOrDefault<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
        {
            return FindNameOrDefault(source, name.AsSpan());
        }

        /// <summary>Find a node by name. Returns the first node found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node as <see cref="Option{T}"/></returns>
        public static Option<XmlNode> FindNameOrDefault<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
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
        public static Option<XmlNode> FindNameOrDefault<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
        {
            return FindNameOrDefault(source, name.AsSpan());
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        public static XmlNode FindName<TNodes>(this TNodes source, ReadOnlySpan<byte> name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        public static XmlNode FindName<TNodes>(this TNodes source, RawString name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        public static XmlNode FindName<TNodes>(this TNodes source, ReadOnlySpan<char> name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }

        /// <summary>Find a node by name. Returns the first node found, or throws <see cref="InvalidOperationException"/> if not found.</summary>
        /// <param name="source">source node list to enumerate</param>
        /// <param name="name">node name to find</param>
        /// <returns>a found node</returns>
        public static XmlNode FindName<TNodes>(this TNodes source, string name) where TNodes : IEnumerable<XmlNode>
        {
            if(FindNameOrDefault(source, name).TryGetValue(out var node) == false) {
                ThrowHelper.ThrowInvalidOperation(NoMatchingMessage);
            }
            return node;
        }
    }
}
