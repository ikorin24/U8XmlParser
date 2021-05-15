#nullable enable

#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if !(NETSTANDARD2_0 || NET48 || IS_UNITY)
#define STREAM_SPAN_API
#endif

#if !STREAM_SPAN_API
using System.Buffers;
#endif

using System;
using System.IO;

namespace U8Xml.Internal
{
    internal static class StreamExtension
    {
        public static unsafe (UnmanagedBuffer buffer, int length) ReadAllToUnmanaged(this Stream stream, int fileSizeHint)
        {
            int capacity = Math.Min(int.MaxValue, Math.Max(0, fileSizeHint));       // 0 <= capacity <= int.MaxValue

#if STREAM_SPAN_API
            var buf = new UnmanagedBuffer(capacity);
            var length = 0;
            try {
                while(true) {
                    const int LengthToRead = 4096;
                    if(buf.Length < length + LengthToRead) {
                        var tmp = new UnmanagedBuffer(checked(length + LengthToRead));
                        buf.AsSpan(0, length).CopyTo(tmp.AsSpan());
                        buf.Dispose();
                        buf = tmp;
                    }
                    var readlen = stream.Read(buf.AsSpan(length));
                    length += readlen;
                    if(stream.CanSeek && stream.Position == stream.Length) { break; }
                    if(readlen == 0) { break; }
                }
                return (buf, length);
            }
            catch {
                buf.Dispose();
                throw;
            }
#else
            const int LengthToRead = 4096;
            int bufSize = Math.Min(fileSizeHint, LengthToRead);
            var rentArray = ArrayPool<byte>.Shared.Rent(bufSize);

            var buf = new UnmanagedBuffer(fileSizeHint);
            var totalLen = 0;
            try {
                while(true) {
                    var readlen = stream!.Read(rentArray, 0, bufSize);
                    if(readlen == 0) { break; }

                    if(buf.Length < totalLen + readlen) {
                        ExtendBuffer(ref buf, totalLen);
                    }
                    rentArray.AsSpan(0, readlen).CopyTo(buf.AsSpan(totalLen));
                    totalLen += readlen;

                    if(stream.CanSeek && stream.Position == stream.Length) { break; }
                }
                return (buf, totalLen);
            }
            catch {
                buf.Dispose();
                throw;
            }
            finally {
                ArrayPool<byte>.Shared.Return(rentArray);
            }

            static void ExtendBuffer(ref UnmanagedBuffer buf, int currentUsedLen)
            {
                var newBufSize = buf.Length <= 0 ? 1024 : buf.Length * 2;
                var newBuf = new UnmanagedBuffer(newBufSize);
                try {
                    buf.AsSpan(0, currentUsedLen).CopyTo(newBuf.AsSpan());
                }
                catch {
                    newBuf.Dispose();
                    throw;
                }
                buf.Dispose();
                buf = newBuf;
            }
#endif
        }
    }
}
