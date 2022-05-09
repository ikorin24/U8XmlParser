#nullable enable
using System;
using System.IO;
using System.Diagnostics;
using U8Xml.Internal;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace U8Xml
{
    /// <summary>Parser for xml file</summary>
    public static unsafe class XmlParser
    {
        /// <summary>Byte Order Mark of utf-8</summary>
        private static ReadOnlySpan<byte> Utf8BOM => new byte[] { 0xEF, 0xBB, 0xBF };   // Bytes are embedded in dll, so there are no heap allocation.
        /// <summary>Byte Order Mark of utf-16 little endian</summary>
        private static ReadOnlySpan<byte> Utf16LEBOM => new byte[] { 0xFF, 0xFE };


        /// <summary>Parse xml of <see langword="string"/></summary>
        /// <param name="text">text of xml</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(string text) => Parse(text.AsSpan());

        /// <summary>Parse xml of <see cref="ReadOnlySpan{T}"/> of type <see cref="char"/> </summary>
        /// <param name="text">text of xml</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(ReadOnlySpan<char> text)
        {
            return new XmlObject(ParseCharSpanCore(text));
        }

        /// <summary>Parse xml encoded as UTF8 (both with and without BOM).</summary>
        /// <param name="utf8Text">utf-8 byte span data</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(ReadOnlySpan<byte> utf8Text)
        {
            var buf = new UnmanagedBuffer(utf8Text);
            try {
                return new XmlObject(ParseCore(ref buf, utf8Text.Length));
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        /// <summary>Parse xml file encoded as UTF8 (both with and without BOM).</summary>
        /// <param name="stream">stream to read</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream)
        {
            var fileSizeHint = stream.CanSeek ? (int)stream.Length : 1024 * 1024;
            return Parse(stream, fileSizeHint);
        }

        /// <summary>Parse xml file encoded as UTF8 (both with and without BOM).</summary>
        /// <param name="stream">stream to read</param>
        /// <param name="fileSizeHint">file size hint which is used for optimizing memory</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream, int fileSizeHint)
        {
            if(stream is null) { ThrowHelper.ThrowNullArg(nameof(stream)); }
            var (buf, length) = stream!.ReadAllToUnmanaged(fileSizeHint);
            try {
                return new XmlObject(ParseCore(ref buf, length));
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        /// <summary>Parse xml file encoded as specified encoding.</summary>
        /// <param name="stream">stream to read</param>
        /// <param name="encoding">encoding of <paramref name="stream"/></param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream, Encoding encoding)
        {
            var fileSizeHint = stream.CanSeek ? (int)stream.Length : 1024 * 1024;
            return Parse(stream, encoding, fileSizeHint);
        }

        /// <summary>Parse xml file encoded as specified encoding.</summary>
        /// <param name="stream">stream to read</param>
        /// <param name="encoding">encoding of <paramref name="stream"/></param>
        /// <param name="fileSizeHint">file size hint</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream, Encoding encoding, int fileSizeHint)
        {
            if(stream is null) { ThrowHelper.ThrowNullArg(nameof(stream)); }
            if(encoding is null) { ThrowHelper.ThrowNullArg(nameof(encoding)); }
            if(encoding is UTF8Encoding || encoding is ASCIIEncoding) {
                return Parse(stream!);
            }
            else if(encoding!.Equals(Encoding.Unicode) && BitConverter.IsLittleEndian) {
                // The runtime is little endian and the encoding is utf-16 LE with BOM
                var (utf16LEBuf, utf16ByteLength) = stream!.ReadAllToUnmanaged(fileSizeHint);
                using(utf16LEBuf) {
                    var utf16LESpan = SpanHelper.CreateReadOnlySpan<char>((void*)utf16LEBuf.Ptr, utf16ByteLength / sizeof(char));
                    // Remove BOM
                    if(MemoryMarshal.AsBytes(utf16LESpan).StartsWith(Utf16LEBOM)) {
                        utf16LESpan = utf16LESpan.Slice(1);
                    }
                    return Parse(utf16LESpan);
                }
            }
            else {
                var (buf, byteLength) = stream!.ReadAllToUnmanaged(fileSizeHint);
                UnmanagedBuffer charBuf = default;
                ReadOnlySpan<char> charSpan = default;
                try {
                    try {
                        var charCount = encoding.GetCharCount((byte*)buf.Ptr, byteLength);
                        charBuf = new UnmanagedBuffer(charCount * sizeof(char));
                        encoding.GetChars((byte*)buf.Ptr, byteLength, (char*)charBuf.Ptr, charCount);
                        charSpan = SpanHelper.CreateReadOnlySpan<char>((void*)charBuf.Ptr, charCount);
                    }
                    finally {
                        buf.Dispose();
                    }
                    return Parse(charSpan);
                }
                finally {
                    charBuf.Dispose();
                }
            }
        }

        /// <summary>Parse xml file encoded as UTF8 (both with and without BOM).</summary>
        /// <param name="filePath">file path to parse</param>
        /// <returns>xml object</returns>
        public static XmlObject ParseFile(string filePath)
        {
            return ParseFile(filePath, UTF8ExceptionFallbackEncoding.Instance);
        }

        /// <summary>Parse xml file encoded as specified encoding.</summary>
        /// <param name="filePath">file path to parse</param>
        /// <param name="encoding">encoding of the file</param>
        /// <returns>xml object</returns>
        public static XmlObject ParseFile(string filePath, Encoding encoding)
        {
            if(filePath is null) { ThrowHelper.ThrowNullArg(nameof(filePath)); }
            if(encoding is null) { ThrowHelper.ThrowNullArg(nameof(encoding)); }

            return new XmlObject(ParseFileCore(filePath!, encoding!));
        }

        internal static XmlObjectCore ParseFileCore(string filePath, Encoding encoding)
        {
            if(encoding is UTF8Encoding || encoding is ASCIIEncoding) {
                UnmanagedBuffer buffer = default;
                try {
                    int length;
                    (buffer, length) = FileHelper.ReadFileToUnmanaged(filePath);
                    return ParseCore(ref buffer, length);
                }
                catch {
                    buffer.Dispose();
                    throw;
                }
            }
            else if(encoding.Equals(Encoding.Unicode) && BitConverter.IsLittleEndian) {
                // The runtime is little endian and the encoding is utf-16 LE with BOM
                var (utf16LEBuf, utf16LEByteLength) = FileHelper.ReadFileToUnmanaged(filePath);
                try {
                    var charSpan = SpanHelper.CreateReadOnlySpan<char>((void*)utf16LEBuf.Ptr, utf16LEByteLength / sizeof(char));
                    // Remove BOM
                    if(MemoryMarshal.AsBytes(charSpan).StartsWith(Utf16LEBOM)) {
                        charSpan = charSpan.Slice(1);
                    }
                    return ParseCharSpanCore(charSpan);
                }
                finally {
                    utf16LEBuf.Dispose();
                }
            }
            else {
                UnmanagedBuffer charBuf = default;
                ReadOnlySpan<char> charSpan = default;
                try {
                    var (buf, byteLength) = FileHelper.ReadFileToUnmanaged(filePath);
                    try {
                        var charCount = encoding.GetCharCount((byte*)buf.Ptr, byteLength);
                        charBuf = new UnmanagedBuffer(charCount * sizeof(char));
                        encoding.GetChars((byte*)buf.Ptr, byteLength, (char*)charBuf.Ptr, charCount);
                        charSpan = SpanHelper.CreateReadOnlySpan<char>((void*)charBuf.Ptr, charCount);
                    }
                    finally {
                        buf.Dispose();
                    }
                    return ParseCharSpanCore(charSpan);
                }
                finally {
                    charBuf.Dispose();
                }
            }
        }

        private static XmlObjectCore ParseCharSpanCore(ReadOnlySpan<char> charSpan)
        {
            var utf8Buf = default(UnmanagedBuffer);
            try {
                var utf8Enc = UTF8ExceptionFallbackEncoding.Instance;
                fixed(char* ptr = charSpan) {
                    var byteLen = utf8Enc.GetByteCount(ptr, charSpan.Length);
                    utf8Buf = new UnmanagedBuffer(byteLen);
                    utf8Enc.GetBytes(ptr, charSpan.Length, (byte*)utf8Buf.Ptr, utf8Buf.Length);
                }
                return ParseCore(ref utf8Buf, utf8Buf.Length);
            }
            catch {
                utf8Buf.Dispose();
                throw;
            }
        }

        internal static XmlObjectCore ParseCore(ref UnmanagedBuffer utf8Buf, int length)
        {
            // Remove utf-8 bom
            var offset = utf8Buf.AsSpan(0, 3).SequenceEqual(Utf8BOM) ? 3 : 0;
            var rawString = new RawString((byte*)utf8Buf.Ptr + offset, length - offset);

            NodeStore nodeStore = default;
            OptionalNodeList optional = default;
            RawStringTable entities = default;
            try {
                nodeStore = NodeStore.Create();
                optional = OptionalNodeList.Create();
                StartStateMachine(rawString, ref nodeStore, optional, ref entities);
                return new XmlObjectCore(ref utf8Buf, offset, ref nodeStore, optional, entities);
            }
            catch {
                nodeStore.Dispose();
                optional.Dispose();
                entities.Dispose();
                throw;
            }
        }

        private static void StartStateMachine(RawString data, ref NodeStore store, OptionalNodeList optional, ref RawStringTable entities)
        {
            // Encoding assumes utf-8 without bom. Others are not supported.
            // Parse format by using a state machine. (It's very primitive but fastest.)
            int i = 0;
            using var nodeStack = new NodeStack(32);

        None:   // Out of xml node
            {
                if(SkipEmpty(data, ref i) == false) {
                    if(nodeStack.Count == 0) { goto End; }
                    else {
                        throw NewFormatException(data, i, "Unexpected end of xml. The parent node is not closed.");
                    }
                }

                // Must be '<', otherwise error.
                if(data.At(i) == '<') {
                    if(nodeStack.Count == 0 && store.NodeCount > 0) {
                        throw NewFormatException(data, i, "Xml does not have multiple root nodes.");
                    }
                    if(i + 1 < data.Length && data.At(i + 1) == '/') {
                        i += 2;
                        goto NodeTail;
                    }
                    else {
                        i++;
                        goto NodeHead;
                    }
                }
                else { goto InnerText; }
            }

        InnerText:
            {
                var nodeStrStart = i;
                byte* nodeStrPtr = data.GetPtr() + nodeStrStart;
                if(nodeStack.TryPeek(out var node) == false) {
                    throw NewFormatException(data, i, "Text node can not be a root node.");
                }
                var textNode = store.AddTextNode(nodeStack.Count, nodeStrPtr);
                GetInnerText(data, ref i, out var text);
                textNode->InnerText = text;
                textNode->NodeStrLength = text.Length;
                XmlNode_.AddChildTextNode(node, textNode);
                goto None;
            }

        NodeHead:   // Current data[i] is next char to '<'.
            {
                if(data.At(i) == '!') {
                    // Skip comment <!--xxx-->
                    if((i + 2 < data.Length) && (data.At(i + 1) == '-') && (data.At(i + 2) == '-'))  // Start with "<!--"
                    {
                        var commentStart = i - 1;   // data[commentStart] == '<'
                        if(SkipComment(data, ref i) == false) { throw NewFormatException(data, commentStart, "The comment is not closed."); }
                        goto None;
                    }
                    else {
                        i++;
                        goto ExtraNode; // extra node. ex)  <!ENTITY st3 "font-family:'Arial';">
                    }
                }
                else if(data.At(i) == '?') {
                    var nodeStart = i - 1;  // data[nodeStart] == '<'
                    if(i + 4 < data.Length && data.At(i + 1) == 'x' && data.At(i + 2) == 'm' && data.At(i + 3) == 'l' && data.At(i + 4) == ' ') // Start with "<?xml "
                    {
                        if(store.NodeCount != 0) {
                            throw NewFormatException(data, nodeStart, "Xml declaration must be at the head in xml.");
                        }
                        if(optional.Declaration->Body.IsEmpty == false) {
                            throw NewFormatException(data, nodeStart, "Multiple xml declaration in xml.");
                        }

                        // ex) <?xml version="1.0" encoding="UTF-8"?>
                        GetXmlDeclaration(data, ref i, store.AllAttrs, optional);
                        goto None;
                    }
                    else { throw NewFormatException(data, nodeStart, "Invalid node. It must start with '<?xml '"); }
                }
                else {
                    var attrs = store.AllAttrs;
                    var nodeStrStart = i - 1;
                    GetNodeName(data, ref i, out var name);
                    var node = store.AddElementNode(name, nodeStack.Count, data.GetPtr() + nodeStrStart);
                    while(true) {
                        if(data.At(i) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode_.AddChildElementNode(nodeStack.Peek(), node);
                            }
                            nodeStack.Push(node);
                            i++;
                            if(i >= data.Length) { throw NewFormatException(data, i, $"Unexpected end of xml. The node '<{name}>' is not closed"); }
                            goto None;
                        }
                        else if((i + 1 < data.Length) && data.At(i) == '/' && data.At(i + 1) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode_.AddChildElementNode(nodeStack.Peek(), node);
                            }
                            i += 2;
                            node->NodeStrLength = i - nodeStrStart;
                            goto None;
                        }
                        else {
                            var attr = attrs.GetPointerToAdd(out _);
                            *attr = GetAttr(data, ref i, node);
                            var attrName = attr->Name;
                            const uint xmln = (byte)'x' + ((byte)'m' << 8) + ((byte)'l' << 16) + ((byte)'n' << 24);
                            if((attrName.Length >= 5) && (*(uint*)attrName.GetPtr() == xmln)
                                                      && (attrName.At(4) == (byte)'s')) {
                                node->HasXmlNamespaceAttr = true;
                            }

                            if(node->HasAttribute == false) {
                                node->AttrIndex = attrs.Count - 1;
                            }
                            node->AttrCount++;
                        }
                    }
                }
            }

        NodeTail:
            {
                GetNodeName(data, ref i, out var name);
                var node = nodeStack.Pop();
                if(node->Name.SequenceEqual(name) == false) {
                    throw NewFormatException(data, i - name.Length - 2, $"Unexpected node tail. expected: </{node->Name}>, actual: </{name}>");
                }
                if(data.At(i) == '>') {
                    i++;
                    long len = data.GetPtr() + i - node->NodeStrPtr;
                    node->NodeStrLength = checked((int)len);
                    if(node->ChildTextCount == 1 && node->ChildElementCount == 0) {
                        Debug.Assert(node->FirstChild->NodeType == XmlNodeType.TextNode);
                        node->InnerText = node->FirstChild->InnerText;
                    }
                    goto None;
                }
                else { throw NewFormatException(data, i, $"Unexpected character. expected: '>', actual: '{(char)data.At(i)}'"); }
            }

        ExtraNode:  // Current data[i] is next char to "<!". (except comment out)
            {
                var start = i;
                if(TryParseCDATA(data, ref i, nodeStack)) {
                    goto None;
                }
                else if(TryParseDocType(data, ref i, store.NodeCount > 0, optional, ref entities)) {
                    goto None;
                }
                else {
                    var unknownNodeStr = data.Slice(start, Math.Min(6, data.Length - start));
                    throw NewFormatException(data, start - 2, $"Unknown node starts with '<!{unknownNodeStr}'. It should be '<!DOCTYPE ...>' or '<![CDATA[...]]>'.");
                }
            }

        End:
            {
                if(store.NodeCount == 0) {
                    throw NewFormatException(data, i, "Xml must have at least one node.");
                }
                Debug.Assert(nodeStack.Count == 0);
                return;
            }
        }

        private static void ParseDtd(RawString data, ref int i, ref RawStringPairList list, bool isInternalSubset)
        {
            while(true) {
                {
                    var pos = i;
                    if(SkipEmpty(data, ref i) == false) { throw NewFormatException(data, pos, "Unexpected end of xml. DOCTYPE is not closed."); }
                }

                var c = data.At(i++);
                if(isInternalSubset && c == ']') {
                    break;
                }

                if(c != '<') { throw NewFormatException(data, i - 1, $"Failed to parse DOCTYPE. expected: '<', actual: '{(char)c}'."); }
                if((i + 2 < data.Length) && (data.At(i) == '!') && (data.At(i + 1) == '-') && (data.At(i + 2) == '-'))  // Start with "<!--"
                {
                    // Skip comment <!--xxx-->
                    SkipComment(data, ref i);
                    continue;
                }

                if(((i + 6) < data.Length) &&
                   (data.At(i) == '!') && (data.At(i + 1) == 'E') && (data.At(i + 2) == 'N') && (data.At(i + 3) == 'T') &&
                   (data.At(i + 4) == 'I') && (data.At(i + 5) == 'T') && (data.At(i + 6) == 'Y') && (data.At(i + 7) == ' ')) {

                    // "<!ENTITY "
                    i += 8;

                    var entityNameStart = i;
                    if(SkipToEmpty(data, ref i) == false) {
                        throw NewFormatException(data, entityNameStart, "Unexpected end of xml. ENTITY is invalid formatted.");
                    }
                    var name = data.SliceUnsafe(entityNameStart, i - entityNameStart);
                    if(SkipEmpty(data, ref i) == false) {
                        throw NewFormatException(data, entityNameStart + name.Length, "Unexpected end of xml. ENTITY is not closed.");
                    }

                    var quote = data.At(i);
                    if(quote != '"' && quote != '\'') { throw NewFormatException(data, i, $"Invalid character. expected ''' or '\"', actual: '{(char)quote}'."); }
                    i++;
                    var k = i;
                    while(true) {
                        i++;
                        if(i >= data.Length) { throw NewFormatException(data, k, "Unexpected end of xml. ENTITY is not closed."); }
                        var q = data.At(i);
                        if(q == quote) { break; }
                    }
                    var value = data.SliceUnsafe(k, i - k);
                    i++;
                    list.Add(name, value);

                    {
                        var pos = i;
                        if(SkipUntil((byte)'>', data, ref i) == false) {
                            throw NewFormatException(data, pos, "Unexpected end of xml. ENTITY is not closed.");
                        }
                    }
                    continue;
                }

                {
                    // Skip other types of tag except ENTITY
                    var pos = i;
                    if(SkipUntil((byte)'>', data, ref i) == false) {
                        throw NewFormatException(data, pos, "Unexpected end of xml. The tag is not closed.");
                    }
                    continue;
                }
            }


            static bool SkipUntil(byte ascii, RawString data, ref int i)
            {
                // Returns false if end of file

                while(true) {
                    if(i >= data.Length) { return false; }
                    if(data.At(i++) == ascii) { return true; }
                }
            }

            static bool SkipToEmpty(RawString data, ref int i)
            {
                // Returns false if end of file

                while(true) {
                    if(i + 1 >= data.Length) { return false; }
                    ref var next = ref data.At(i + 1);
                    i++;
                    if(next == ' ' || next == '\t' || next == '\r' || next == '\n') { break; }
                }
                return true;
            }
        }

        private static void ParseDtdInternalSubset(RawString data, ref int i, ref RawStringTable entities, out RawString internalSubset)
        {
            var contentStart = i;
            var list = default(RawStringPairList);
            try {
                ParseDtd(data, ref i, ref list, true);
                internalSubset = data.Slice(contentStart, i - contentStart - 1);

                {
                    var pos = i;
                    while(true) {
                        if(i >= data.Length) { throw NewFormatException(data, pos, "Unexpected end of xml. DOCTYPE is not closed."); }
                        if(data.At(i++) == '>') { break; }
                    }
                }

                if(list.Count == 0) {
                    return;
                }

                entities = RawStringTable.Create(list.Count);
                for(int l = 0; l < list.Count; l++) {
                    ref readonly var item = ref list[l];

                    // An entity can refer to another entity that was defined before it.
                    if(ContainsAlias(item.Value, out var alias)) {
                        if(entities.TryGetValue(alias, out _) == false) {
                            int offset = checked((int)(uint)(alias.GetPtr() - data.GetPtr()));
                            throw NewFormatException(data, offset, "Alias can not be resolved.");
                        }
                    }

                    // Ignore the entity if the key is duplicated.
                    entities.TryAdd(item.Key, item.Value);
                }
                return;
            }
            finally {
                list.Dispose();
            }

            static bool ContainsAlias(RawString str, out RawString alias)
            {
                int pos = 0;
                for(int i = 0; i < str.Length; i++) {
                    if(str.At(i) == '&' && pos == 0) {
                        pos = i + 1;
                        continue;
                    }
                    if(str.At(i) == ';' && pos != 1) {
                        alias = str.SliceUnsafe(pos, i - 1 - pos);
                        return true;
                    }
                }
                alias = RawString.Empty;
                return false;
            }
        }

        private static bool TryParseDocType(RawString data, ref int i, bool hasNode, OptionalNodeList optional, ref RawStringTable entities)
        {
            // Returns false if not DOCTYPE node.

            // data[i] is the next char of "<!"

            // ------------------------------------------
            // DOCTYPE has three types.
            // 
            // 1) internal
            // <!DOCTYPE rootname [...]>
            // or
            // <!DOCTYPE rootname[...]>
            //
            // 2) external (SYSTEM)
            // <!DOCTYPE rootname SYSTEM "http://github.com/ikorin24/foo.dtd">
            //
            // 3) external (PUBLIC)
            // <!DOCTYPE html PUBLIC "-//W3C//DTD HTML 3.2 Final//EN" "http://www.w3.org/MarkUp/Wilbur/HTML32.DTD">

            ReadOnlySpan<byte> Str_DOCTYPE = stackalloc byte[] { (byte)'D', (byte)'O', (byte)'C', (byte)'T', (byte)'Y', (byte)'P', (byte)'E' };
            ReadOnlySpan<byte> Str_SYSTEM = stackalloc byte[] { (byte)'S', (byte)'Y', (byte)'S', (byte)'T', (byte)'E', (byte)'M', };
            ReadOnlySpan<byte> Str_PUBLIC = stackalloc byte[] { (byte)'P', (byte)'U', (byte)'B', (byte)'L', (byte)'I', (byte)'C' };

            // <!DOCTYPE rootname
            // |
            // `--> data[bodyStart]
            var bodyStart = i - 2;

            if(data.SliceUnsafe(i, data.Length - i).StartsWith(Str_DOCTYPE) == false) {
                // The node is not DOCTYPE
                return false;
            }
            {
                var j = i + Str_DOCTYPE.Length;
                if(j >= data.Length || IsEmptyChar(data.At(j)) == false) {
                    // The node is not DOCTYPE
                    return false;
                }
                i += Str_DOCTYPE.Length + 1;
                if(SkipEmpty(data, ref i) == false) {
                    throw NewFormatException(data, j, "Failed to parse DOCTYPE.");
                }
            }

            if(hasNode) {
                throw NewFormatException(data, i - 2, "DTD must be defined before the document root element.");
            }
            if(optional.DocumentType->Body.IsEmpty == false) {
                throw NewFormatException(data, i - 2, "Cannot have multiple DTDs.");
            }

            var docType = optional.DocumentType;
            i += Str_DOCTYPE.Length;
            if(SkipEmpty(data, ref i) == false) {
                throw NewFormatException(data, bodyStart, "Failed to parse DOCTYPE.");
            }

            // <!DOCTYPE rootname
            //           |
            //           `--> data[nameStart]
            var nameStart = i;
            Debug.Assert(IsEmptyChar(data[nameStart]) == false);
            i++;

            while(true) {
                if(i >= data.Length) {
                    throw NewFormatException(data, bodyStart, "Unexpected end of xml. Failed to parse DOCTYPE.");
                }
                var c = data.At(i++);
                if(c == '[') {
                    // <!DOCTYPE rootname[...]>
                    var name = data.Slice(nameStart, i - 1 - nameStart);
                    Debug.Assert(name.Length > 0);
                    docType->Name = name;
                    ParseDtdInternalSubset(data, ref i, ref entities, out docType->InternalSubset);
                    docType->Body = data.Slice(bodyStart, i - bodyStart);
                    return true;
                }
                else if(IsEmptyChar(c)) {
                    var name = data.Slice(nameStart, i - 1 - nameStart);
                    Debug.Assert(name.Length > 0);
                    docType->Name = name;
                    if(SkipEmpty(data, ref i) == false) {
                        throw NewFormatException(data, nameStart + name.Length, "Unexpected end of xml. Failed to parse DOCTYPE.");
                    }
                    if(data.At(i) == '[') {
                        // <!DOCTYPE rootname [...]>
                        ParseDtdInternalSubset(data, ref i, ref entities, out docType->InternalSubset);
                        docType->Body = data.Slice(bodyStart, i - bodyStart);
                        return true;
                    }
                    var identifierStart = i;
                    i++;
                    while(true) {
                        if(i >= data.Length) {
                            throw NewFormatException(data, identifierStart, "Unexpected end of xml. Failed to parse DOCTYPE.");
                        }
                        if(IsEmptyChar(data.At(i++))) {
                            break;
                        }
                    }
                    var identifier = data.SliceUnsafe(identifierStart, i - 1 - identifierStart);
                    if(identifier == Str_SYSTEM) {
                        // <!DOCTYPE rootname SYSTEM "...">
                        RawString uri;
                        {
                            var pos = i;
                            if(ReadQuotedString(data, ref i, out uri) == false) { throw NewFormatException(data, pos, "Failed to parse DOCTYPE."); }
                        }
                        {
                            var pos = i;
                            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(data, pos, "Unexpected end of xml. Failed to parse DOCTYPE."); }
                        }
                        if(i >= data.Length || data.At(i++) != '>') { throw NewFormatException(data, i, "Unexpected end of xml. DOCTYPE is not closed."); }

                        var body = data.SliceUnsafe(bodyStart, i - bodyStart);
                        var state = new ExternalDtdGetterState_
                        {
                            Body = body,
                            DtdType = ExternalDtdType.System,
                            PublicIdentifier = RawString.Empty,
                            Uri = uri,
                        };
                        // TODO:
                        DefaultExternalDtdGetter.Instance.GetDtd(new ExternalDtdGetterState(&state));
                        //throw new NotImplementedException($"DTD with SYSTEM identifier is not implemented yet. DtdType: '{state.DtdType}', PI: '{state.PublicIdentifier}', Uri: '{state.Uri}', Body: '{state.Body}'");
                        docType->Body = body;
                        return true;
                    }
                    else if(identifier == Str_PUBLIC) {
                        // <!DOCTYPE rootnode PUBLIC "..." "...">
                        RawString publicIdentifier;
                        RawString uri;
                        {
                            var pos = i;
                            if(ReadQuotedString(data, ref i, out publicIdentifier) == false) { throw NewFormatException(data, pos, "Failed to parse DOCTYPE."); }
                        }
                        {
                            var pos = i;
                            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(data, pos, "Unexpected end of xml. Failed to parse DOCTYPE."); }
                        }
                        {
                            var pos = i;
                            if(ReadQuotedString(data, ref i, out uri) == false) { throw NewFormatException(data, pos, "Failed to parse DOCTYPE."); }
                        }
                        {
                            var pos = i;
                            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(data, pos, "Unexpected end of xml. Failed to parse DOCTYPE."); }
                        }
                        if(i >= data.Length || data.At(i++) != '>') { throw NewFormatException(data, i, "Unexpected end of xml. DOCTYPE is not closed."); }

                        var body = data.SliceUnsafe(bodyStart, i - bodyStart);
                        var state = new ExternalDtdGetterState_
                        {
                            Body = body,
                            DtdType = ExternalDtdType.Public,
                            PublicIdentifier = publicIdentifier,
                            Uri = uri,
                        };
                        // TODO:
                        DefaultExternalDtdGetter.Instance.GetDtd(new ExternalDtdGetterState(&state));
                        //throw new NotImplementedException($"DTD with PUBLIC identifier is not implemented yet. DtdType: '{state.DtdType}', PI: '{state.PublicIdentifier}', Uri: '{state.Uri}', Body: '{state.Body}'");
                        docType->Body = body;
                        return true;
                    }
                    else {
                        throw NewFormatException(data, i, $"DTD identifier type must be 'SYSTEM' or 'PUBLIC. The identifier is '{identifier}'");
                    }
                }
            }

            static bool ReadQuotedString(RawString data, ref int i, out RawString value)
            {
                if(i >= data.Length) {
                    value = RawString.Empty;
                    return false;
                }
                var quote = data.At(i++);
                if(quote != '"' && quote != '\'') {
                    value = RawString.Empty;
                    return false;
                }

                // "foo"
                // |
                // `--> data[i]

                var start = i;
                while(true) {
                    if(i >= data.Length) {
                        value = RawString.Empty;
                        return false;
                    }
                    if(data.At(i++) == quote) {
                        value = data.SliceUnsafe(start, i - 1 - start);
                        return true;
                    }
                }
            }
        }

        private static bool TryParseCDATA(RawString data, ref int i, in NodeStack nodeStack)
        {
            if(i + 6 < data.Length && data.At(i) == '[' && data.At(i + 1) == 'C' && data.At(i + 2) == 'D' &&
               data.At(i + 3) == 'A' && data.At(i + 4) == 'T' && data.At(i + 5) == 'A' && data.At(i + 6) == '[') {
                // <![CDATA[...]]>
                var nodeStart = i - 2;  // data[nodeStart] == '<'
                i += 7;
                var start = i;
                while(true) {
                    if(i + 2 >= data.Length) { throw NewFormatException(data, nodeStart, "Unexpected end of xml. CDATA is not closed."); }
                    if(data.At(i) == ']' && data.At(i + 1) == ']' && data.At(i + 2) == '>') {
                        i += 3;
                        break;
                    }
                    else {
                        i++;
                    }
                }
                var node = nodeStack.Peek();
                node->InnerText = data.SliceUnsafe(start, i - start - 3);
                return true;
            }
            else {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SkipEmpty(RawString data, ref int i)
        {
            // Skip whitespace, tab, CR and LF.
            // return false if end of data, otherwise true.

            if(i >= data.Length) { return false; }
            if(IsEmptyChar(data.At(i)) == false) { return true; }
            return Loop(data, ref i);

            static bool Loop(RawString data, ref int i)
            {
                i++;
                while(true) {
                    if(i >= data.Length) { return false; }
                    if(IsEmptyChar(data.At(i))) { i++; continue; }
                    return true;
                }
            }
        }

        private static bool SkipComment(RawString data, ref int i)
        {
            // Skip comment <!--xxx-->
            // Current data[i] == '!'
            // return false if end of data, otherwise true.

            Debug.Assert(data.Slice(i, 3).ToString() == "!--");
            i += 3;
            while(true) {
                if(i + 2 >= data.Length) { return false; }
                if(data.At(i) == '-' && data.At(i + 1) == '-' && data.At(i + 2) == '>') // end with "-->"
                {
                    i += 3;
                    return true;
                }
                i++;
            }
        }

        private static void GetXmlDeclaration(RawString data, ref int i, CustomList<XmlAttribute_> attrs, OptionalNodeList optional)
        {
            // Parse <?xml version="1.0" encoding="UTF-8"?>
            // Current data[i] == '?'
            // return false if end of data, otherwise true

            Debug.Assert(data.Slice(i, 5) == "?xml ");
            Debug.Assert(i - 1 >= 0);

            var declaration = optional.Declaration;
            var start = i - 1;
            i += 5;
            while(true) {
                if(i + 1 >= data.Length) {
                    i += 2;
                    throw NewFormatException(data, start, "Xml declaration is not closed.");
                }
                if(data.At(i) == '?' && data.At(i + 1) == '>')   // end with "?>"
                {
                    i += 2;
                    declaration->Body = data.SliceUnsafe(start, i - start);
                    return;
                }
                else {
                    var attr = attrs.GetPointerToAdd(out _);
                    *attr = GetAttr(data, ref i, null);

                    // const utf-8 string. They are embedded in the dll.
                    ReadOnlySpan<byte> version = new byte[] { (byte)'v', (byte)'e', (byte)'r', (byte)'s', (byte)'i', (byte)'o', (byte)'n' };
                    ReadOnlySpan<byte> v1_0 = new byte[] { (byte)'1', (byte)'.', (byte)'0' };
                    ReadOnlySpan<byte> encoding = new byte[] { (byte)'e', (byte)'n', (byte)'c', (byte)'o', (byte)'d', (byte)'i', (byte)'n', (byte)'g' };

                    if(attr->Name.SequenceEqual(version)) {
                        if(attr->Value.SequenceEqual(v1_0) == false) {
                            throw NewFormatException(data, i, "Invalid xml version. it must be '1.0'");
                        }
                        declaration->Version = attr;
                    }
                    if(attr->Name.SequenceEqual(encoding)) {
                        declaration->Encoding = attr;
                    }
                }
            }
        }

        private static void GetInnerText(RawString data, ref int i, out RawString innerText)
        {
            var start = i;
            while(true) {
                if(i + 1 >= data.Length) { throw NewFormatException(data, start, "Unexpected end of xml. Text is not closed."); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == '<') { break; }
            }
            innerText = data.Slice(start, i - start).TrimEnd();
        }

        private static void GetNodeName(RawString data, ref int i, out RawString name)
        {
            var nameStart = i;
            while(true) {
                if(i + 1 >= data.Length) { throw NewFormatException(data, nameStart, "Unexpected end of xml. Failed to parse the node name."); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == ' ' || next == '\t' || next == '\r' || next == '\n' || next == '/' || next == '>') { break; }
            }
            name = data.Slice(nameStart, i - nameStart);
            if(SkipEmpty(data, ref i) == false) {
                throw NewFormatException(data, nameStart + name.Length, "Unexpected end of xml.");
            }
        }

        private static XmlAttribute_ GetAttr(RawString data, ref int i, XmlNode_* node)
        {
            // [NOTE]
            // 'node' is null when the attribute is belonging to the xml declaration.

            // ---------------------------------
            // The Current position is here.
            //
            // <foo aaa="bbb" ...
            //      |
            //      `- data[i]
            //
            // ---------------------------------
            //
            // Empty character may exist at the position of @.
            //
            // <foo aaa@=@"bbb" ...
            //
            // ---------------------------------

            // Get attribute name
            var nameStart = i;
            if(data.At(i++) == '=') {
                // in case of "<foo =...", that is no attribute name.
                throw NewFormatException(data, nameStart, "Unexpected character '='. The attribute name is not found.");
            }

            RawString name;
            while(true) {
                if(i >= data.Length) {
                    throw NewFormatException(data, nameStart, "Unexpected end of xml. The attribute name is not found.");
                }
                var next = data.At(i++);
                if(IsEmptyChar(next)) {
                    int nameLen = i - 1 - nameStart;
                    if(SkipEmpty(data, ref i) == false) {
                        throw NewFormatException(data, nameStart, "Unexpected end of xml. The attribute name is not found.");
                    }
                    var c = data.At(i++);
                    if(c == '=') {
                        name = data.Slice(nameStart, nameLen);
                        break;
                    }
                    throw NewFormatException(data, i - 1, $"Unexpected character appears on parsing an attribute. expected: '=', actual: '{(char)c}'.");
                }
                if(next == '=') {
                    name = data.Slice(nameStart, i - 1 - nameStart);
                    break;
                }
            }

            {
                var pos = i;
                if(SkipEmpty(data, ref i) == false) {
                    throw NewFormatException(data, pos, "Unexpected end of xml. The attribute value is not found.");
                }
            }

            // ---------------------------------
            // The Current position is here. (Empty character may exist at the position of @.)
            //
            // <foo aaa@=@"bbb" ...
            //            |
            //            `- data[i]
            //
            // ---------------------------------

            // Get attribute value
            var quote = data.At(i);     // " or '
            if(quote != '"' && quote != '\'') {
                throw NewFormatException(data, i, $"Invalid character. expected ''' or '\"', actual: '{(char)quote}'.");
            }
            i++;
            if(i >= data.Length) { throw NewFormatException(data, i - 1, "Unexpected end of xml. The attribute value is not closed."); }
            var valueStart = i;
            while(true) {
                if(data.At(i) == quote) { break; }
                i++;
                if(i >= data.Length) {
                    throw NewFormatException(data, valueStart - 1, "Unexpected end of xml. The attribute value is not closed.");
                }
            }
            var value = data.Slice(valueStart, i - valueStart);
            i++;
            {
                var pos = i;
                if(SkipEmpty(data, ref i) == false) {
                    throw NewFormatException(data, pos, "Unexpected end of xml. The element node is not closed.");
                }
            }
            return new XmlAttribute_(name, value, node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEmptyChar(byte c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        private static FormatException NewFormatException(RawString data, int byteOffset, string message)
        {
            var linePos = DataOffsetHelper.GetLinePosition(data.GetPtr(), data.Length, data.GetPtr() + byteOffset);
            if(linePos.HasValue) {
                var (line, pos) = linePos.Value;

                // Use one-based numbering for the message.
                return new FormatException($"(line {line + 1}, char {pos + 1}): {message}");
            }
            return new FormatException(message);
        }
    }
}
