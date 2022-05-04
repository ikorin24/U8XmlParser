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

        /// <summary>Get sliced xml string as utf-8 bytes data.</summary>
        /// <param name="start">start byte offset</param>
        /// <returns>sliced xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString(int start) => _core.AsRawString(start);

        /// <summary>Get sliced xml string as utf-8 bytes data.</summary>
        /// <param name="start">start byte offset</param>
        /// <param name="length">byte length</param>
        /// <returns>sliced xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString(int start, int length) => _core.AsRawString(start, length);

        /// <summary>Get sliced xml string as utf-8 bytes data.</summary>
        /// <param name="range">data range</param>
        /// <returns>sliced xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString(DataRange range) => _core.AsRawString(range);

        /// <summary>Get all nodes (target type is <see cref="XmlNodeType.ElementNode"/>)</summary>
        /// <returns>all element nodes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes() => _core.GetAllNodes();

        /// <summary>Get all nodes by specifying node type</summary>
        /// <param name="targetType">node type</param>
        /// <returns>all nodes</returns>
        public AllNodeList GetAllNodes(XmlNodeType? targetType) => _core.GetAllNodes(targetType);

        public DataLocation GetLocation(XmlNode node) => _core.GetLocation(node);

        public DataLocation GetLocation(XmlAttribute attr) => _core.GetLocation(attr);

        public DataLocation GetLocation(RawString str) => _core.GetLocation(str);

        public DataLocation GetLocation(DataRange range) => _core.GetLocation(range);

        public DataRange GetRange(XmlNode node) => _core.GetRange(node);

        public DataRange GetRange(XmlAttribute attr) => _core.GetRange(attr);

        public DataRange GetRange(RawString str) => _core.GetRange(str);

        public override string ToString() => AsRawString().ToString();
    }
}
