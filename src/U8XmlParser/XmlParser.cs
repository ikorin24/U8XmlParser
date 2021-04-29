#nullable enable
using System;
using System.IO;
using System.Diagnostics;
using U8Xml.Internal;
using System.Text;
using System.Runtime.InteropServices;

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

        /// <summary>Parse xml of <see cref="ReadOnlySpan{char}"/></summary>
        /// <param name="text">text of xml</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(ReadOnlySpan<char> text)
        {
            var buf = default(UnmanagedBuffer);
            try {
                fixed(char* ptr = text) {
                    var byteLen = Encoding.UTF8.GetByteCount(ptr, text.Length);
                    buf = new UnmanagedBuffer(byteLen);
                    Encoding.UTF8.GetBytes(ptr, text.Length, (byte*)buf.Ptr, buf.Length);
                }
                return new XmlObject(ParseCore(ref buf, buf.Length));
            }
            catch {
                buf.Dispose();
                throw;
            }
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
                        utf16LESpan = utf16LESpan.Slice(2);
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

        internal static XmlObjectCore ParseCore(ref UnmanagedBuffer utf8Buf, int length)
        {
            // Remove utf-8 bom
            var offset = utf8Buf.AsSpan(0, 3).SequenceEqual(Utf8BOM) ? 3 : 0;
            var rawString = new RawString((byte*)utf8Buf.Ptr + offset, length - offset);

            var nodes = CustomList<XmlNode_>.Create();
            var attrs = CustomList<XmlAttribute_>.Create();
            var optional = OptionalNodeList.Create();
            try {
                StartStateMachine(rawString, nodes, attrs, optional);
                return new XmlObjectCore(ref utf8Buf, offset, nodes, attrs, optional);
            }
            catch {
                nodes.Dispose();
                attrs.Dispose();
                optional.Dispose();
                throw;
            }
        }

        private static void StartStateMachine(RawString data, CustomList<XmlNode_> nodes, CustomList<XmlAttribute_> attrs, OptionalNodeList optional)
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
                var node = nodeStack.Peek();
                GetInnerText(data, ref i, out node->InnerText);
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
                        if(GetXmlDeclaration(data, ref i, attrs, optional) == false) { throw NewFormatException(); }   // <?xml version="1.0" encoding="UTF-8"?>
                        goto None;
                    }
                    else { throw NewFormatException(); }
                }
                else {
                    GetNodeName(data, ref i, out var name);
                    var node = nodes.GetPointerToAdd();
                    *node = new XmlNode_(name, attrs);
                    while(true) {
                        if(data.At(i) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode_.AddChild(nodeStack.Peek(), node);
                            }
                            nodeStack.Push(node);
                            i++;
                            if(i >= data.Length) { throw NewFormatException(); }
                            goto None;
                        }
                        else if((i + 1 < data.Length) && data.At(i) == '/' && data.At(i + 1) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode_.AddChild(nodeStack.Peek(), node);
                            }
                            i += 2;
                            goto None;
                        }
                        else {
                            var attr = attrs.GetPointerToAdd();
                            *attr = GetAttr(data, ref i);
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
                if(nodeStack.Pop()->Name.SequenceEqual(name) == false) { throw NewFormatException(); }
                if(data.At(i) == '>') {
                    i++;
                    goto None;
                }
                else { throw NewFormatException(); }
            }

        ExtraNode:  // Current data[i] is next char to "<!". (except comment out)
            {
                if(TryParseCDATA(data, ref i, nodeStack)) {
                    goto None;
                }
                else {
                    var bracketCount = 0;
                    while(true) {
                        if(i >= data.Length) { throw NewFormatException(); }
                        if(data.At(i) == '<') { bracketCount++; }
                        else if(data.At(i) == '>') {
                            if(bracketCount == 0) { break; }
                            bracketCount--;
                        }
                        i++;
                    }
                    i++;
                    goto None;
                }
            }

        End:
            {
                Debug.Assert(nodeStack.Count == 0);
                return;
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

        private static bool SkipEmpty(RawString data, ref int i)
        {
            // Skip whitespace, tab, CR and LF.
            // return false if end of data, otherwise true.

            while(true) {
                if(i >= data.Length) { return false; }
                if(data.At(i) == ' ' || data.At(i) == '\t' || data.At(i) == '\n' || data.At(i) == '\r') { i++; continue; }
                return true;
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
                    var attr = attrs.GetPointerToAdd();
                    *attr = GetAttr(data, ref i);

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

        private static XmlAttribute_ GetAttr(RawString data, ref int i)
        {
            // Get attribute name
            var nameStart = i;
            while(true) {
                if(i + 1 >= data.Length) { throw NewFormatException(); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == '=') { break; }
            }
            var name = data.Slice(nameStart, i - nameStart);
            i++;

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
            return new XmlAttribute_(name, value);
        }

        private static FormatException NewFormatException(string? message = null) => new FormatException(message);
    }
}
