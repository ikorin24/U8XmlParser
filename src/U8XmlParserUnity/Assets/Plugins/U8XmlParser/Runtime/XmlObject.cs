#nullable enable
using System;
using U8Xml.Internal;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    /// <summary>Xml structual object parsed from xml file.</summary>
    /// <remarks>[CAUTION] DO NOT call any methods or properties except <see cref="IsDisposed"/> after calling <see cref="Dispose"/>.</remarks>
    public unsafe sealed class XmlObject : IDisposable, IXmlObject
    {
        private readonly XmlObjectCore _core;

        /// <summary>Get whether the xml object is disposed or not.</summary>
        /// <remarks>DO NOT call any other methods or properties if the property is false.</remarks>
        public bool IsDisposed => _core.IsDisposed;

        /// <summary>Get th root node</summary>
        public XmlNode Root => _core.Root;

        /// <summary>Get xml declaration</summary>
        public Option<XmlDeclaration> Declaration => _core.Declaration;

        /// <summary>Get xml document type declaration</summary>
        public Option<XmlDocumentType> DocumentType => _core.DocumentType;

        /// <summary>Get xml entity table</summary>
        public XmlEntityTable EntityTable => _core.EntityTable;

        internal XmlObject(in XmlObjectCore core)
        {
            _core = core;
        }

        ~XmlObject() => _core.Dispose();

        /// <summary>Dispose the xml object and release all memoriess it has.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _core.Dispose();
        }

        /// <summary>Get whole xml string as utf-8 bytes data.</summary>
        /// <returns>whole xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => _core.AsRawString();

        /// <summary>Get all nodes (target type is <see cref="XmlNodeType.ElementNode"/>)</summary>
        /// <returns>all element nodes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes() => _core.GetAllNodes();

        /// <summary>Get all nodes by specifying node type</summary>
        /// <param name="targetType">node type</param>
        /// <returns>all nodes</returns>
        public AllNodeList GetAllNodes(XmlNodeType? targetType) => _core.GetAllNodes(targetType);

        public (int Line, int Position) GetLineAndPosition(XmlNode node, bool useZeroBasedNum) => _core.GetLineAndPosition(node, useZeroBasedNum);

        public (int Line, int Position) GetLineAndPosition(XmlAttribute attr, bool useZeroBasedNum) => _core.GetLineAndPosition(attr, useZeroBasedNum);

        public (int Line, int Position) GetLineAndPosition(RawString str, bool useZeroBasedNum) => _core.GetLineAndPosition(str, useZeroBasedNum);

        public int GetOffset(XmlNode node) => _core.GetOffset(node);

        public int GetOffset(XmlAttribute attr) => _core.GetOffset(attr);

        public int GetOffset(RawString str) => _core.GetOffset(str);

        public override string ToString() => AsRawString().ToString();
    }
}
