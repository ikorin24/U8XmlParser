#nullable enable
using System;

namespace U8Xml.Internal
{
    internal interface IXmlObject : IDisposable
    {
        bool IsDisposed { get; }
        XmlNode Root { get; }
        XmlDeclaration Declaration { get; }
        RawString AsRawString();
    }
}
