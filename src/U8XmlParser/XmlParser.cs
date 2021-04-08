#nullable enable
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using U8Xml.Internal;

namespace U8Xml
{
    /// <summary>Parser for xml file</summary>
    public static unsafe class XmlParser
    {
        /// <summary>Byte Order Mark of utf-8 with bom</summary>
        internal static ReadOnlySpan<byte> Utf8BOM => new byte[] { 0xEF, 0xBB, 0xBF };   // Bytes are embedded in dll, so there are no heap allocation.

        /// <summary>Parse xml file encoded as UTF8.</summary>
        /// <param name="utf8String">utf-8 byte span data</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(ReadOnlySpan<byte> utf8String)
        {
            var buf = new UnmanagedBuffer(utf8String);
            try {
                return ParseCore(ref buf, utf8String.Length);
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        /// <summary>Parse xml file encoded as UTF8.</summary>
        /// <param name="stream">stream to read</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream)
        {
            var fileSizeHint = stream.CanSeek ? (int)stream.Length : 1024 * 1024;
            return Parse(stream, fileSizeHint);
        }

        /// <summary>Parse xml file</summary>
        /// <param name="stream">stream to read</param>
        /// <param name="fileSizeHint">file size hint which is used for optimizing memory</param>
        /// <returns>xml object</returns>
        public static XmlObject Parse(Stream stream, int fileSizeHint)
        {
            if(stream is null) { ThrowHelper.ThrowNullArg(nameof(stream)); }
            var (buf, length) = stream!.ReadAllToUnmanaged(fileSizeHint);
            try {
                return ParseCore(ref buf, length);
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        private static XmlObject ParseCore(ref UnmanagedBuffer buf, int length)
        {
            // Remove utf-8 bom
            var offset = buf.AsSpan(0, 3).SequenceEqual(Utf8BOM) ? 3 : 0;
            var rawString = new RawString((byte*)buf.Ptr + offset, length - offset);

            var nodes = CustomList<XmlNode>.Create();
            var attrs = CustomList<XmlAttribute>.Create();
            try {
                StartStateMachine(rawString, nodes, attrs);
                return new XmlObject(ref buf, offset, nodes, attrs);
            }
            catch {
                nodes.Dispose();
                attrs.Dispose();
                throw;
            }
        }

        private static void StartStateMachine(RawString data, CustomList<XmlNode> nodes, CustomList<XmlAttribute> attrs)
        {
            // Encoding assumes utf-8 without bom. Others are not supported.
            // Parse format by using a state machine. (It's very primitive but fastest.)
            int i = 0;
            using var nodeStack = new NodeStack(capacity: 32);

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
                else { goto InlineContent; }
            }

        InlineContent:
            {
                var node = nodeStack.Peek();
                Unsafe.AsRef(node->Content) = GetInlineContent(data, ref i);
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
                    else { goto ExtraNode; }    // extra node. ex)  <!ENTITY st3 "font-family:'Arial';">
                }
                else if(data.At(i) == '?') {
                    if(i + 4 < data.Length && data.At(i + 1) == 'x' && data.At(i + 2) == 'm' && data.At(i + 3) == 'l' && data.At(i + 4) == ' ') // Start with "<?xml "
                    {
                        if(SkipXmlDeclaration(data, ref i) == false) { throw NewFormatException(); }   // Skip <?xml version="1.0" encoding="UTF-8"?>
                        goto None;
                    }
                    else { throw NewFormatException(); }
                }
                else {
                    var node = nodes.Add(new XmlNode(GetNodeName(data, ref i), attrs));
                    while(true) {
                        if(data.At(i) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode.AddChild(nodeStack.Peek(), node);
                            }
                            nodeStack.Push(node);
                            i++;
                            if(i >= data.Length) { throw NewFormatException(); }
                            goto None;
                        }
                        else if((i + 1 < data.Length) && data.At(i) == '/' && data.At(i + 1) == '>') {
                            if(nodeStack.Count > 0) {
                                XmlNode.AddChild(nodeStack.Peek(), node);
                            }
                            i += 2;
                            goto None;
                        }
                        else {
                            var attr = attrs.Add(GetAttr(data, ref i));
                            if(node->HasAttribute == false) {
                                Unsafe.AsRef(node->AttrIndex) = attrs.Count - 1;
                            }
                            Unsafe.AsRef(node->AttrCount)++;
                        }
                    }
                }
            }

        NodeTail:
            {
                var name = GetNodeName(data, ref i);
                if(nodeStack.Pop()->Name.SequenceEqual(name) == false) { throw NewFormatException(); }
                if(data.At(i) == '>') {
                    i++;
                    goto None;
                }
                else { throw NewFormatException(); }
            }

        ExtraNode:  // Current data[i] is next char to "<!". (except comment out)
            {
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

        End:
            {
                Debug.Assert(nodeStack.Count == 0);
                return;
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

        private static bool SkipXmlDeclaration(RawString data, ref int i)
        {
            // Skip <?xml version="1.0" encoding="UTF-8"?>
            // Current data[i] == '?'
            // return false if end of data, otherwise true

            Debug.Assert(data.Slice(i, 5).ToString() == "?xml ");
            i += 5;
            while(true) {
                if(i + 1 >= data.Length) {
                    i += 2;
                    return false;
                }
                if(data.At(i) == '?' && data.At(i + 1) == '>')   // end with "?>"
                {
                    i += 2;
                    return true;
                }
                i++;
            }
        }

        private static RawString GetInlineContent(RawString data, ref int i)
        {
            var start = i;
            while(true) {
                if(i + 1 >= data.Length) { throw NewFormatException(); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == '<') { break; }
            }
            var content = data.Slice(start, i - start).TrimEnd();
            return content;
        }

        private static RawString GetNodeName(RawString data, ref int i)
        {
            var nameStart = i;
            while(true) {
                if(i + 1 >= data.Length) { throw NewFormatException(); }
                ref var next = ref data.At(i + 1);
                i++;
                if(next == ' ' || next == '\t' || next == '\r' || next == '\n' || next == '/' || next == '>') { break; }
            }
            var name = data.Slice(nameStart, i - nameStart);
            if(SkipEmpty(data, ref i) == false) { throw NewFormatException(); }
            return name;
        }

        private static XmlAttribute GetAttr(RawString data, ref int i)
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
            return new XmlAttribute(name, value);
        }

        private static FormatException NewFormatException(string? message = null) => new FormatException(message);
    }
}
