#nullable enable
#if !(NETSTANDARD2_0 || NET48)
#define STREAM_SPAN_API
#endif

#if !STREAM_SPAN_API
using System.Buffers;
#endif

using System;
using System.IO;
using System.Linq;
using UnmanageUtility;

namespace U8Xml.Internal
{
    internal static class StreamExtension
    {
        public static unsafe (UnmanagedList<byte> data, RawString rawString) ReadAllToUnmanaged(this Stream stream, long fileSizeHint)
        {
            int capacity = (int)Math.Min(int.MaxValue, Math.Max(0, fileSizeHint));       // 0 <= capacity <= int.MaxValue

#if STREAM_SPAN_API
            var buf = new UnmanagedList<byte>(capacity);
            try
            {
                var length = 0;
                while (true)
                {
                    var readlen = stream.Read(buf.Extend(4096, false));
                    length += readlen;
                    if (readlen == 0) { break; }
                }
                // Remove utf-8 bom
                var offset = buf.AsSpan(0, 3).SequenceEqual(XmlParser.Utf8BOM) ? 3 : 0;
                var rawString = new RawString((byte*)buf.Ptr + offset, length - offset);
                return (buf, rawString);
            }
            catch
            {
                buf.Dispose();
                throw;
            }
#else
            int bufSize = (int)Math.Min(fileSizeHint, 1024 * 1024);     // 1024 * 1024 is max length of pooled array
            var rentArray = ArrayPool<byte>.Shared.Rent(bufSize);
            var buf = new UnmanagedList<byte>(0);
            try
            {
                while (true)
                {
                    var readlen = stream!.Read(rentArray, 0, rentArray.Length);
                    if (readlen == 0) { break; }
                    buf.AddRange(rentArray.AsSpan(0, readlen));
                }
                // Remove utf-8 bom
                var offset = buf.AsSpan(0, 3).SequenceEqual(XmlParser.Utf8BOM) ? 3 : 0;
                var rawString = new RawString((byte*)buf.Ptr + offset, buf.Count - offset);
                return (buf, rawString);
            }
            catch
            {
                buf.Dispose();
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentArray);
            }
#endif
        }
    }
}
