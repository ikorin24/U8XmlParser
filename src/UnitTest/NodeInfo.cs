#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using U8Xml;

namespace UnitTest
{
    internal sealed class NodeInfo : IEquatable<NodeInfo>
    {
        public readonly string Name;
        public readonly string InnerText;
        public readonly ReadOnlyMemory<(string name, string value)> Attr;
        public readonly ReadOnlyMemory<NodeInfo> Children;

        public NodeInfo(XmlNode node)
        {
            Name = node.Name.ToString();
            InnerText = node.InnerText.ToString();
            Attr = node.Attributes.Select(attr => (attr.Name.ToString(), attr.Value.ToString())).ToArray();
            Children = node.Children.Select(c => new NodeInfo(c)).ToArray();
        }

        public NodeInfo(string name, string innerText, (string attrName, string attrValue)[]? attrs, params NodeInfo[]? children)
        {
            Name = name;
            InnerText = innerText;
            Attr = attrs;
            Children = children;
        }

        public override bool Equals(object? obj) => obj is NodeInfo info && Equals(info);

        public bool Equals(NodeInfo? other)
        {
            var result = other is not null &&
                         Name == other.Name &&
                         InnerText == other.InnerText;
            if(result == false) { return false; }

            if(Attr.Span.Length != other!.Attr.Span.Length) { return false; }
            var attr1 = Attr.Span;
            var attr2 = other.Attr.Span;
            for(int i = 0; i < attr1.Length; i++) {
                result &= attr1[i].Equals(attr2[i]);
            }
            if(result == false) { return false; }

            if(Children.Length != other!.Children.Length) { return false; }
            var span1 = Children.Span;
            var span2 = other.Children.Span;
            for(int i = 0; i < span1.Length; i++) {
                result &= span1[i].Equals(span2[i]);
            }
            return result;
        }

        public override int GetHashCode() => HashCode.Combine(Name, InnerText, Attr);
    }

    internal sealed class NodeInfoComparer : IEqualityComparer<NodeInfo>
    {
        public static readonly NodeInfoComparer Default = new NodeInfoComparer();

        public bool Equals(NodeInfo? x, NodeInfo? y) => x is null ? y is null : x.Equals(y);

        public int GetHashCode(NodeInfo obj) => 1;   // It's bad but legal.
    }
}
