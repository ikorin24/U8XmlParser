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
    }
}
