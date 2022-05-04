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
                    else { throw NewFormatException(); }
                }

                // Must be '<', otherwise error.
                if(data.At(i) == '<') {
                    if(nodeStack.Count == 0 && store.NodeCount > 0) {
                        throw NewFormatException("Xml does not have multiple root nodes.");
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
                var node = nodeStack.Peek();
                var textNode = store.AddTextNode(nodeStack.Count, nodeStrPtr);
                GetInnerText(data, ref i, out textNode->InnerText);
                XmlNode_.AddChildTextNode(node, textNode);
                goto None;
            }

        NodeHead:   // Current data[i] is next char to '<'.
            {
                if(data.At(i) == '!') {
                    // Skip comment <!--xxx-->
                    if((i + 2 < data.Length) && (data.At(i + 1) == '-') && (data.At(i + 2) == '-'))  // Start with "<!--"
                    {
                        if(SkipComment(data, ref i) == false) { throw NewFormatException(); }
                        goto None;
                    }
                    else {
                        i++;
                        goto ExtraNode; // extra node. ex)  <!ENTITY st3 "font-family:'Arial';">
                    }
                }
                else if(data.At(i) == '?') {
                    if(i + 4 < data.Length && data.At(i + 1) == 'x' && data.At(i + 2) == 'm' && data.At(i + 3) == 'l' && data.At(i + 4) == ' ') // Start with "<?xml "
                    {
                        if(GetXmlDeclaration(data, ref i, store.AllAttrs, optional) == false) { throw NewFormatException(); }   // <?xml version="1.0" encoding="UTF-8"?>
                        goto None;
                    }
                    else { throw NewFormatException(); }
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
                            if(i >= data.Length) { throw NewFormatException(); }
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
                if(node->Name.SequenceEqual(name) == false) { throw NewFormatException(); }
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
                else { throw NewFormatException(); }
            }

        ExtraNode:  // Current data[i] is next char to "<!". (except comment out)
            {
                if(TryParseCDATA(data, ref i, nodeStack)) {
                    goto None;
                }
                else if(TryParseDocType(data, ref i, store.NodeCount > 0, optional, ref entities)) {
                    goto None;
                }
                else {
                    throw NewFormatException();
                }
            }

        End:
            {
                if(store.NodeCount == 0) {
                    throw NewFormatException("Xml must have at least one node.");
                }
                Debug.Assert(nodeStack.Count == 0);
                return;
            }
        }

        private static bool TryParseDocType(RawString data, ref int i, bool hasNode, OptionalNodeList optional, ref RawStringTable entities)
        {
            // <!DOCTYPE foo[...]>
            ReadOnlySpan<byte> DocTypeStr = stackalloc byte[] { (byte)'D', (byte)'O', (byte)'C', (byte)'T', (byte)'Y', (byte)'P', (byte)'E', (byte)' ' };

            if(data.Slice(i).StartsWith(DocTypeStr) == false) { return false; }
            if(hasNode) {
                ThrowHelper.ThrowFormatException("DTD must be defined before the document root element.");
            }
            if(optional.DocumentType->Body.IsEmpty == false) {
                ThrowHelper.ThrowFormatException("Cannot have multiple DTDs.");
            }


            var bodyStart = i - 2;
            var docType = optional.DocumentType;
            i += DocTypeStr.Length;
            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }

            var nameStart = i;
            SkipUntil((byte)'[', data, ref i);
            var nameLen = i - nameStart - 1;
            var contentStart = i;
            if(nameLen <= 0) { throw NewFormatException(); }
            docType->Name = data.Slice(nameStart, nameLen).TrimEnd();

            using var list = default(RawStringPairList);
            while(true) {
                if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }
                var c = data.At(i++);
                if(c == ']') { break; }
                if(c != '<') { throw NewFormatException(); }
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

                    var j = i;
                    SkipToEmpty(data, ref i);
                    var name = data.SliceUnsafe(j, i - j);
                    if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }

                    var quote = data.At(i);
                    if(quote != '"' && quote != '\'') { throw NewFormatException(); }
                    i++;
                    var k = i;
                    while(true) {
                        i++;
                        if(i >= data.Length) { throw NewFormatException(); }
                        var q = data.At(i);
                        if(q == quote) { break; }
                    }
                    var value = data.SliceUnsafe(k, i - k);
                    i++;
                    list.Add(name, value);

                    SkipUntil((byte)'>', data, ref i);
                    continue;
                }

                SkipUntil((byte)'>', data, ref i);
            }

            docType->InternalSubset = data.Slice(contentStart, i - contentStart - 1);
            SkipUntil((byte)'>', data, ref i);
            docType->Body = data.Slice(bodyStart, i - bodyStart);

            if(list.Count == 0) {
                return true;
            }

            entities = RawStringTable.Create(list.Count);
            for(int l = 0; l < list.Count; l++) {
                ref readonly var item = ref list[l];

                // An entity can refer to another entity that was defined before it.
                if(ContainsAlias(item.Value, out var alias)) {
                    if(entities.TryGetValue(alias, out _) == false) {
                        throw NewFormatException();
                    }
                }

                // Ignore the entity if the key is duplicated.
                entities.TryAdd(item.Key, item.Value);
            }
            return true;

            static void SkipUntil(byte ascii, RawString data, ref int i)
            {
                while(true) {
                    if(i >= data.Length) { throw NewFormatException(); }
                    if(data.At(i++) == ascii) { return; }
                }
            }

            static void SkipToEmpty(RawString data, ref int i)
            {
                while(true) {
                    if(i + 1 >= data.Length) { throw NewFormatException(); }
                    ref var next = ref data.At(i + 1);
                    i++;
                    if(next == ' ' || next == '\t' || next == '\r' || next == '\n') { break; }
                }
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

        private static bool TryParseCDATA(RawString data, ref int i, in NodeStack nodeStack)
        {
            if(i + 6 < data.Length && data.At(i) == '[' && data.At(i + 1) == 'C' && data.At(i + 2) == 'D' &&
               data.At(i + 3) == 'A' && data.At(i + 4) == 'T' && data.At(i + 5) == 'A' && data.At(i + 6) == '[') {
                // <![CDATA[...]]>
                i += 7;
                var start = i;
                while(true) {
                    if(i + 2 >= data.Length) { throw NewFormatException(); }
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

        private static bool GetXmlDeclaration(RawString data, ref int i, CustomList<XmlAttribute_> attrs, OptionalNodeList optional)
        {
            // Parse <?xml version="1.0" encoding="UTF-8"?>
            // Current data[i] == '?'
            // return false if end of data, otherwise true

            Debug.Assert(data.Slice(i, 5).ToString() == "?xml ");
            Debug.Assert(i - 1 >= 0);

            var declaration = optional.Declaration;
            var start = i - 1;
            i += 5;
            while(true) {
                if(i + 1 >= data.Length) {
                    i += 2;
                    return false;
                }
                if(data.At(i) == '?' && data.At(i + 1) == '>')   // end with "?>"
                {
                    i += 2;
                    declaration->Body = data.SliceUnsafe(start, i - start);
                    return true;
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
                            throw NewFormatException("Invalid xml version. it must be '1.0'");
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
                if(i + 1 >= data.Length) { throw NewFormatException(); }
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
                if(i + 1 >= data.Length) { throw NewFormatException(); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == ' ' || next == '\t' || next == '\r' || next == '\n' || next == '/' || next == '>') { break; }
            }
            name = data.Slice(nameStart, i - nameStart);
            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }
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
                throw NewFormatException(); // in case of "<foo =...", that is no attribute name.
            }

            RawString name;
            while(true) {
                if(i >= data.Length) { throw NewFormatException(); }
                var next = data.At(i++);
                if(IsEmptyChar(next)) {
                    int nameLen = i - 1 - nameStart;
                    if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }
                    if(data.At(i++) == '=') {
                        name = data.Slice(nameStart, nameLen);
                        break;
                    }
                    throw NewFormatException();
                }
                if(next == '=') {
                    name = data.Slice(nameStart, i - 1 - nameStart);
                    break;
                }
            }
            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }

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
            if(quote != '"' && quote != '\'') { throw NewFormatException(); }
            i++;
            if(i >= data.Length) { throw NewFormatException(); }
            var valueStart = i;
            while(true) {
                if(data.At(i) == quote) { break; }
                i++;
                if(i >= data.Length) { throw NewFormatException(); }
            }
            var value = data.Slice(valueStart, i - valueStart);
            i++;
            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }
            return new XmlAttribute_(name, value, node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEmptyChar(byte c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        private static FormatException NewFormatException(string? message = null) => new FormatException(message);
        private static FormatException NewFormatException(string message, int line, int pos)
        {
            return new FormatException($"line {line}, position {pos}: {message}");
        }
    }
}
