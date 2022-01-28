#nullable enable
using System;
using System.IO;
using System.Text;
using U8Xml.Internal;

namespace U8Xml.Unsafes
{
    /// <summary>
    /// [WARNING] DON'T use this if you don't know how to use. The class is hidden.<para/>
    /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
    /// The object returned from the methods MUST BE disposed after you use it.<para/>
    /// </summary>
    public static class XmlParserUnsafe
    {
        /// <summary>
        /// [WARNING] DON'T use this if you don't know how to use. The method is hidden. <para/>
        /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
        /// The object returned from the method MUST BE disposed after you use it.<para/>
        /// </summary>
        /// <param name="utf8Text">utf-8 string to parse</param>
        /// <returns>xml object</returns>
        public static XmlObjectUnsafe ParseUnsafe(ReadOnlySpan<byte> utf8Text)
        {
            var buf = new UnmanagedBuffer(utf8Text);
            try {
                return XmlObjectUnsafe.Create(XmlParser.ParseCore(ref buf, utf8Text.Length));
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        /// <summary>
        /// [WARNING] DON'T use this if you don't know how to use. The method is hidden. <para/>
        /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
        /// The object returned from the method MUST BE disposed after you use it.<para/>
        /// </summary>
        /// <param name="stream">stream to parse</param>
        /// <returns>xml object</returns>
        public static XmlObjectUnsafe ParseUnsafe(Stream stream)
        {
            var fileSizeHint = stream.CanSeek ? (int)stream.Length : 1024 * 1024;
            return ParseUnsafe(stream, fileSizeHint);
        }

        /// <summary>
        /// [WARNING] DON'T use this if you don't know how to use. The method is hidden. <para/>
        /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
        /// The object returned from the method MUST BE disposed after you use it.<para/>
        /// </summary>
        /// <param name="stream">stream to parse</param>
        /// <param name="fileSizeHint">file size hint</param>
        /// <returns>xml object</returns>
        public static XmlObjectUnsafe ParseUnsafe(Stream stream, int fileSizeHint)
        {
            if(stream is null) { ThrowHelper.ThrowNullArg(nameof(stream)); }
            var (buf, length) = stream!.ReadAllToUnmanaged(fileSizeHint);
            try {
                return XmlObjectUnsafe.Create(XmlParser.ParseCore(ref buf, length));
            }
            catch {
                buf.Dispose();
                throw;
            }
        }

        /// <summary>
        /// [WARNING] DON'T use this if you don't know how to use. The method is hidden. <para/>
        /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
        /// The object returned from the method MUST BE disposed after you use it.<para/>
        /// </summary>
        /// <param name="filePath">file path to parse</param>
        /// <returns>xml object</returns>
        public static XmlObjectUnsafe ParseFileUnsafe(string filePath)
        {
            return XmlObjectUnsafe.Create(XmlParser.ParseFileCore(filePath, Encoding.UTF8));
        }

        /// <summary>
        /// [WARNING] DON'T use this if you don't know how to use. The method is hidden. <para/>
        /// *** Memory leaks happen if you use it in the wrong way. ***<para/>
        /// The object returned from the method MUST BE disposed after you use it.<para/>
        /// </summary>
        /// <param name="filePath">file path to parse</param>
        /// <param name="encoding">encoding of the file</param>
        /// <returns>xml object</returns>
        public static XmlObjectUnsafe ParseFileUnsafe(string filePath, Encoding encoding)
        {
            return XmlObjectUnsafe.Create(XmlParser.ParseFileCore(filePath, encoding));
        }
    }
}
