#nullable enable
using System;

namespace U8Xml.Internal
{
    internal interface IXmlObject : IDisposable
    {
        bool IsDisposed { get; }
        XmlNode Root { get; }
        Option<XmlDeclaration> Declaration { get; }
        Option<XmlDocumentType> DocumentType { get; }
        XmlEntityTable EntityTable { get; }
        RawString AsRawString();
        AllNodeList GetAllNodes();
        AllNodeList GetAllNodes(XmlNodeType? targetType);

        DataLocation GetLocation(XmlNode node, bool useZeroBasedNum);
        DataLocation GetLocation(XmlAttribute attr, bool useZeroBasedNum);
        DataLocation GetLocation(RawString str, bool useZeroBasedNum);

        DataRange GetRange(XmlNode node);
        DataRange GetRange(XmlAttribute attr);
        DataRange GetRange(RawString str);
    }
}
