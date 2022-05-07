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
        RawString AsRawString(int start);
        RawString AsRawString(int start, int length);
        RawString AsRawString(DataRange range);

        AllNodeList GetAllNodes();
        AllNodeList GetAllNodes(XmlNodeType? targetType);

        DataLocation GetLocation(XmlNode node);
        DataLocation GetLocation(XmlAttribute attr);
        DataLocation GetLocation(RawString str);
        DataLocation GetLocation(DataRange range);

        DataRange GetRange(XmlNode node);
        DataRange GetRange(XmlAttribute attr);
        DataRange GetRange(RawString str);
    }
}
