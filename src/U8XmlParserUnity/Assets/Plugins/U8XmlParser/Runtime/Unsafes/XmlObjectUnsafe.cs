#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using U8Xml.Internal;

namespace U8Xml.Unsafes
{
    /// <summary>
    /// [WARNING] DON'T use this if you don't know how to use. This method is hidden. <para/>
    /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
    /// You MUST dispose after using it. It's compatible with <see cref="XmlObject"/>
    /// </summary>
    public unsafe readonly struct XmlObjectUnsafe : IDisposable, IXmlObject
    {
        private readonly IntPtr _core;  // XmlObjectCore*

        public bool IsDisposed => _core == IntPtr.Zero;

        private XmlObjectUnsafe(IntPtr core)
        {
            _core = core;
        }

        public XmlNode Root => ((XmlObjectCore*)_core)->Root;

        public Option<XmlDeclaration> Declaration => ((XmlObjectCore*)_core)->Declaration;

        public Option<XmlDocumentType> DocumentType => ((XmlObjectCore*)_core)->DocumentType;

        public XmlEntityTable EntityTable => ((XmlObjectCore*)_core)->EntityTable;

        public RawString AsRawString() => ((XmlObjectCore*)_core)->AsRawString();

        public void Dispose()
        {
            var core = Interlocked.Exchange(ref Unsafe.AsRef(_core), IntPtr.Zero);
            if(core == IntPtr.Zero) { return; }
            ((XmlObjectCore*)core)->Dispose();
            Marshal.FreeHGlobal(core);
            AllocationSafety.Remove(sizeof(XmlObjectCore));
        }

        /// <summary>Get all nodes (target type is <see cref="XmlNodeType.ElementNode"/>)</summary>
        /// <returns>all element nodes</returns>
        public AllNodeList GetAllNodes() => ((XmlObjectCore*)_core)->GetAllNodes();

        /// <summary>Get all nodes by specifying node type</summary>
        /// <param name="targetType">node type</param>
        /// <returns>all nodes</returns>
        public AllNodeList GetAllNodes(XmlNodeType? targetType) => ((XmlObjectCore*)_core)->GetAllNodes(targetType);

        public (int Line, int Position) GetLineAndPosition(XmlNode node, bool useZeroBasedNum) => ((XmlObjectCore*)_core)->GetLineAndPosition(node, useZeroBasedNum);

        public (int Line, int Position) GetLineAndPosition(XmlAttribute attr, bool useZeroBasedNum) => ((XmlObjectCore*)_core)->GetLineAndPosition(attr, useZeroBasedNum);

        public (int Line, int Position) GetLineAndPosition(RawString str, bool useZeroBasedNum) => ((XmlObjectCore*)_core)->GetLineAndPosition(str, useZeroBasedNum);

        public int GetOffset(XmlNode node) => ((XmlObjectCore*)_core)->GetOffset(node);

        public int GetOffset(XmlAttribute attr) => ((XmlObjectCore*)_core)->GetOffset(attr);

        public int GetOffset(RawString str) => ((XmlObjectCore*)_core)->GetOffset(str);

        public override string ToString() => AsRawString().ToString();

        internal static XmlObjectUnsafe Create(in XmlObjectCore core)
        {
            var size = sizeof(XmlObjectCore);
            var ptr = Marshal.AllocHGlobal(size);
            AllocationSafety.Add(size);
            *((XmlObjectCore*)ptr) = core;
            return new XmlObjectUnsafe(ptr);
        }
    }
}
