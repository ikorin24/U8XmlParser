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

        (int Line, int Position) GetLineAndPosition(XmlNode node, bool useZeroBasedNum);
        (int Line, int Position) GetLineAndPosition(XmlAttribute attr, bool useZeroBasedNum);
        (int Line, int Position) GetLineAndPosition(RawString str, bool useZeroBasedNum);

        int GetOffset(XmlNode node);
        int GetOffset(XmlAttribute attr);
        int GetOffset(RawString str);
    }
}
