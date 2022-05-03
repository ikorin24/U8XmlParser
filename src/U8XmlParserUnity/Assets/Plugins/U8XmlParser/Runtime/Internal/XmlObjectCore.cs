#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace U8Xml.Internal
{
    internal unsafe readonly struct XmlObjectCore : IDisposable, IXmlObject
    {
        private readonly IntPtr _rawByteData;
        private readonly int _byteLength;
        private readonly int _offset;        // 0 or 3 (3 is UTF-8 BOM)
        private readonly NodeStore _store;
        private readonly OptionalNodeList _optional;
        private readonly RawStringTable _entities;

        public bool IsDisposed => _rawByteData == IntPtr.Zero;

        public XmlNode Root => _store.RootNode;

        public Option<XmlDeclaration> Declaration => new XmlDeclaration(_optional.Declaration);

        public Option<XmlDocumentType> DocumentType => new XmlDocumentType(_optional.DocumentType);

        public XmlEntityTable EntityTable => new XmlEntityTable(_entities);

        internal XmlObjectCore(ref UnmanagedBuffer buffer, int offset, ref NodeStore store, OptionalNodeList optional, RawStringTable entities)
        {
            buffer.TransferMemoryOwnership(out _rawByteData, out _byteLength);
            _offset = offset;
            _store = store;
            store = default;
            _optional = optional;
            _entities = entities;
        }

        public void Dispose()
        {
            var data = Interlocked.Exchange(ref Unsafe.AsRef(_rawByteData), default);
            if(data != IntPtr.Zero) {
                AllocationSafety.Remove(_byteLength);
                Marshal.FreeHGlobal(data);
                Unsafe.AsRef(_byteLength) = 0;
                Unsafe.AsRef(_offset) = 0;
                _store.Dispose();
                _optional.Dispose();
                _entities.Dispose();
            }
        }

        public int GetOffset(XmlNode node)
        {
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var nodeHead = node.NodeHeadPtr;
            var nodeLen = node.NodeByteLen;
            if(CheckContainsMemory(dataHead, dataLen, nodeHead, nodeLen) == false) {
                ThrowHelper.ThrowArg("The target node does not belong to the xml.");
            }
            long offset = nodeHead - dataHead;
            return checked((int)offset);
        }

        public int GetOffset(XmlAttribute attr)
        {
            var rawStr = attr.AsRawString();
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var strHead = rawStr.GetPtr();
            var strLen = rawStr.Length;

            Debug.Assert(strLen >= 0);
            Debug.Assert(dataLen >= 0);

            if(CheckContainsMemory(dataHead, dataLen, strHead, strLen) == false) {
                ThrowHelper.ThrowArg("The target attribute does not belong to the xml.");
            }
            long offset = strHead - dataHead;
            return checked((int)offset);
        }

        public int GetOffset(RawString str)
        {
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var strHead = str.GetPtr();
            var strLen = str.Length;

            Debug.Assert(strLen >= 0);
            Debug.Assert(dataLen >= 0);

            if(CheckContainsMemory(dataHead, dataLen, strHead, strLen) == false) {
                ThrowHelper.ThrowArg("The target string does not belong to the xml.");
            }
            long offset = strHead - dataHead;
            return checked((int)offset);
        }

        public (int Line, int Position) GetLineAndPosition(XmlNode node, bool useZeroBasedNum)
        {
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var nodeHead = node.NodeHeadPtr;
            var nodeLen = node.NodeByteLen;

            Debug.Assert(nodeLen >= 0);
            Debug.Assert(dataLen >= 0);

            if(CheckContainsMemory(dataHead, dataLen, nodeHead, nodeLen) == false) {
                ThrowHelper.ThrowArg("The target node does not belong to the xml.");
            }
            return GetLineAndPosition(dataHead, dataLen, nodeHead, nodeLen, useZeroBasedNum);
        }

        public (int Line, int Position) GetLineAndPosition(XmlAttribute attr, bool useZeroBasedNum)
        {
            var rawStr = attr.AsRawString();
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var strHead = rawStr.GetPtr();
            var strLen = rawStr.Length;

            Debug.Assert(strLen >= 0);
            Debug.Assert(dataLen >= 0);

            if(CheckContainsMemory(dataHead, dataLen, strHead, strLen) == false) {
                ThrowHelper.ThrowArg("The target attribute does not belong to the xml.");
            }
            return GetLineAndPosition(dataHead, dataLen, strHead, strLen, useZeroBasedNum);
        }

        public (int Line, int Position) GetLineAndPosition(RawString str, bool useZeroBasedNum)
        {
            var dataHead = (byte*)_rawByteData;
            var dataLen = _byteLength;
            var strHead = str.GetPtr();
            var strLen = str.Length;

            Debug.Assert(strLen >= 0);
            Debug.Assert(dataLen >= 0);

            if(CheckContainsMemory(dataHead, dataLen, strHead, strLen) == false) {
                ThrowHelper.ThrowArg("The target string does not belong to the xml.");
            }
            return GetLineAndPosition(dataHead, dataLen, strHead, strLen, useZeroBasedNum);
        }

        /// <summary>Get whole xml string as utf-8 bytes data.</summary>
        /// <returns>whole xml string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawString AsRawString() => new RawString((byte*)_rawByteData + _offset, _byteLength - _offset);

        /// <summary>Get all nodes (target type is <see cref="XmlNodeType.ElementNode"/>)</summary>
        /// <returns>all element nodes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes() => _store.GetAllNodes(XmlNodeType.ElementNode);

        /// <summary>Get all nodes by specifying node type</summary>
        /// <param name="targetType">node type</param>
        /// <returns>all nodes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllNodeList GetAllNodes(XmlNodeType? targetType) => _store.GetAllNodes(targetType);

        private bool CheckContainsMemory(byte* dataHead, int dataLen, byte* targetHead, int targetLen)
        {
            return (dataHead <= targetHead) && (targetHead + targetLen) <= (dataHead + dataLen);
        }

        private static (int Line, int Position) GetLineAndPosition(byte* dataHead, int dataLen, byte* targetHead, int targetLen, bool useZeroBasedNum)
        {
            Debug.Assert(targetLen >= 0);
            Debug.Assert(dataLen >= 0);
            Debug.Assert(dataHead <= targetHead);
            Debug.Assert((targetHead + targetLen) <= (dataHead + dataLen));

            int lineNum = 0;
            int pos;
            byte* lineHead = dataHead;
            for(byte* ptr = dataHead; ptr < targetHead; ptr++) {
                if(*ptr == '\n') {
                    lineNum++;
                    lineHead = ptr + 1;
                }
            }

            checked {
                pos = (int)(targetHead - lineHead);
                if(useZeroBasedNum == false) {
                    lineNum += 1;
                    pos += 1;
                }
            }
            return (Line: lineNum, Position: pos);
        }
    }

    internal unsafe struct NodeStore : IDisposable
    {
        private CustomList<XmlNode_> _allNodes;
        private CustomList<XmlAttribute_> _allAttrs;
        private int _elementNodeCount;

        // [NOTE]
        // Don't add a node from this property directly.
        public CustomList<XmlNode_> AllNodes => _allNodes;

        public CustomList<XmlAttribute_> AllAttrs => _allAttrs;
        public int NodeCount => _allNodes.Count;
        public int ElementNodeCount => _elementNodeCount;
        public int TextNodeCount => _allNodes.Count - _elementNodeCount;

        public XmlNode RootNode => new XmlNode(_allNodes.FirstItem);

        public static NodeStore Create()
        {
            CustomList<XmlNode_> allNodes = default;
            CustomList<XmlAttribute_> allAttrs = default;
            try {
                allNodes = CustomList<XmlNode_>.Create();
                allAttrs = CustomList<XmlAttribute_>.Create();
                return new NodeStore
                {
                    _allNodes = allNodes,
                    _allAttrs = allAttrs,
                    _elementNodeCount = 0,
                };
            }
            catch {
                allNodes.Dispose();
                allAttrs.Dispose();
                throw;
            }
        }

        public XmlNode_* AddTextNode(int depth, byte* nodeStrPtr)
        {
            var textNode = _allNodes.GetPointerToAdd(out var nodeIndex);
            *textNode = XmlNode_.CreateTextNode(_allNodes, nodeIndex, depth, nodeStrPtr, _allAttrs);
            return textNode;
        }

        public XmlNode_* AddElementNode(RawString name, int depth, byte* nodeStrPtr)
        {
            var elementNode = _allNodes.GetPointerToAdd(out var nodeIndex);
            *elementNode = XmlNode_.CreateElementNode(_allNodes, nodeIndex, depth, name, nodeStrPtr, _allAttrs);
            _elementNodeCount++;
            return elementNode;
        }

        public AllNodeList GetAllNodes(XmlNodeType? targetType)
        {
            var count = targetType switch
            {
                null => NodeCount,
                XmlNodeType.ElementNode => ElementNodeCount,
                XmlNodeType.TextNode => TextNodeCount,
                _ => default,
            };
            return new AllNodeList(_allNodes, count, targetType);
        }

        public CustomList<XmlNode_>.Enumerator GetAllNodesEnumerator() => _allNodes.GetEnumerator();

        public void Dispose()
        {
            _allNodes.Dispose();
            _allAttrs.Dispose();
        }
    }
}
